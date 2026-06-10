namespace Bizca.Services.AppHost.Constants;

/// <summary>
/// Configuration keys for reading from appsettings.json or User Secrets.
/// </summary>
internal static class ConfigurationKeys
{
    public static class Keycloak
    {
        public const string AdminUser = "Keycloak:AdminUser";
        public const string AdminPassword = "Keycloak:AdminPassword";
    }
}
