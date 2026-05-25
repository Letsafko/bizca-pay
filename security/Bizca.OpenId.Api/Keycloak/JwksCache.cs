using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Bizca.OpenId.Api.Options;

namespace Bizca.OpenId.Api.Keycloak;

/// <summary>
/// Caches JWKS (JSON Web Key Set) with automatic refresh on key rotation.
/// Thread-safe singleton service.
/// </summary>
public sealed class JwksCache : IDisposable
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private OpenIdConnectConfiguration? _cachedConfiguration;
    private DateTime _lastRefresh = DateTime.MinValue;
    private readonly TimeSpan _cacheDuration;
    private bool _disposed;

    public JwksCache(IOptions<KeycloakOptions> options)
    {
        var keycloakOptions = options.Value;
        _cacheDuration = TimeSpan.FromSeconds(keycloakOptions.JwksCacheDurationSeconds);

        var documentRetriever = new HttpDocumentRetriever
        {
            RequireHttps = keycloakOptions.Authority.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
        };

        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress: $"{keycloakOptions.Authority.TrimEnd('/')}/.well-known/openid-configuration",
            configRetriever: new OpenIdConnectConfigurationRetriever(),
            docRetriever: documentRetriever
        );
    }

    /// <summary>
    /// Gets the current signing keys, refreshing from the JWKS endpoint if the cache is expired.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection of signing keys.</returns>
    public async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var now = DateTime.UtcNow;

        // Fast path: cache is still valid
        if (_cachedConfiguration is not null && now - _lastRefresh < _cacheDuration)
        {
            return _cachedConfiguration.SigningKeys;
        }

        // Slow path: refresh required
        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Double-check after acquiring lock
            if (_cachedConfiguration is not null && now - _lastRefresh < _cacheDuration)
            {
                return _cachedConfiguration.SigningKeys;
            }

            // Force refresh if cache expired
            var shouldRefresh = _cachedConfiguration is null || now - _lastRefresh >= _cacheDuration;
            var configuration = await _configurationManager.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);

            if (shouldRefresh)
            {
                _configurationManager.RequestRefresh();
            }

            _cachedConfiguration = configuration;
            _lastRefresh = now;

            return configuration.SigningKeys;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Forces an immediate refresh of the JWKS cache (e.g., after a key rotation).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            _configurationManager.RequestRefresh();
            var configuration = await _configurationManager.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
            _cachedConfiguration = configuration;
            _lastRefresh = DateTime.UtcNow;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        if (_disposed)
		{
			return;
		}

		_lock.Dispose();
        _disposed = true;
    }
}


