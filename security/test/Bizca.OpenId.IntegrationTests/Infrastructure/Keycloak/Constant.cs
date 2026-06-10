namespace Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;

internal static class Constant
{
	internal const string HttpClientName = "KeycloakAdmin";
	internal static class OpenIdProvider
	{
		internal static class Keycloak
		{
			internal const string ClientSecret = "test-secret";
			internal const string ClientId = "bizca-backend";
			internal const string AdminPassword = "admin";
			internal const string AdminUser = "admin";
			internal const string RealmName = "bizca";
		}
	}
}