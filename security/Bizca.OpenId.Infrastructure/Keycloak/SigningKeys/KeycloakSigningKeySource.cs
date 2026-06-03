using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Bizca.OpenId.Infrastructure.Keycloak.SigningKeys;

/// <summary>
/// Fetches signing keys directly from Keycloak's JWKS endpoint.
/// </summary>
internal sealed class KeycloakSigningKeySource : ISigningKeySource
{
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

    public KeycloakSigningKeySource(IOptions<KeycloakOptions> options)
    {
        var keycloakOptions = options.Value;
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
    /// Fetches the latest signing keys from Keycloak.
    /// </summary>
    public async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken = default)
    {
        var configuration = await _configurationManager.GetConfigurationAsync(cancellationToken);
        return configuration.SigningKeys;
    }

    /// <summary>
    /// Refreshes the signing keys from Keycloak.
    /// </summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        _configurationManager.RequestRefresh();
        await _configurationManager.GetConfigurationAsync(cancellationToken);
    }
}


