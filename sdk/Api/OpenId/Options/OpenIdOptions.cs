namespace Bizca.Sdk.Api.OpenId.Options;
/// <summary>
/// Configuration options for Bizca OpenID Connect JWT validation.
/// </summary>
public sealed class OpenIdOptions
{
    /// <summary>
    /// The expected token issuer (e.g., https://keycloak.example.com/realms/bizca).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>
    /// The expected token audience (typically the client_id of the API Gateway).
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>
    /// The OIDC authority URL for JWKS discovery (same as Issuer by default).
    /// </summary>
    public string Authority { get; set; } = string.Empty;
    /// <summary>
    /// Whether to require HTTPS for the authority metadata endpoint (default: true).
    /// </summary>
    public bool RequireHttpsMetadata { get; init; } = true;
    /// <summary>
    /// Clock skew tolerance in seconds for token expiration validation (default: 300 = 5 minutes).
    /// </summary>
    public int ClockSkewSeconds { get; init; } = 300;
}
