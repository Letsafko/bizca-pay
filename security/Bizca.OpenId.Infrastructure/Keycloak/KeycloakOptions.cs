namespace Bizca.OpenId.Infrastructure.Keycloak;

/// <summary>
/// Configuration options for Keycloak integration.
/// </summary>
public sealed class KeycloakOptions
{
	public const string SectionName = "KeycloakOptions";

    /// <summary>
    /// The Keycloak server authority URL (e.g., https://keycloak.example.com/realms/bizca).
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// The OAuth2 client identifier.
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// The OAuth2 client secret.
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// The Keycloak realm name.
    /// </summary>
    public string Realm { get; set; } = string.Empty;

    /// <summary>
    /// OAuth2 scopes to request (e.g., "openid profile email").
    /// </summary>
    public string Scopes { get; set; } = "openid profile email";

    /// <summary>
    /// JWKS endpoint cache duration in seconds (default: 3600 = 1 hour).
    /// </summary>
    public int JwksCacheDurationSeconds { get; init; } = 3600;

    /// <summary>
    /// HTTP timeout for Keycloak API calls in seconds (default: 30).
    /// </summary>
    public int HttpTimeoutSeconds { get; init; } = 30;
}

