using Bizca.Sdk.SharedKernel;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bizca.OpenId.Infrastructure.Keycloak.SigningKeys;

/// <summary>
/// Decorator that caches signing keys with automatic expiration.
/// Single Responsibility: cache management and expiration strategy only.
/// </summary>
internal sealed class CachedSigningKeySource(
	ISigningKeySource inner,
	IDateTimeProvider dateTimeProvider,
	IOptions<KeycloakOptions> options) : ISigningKeySource
{
	private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(options.Value.JwksCacheDurationSeconds);
    private ICollection<SecurityKey>? _cachedKeys;
    private DateTime _lastRefresh = DateTime.MinValue;

	/// <summary>
    /// Gets signing keys from cache if valid, otherwise fetches and updates cache.
    /// </summary>
    public async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = dateTimeProvider.UtcNow;
        if (!IsCacheExpired(utcNow))
        {
            return _cachedKeys!;
        }

        var keys = await inner.GetSigningKeysAsync(cancellationToken);
        _cachedKeys = keys;
        _lastRefresh = utcNow;

        return keys;
    }

    /// <summary>
    /// Refreshes the signing keys.
    /// </summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await inner.RefreshAsync(cancellationToken);
        var keys = await inner.GetSigningKeysAsync(cancellationToken);
        _cachedKeys = keys;
        _lastRefresh = DateTime.UtcNow;
    }

    private bool IsCacheExpired(DateTime now)
        => _cachedKeys is null || now - _lastRefresh >= _cacheDuration;
}



