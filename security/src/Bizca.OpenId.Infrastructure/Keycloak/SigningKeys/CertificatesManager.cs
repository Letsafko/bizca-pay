using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Bizca.OpenId.Infrastructure.Keycloak.SigningKeys;

/// <summary>
/// Thread-safe JWKS cache with automatic refresh on key rotation.
/// Orchestrates composition of Keycloak source + caching + synchronization decorators.
/// This class maintains backward compatibility while delegating to SOLID implementations.
/// </summary>
public sealed class CertificatesManager(ISigningKeySource signingKeySource)
{
	/// <summary>
    /// Gets the current signing keys, refreshing from the JWKS endpoint if the cache is expired.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection of signing keys.</returns>
    public async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken = default)
        => await signingKeySource.GetSigningKeysAsync(cancellationToken);

    /// <summary>
    /// Refreshes the signing keys.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
        => await signingKeySource.RefreshAsync(cancellationToken);
}
