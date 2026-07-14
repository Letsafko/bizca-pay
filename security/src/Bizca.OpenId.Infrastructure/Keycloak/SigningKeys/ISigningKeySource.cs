using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Bizca.OpenId.Infrastructure.Keycloak.SigningKeys;

/// <summary>
/// Abstracts access to signing keys for JWT validation.
/// Implementations may cache, synchronize, or fetch from remote sources.
/// </summary>
public interface ISigningKeySource
{
    /// <summary>
    /// Gets the current collection of signing keys.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The collection of signing keys.</returns>
    Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the signing keys from the source.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RefreshAsync(CancellationToken cancellationToken = default);
}


