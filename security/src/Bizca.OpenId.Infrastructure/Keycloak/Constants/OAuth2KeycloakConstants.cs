using System.Text;

namespace Bizca.OpenId.Infrastructure.Keycloak.Constants;

public static class OAuth2KeycloakConstants
{
	public const string AuthorizationScheme = "Authorization";
	public const string AuthorizationBearerScheme = "Bearer";
	public const string KeycloakClientNameAdmin = "KeycloakAdmin";
	public const string KeycloakClientName = "Keycloak";

	public static class ParameterNames
    {
        public const string TokenTypeHint = "token_type_hint";
        public const string ClientSecret = "client_secret";
        public const string CodeVerifier = "code_verifier";
        public const string RefreshToken = "refresh_token";
        public const string RedirectUri = "redirect_uri";
        public const string GrantType = "grant_type";
        public const string ClientId = "client_id";
		public const string Username = "username";
		public const string Password = "password";
        public const string Scope = "scope";
        public const string Token = "token";
        public const string Code = "code";
    }

	public static class GrantTypes
	{
		public const string AuthorizationCode = "authorization_code";
		public const string ClientCredentials = "client_credentials";
		public const string RefreshToken = "refresh_token";
		public const string Password = "password";
	}

	internal static class Endpoints
	{
        public const string UserInfo = "protocol/openid-connect/userinfo";
        public const string Revoke = "protocol/openid-connect/revoke";
        public const string Token = "protocol/openid-connect/token";

		public static class Admin
		{
			internal static readonly CompositeFormat UpdateEmailVerificationCompositeFormat = CompositeFormat.Parse(
				"admin/realms/{0}/users/{1}");

			internal static readonly CompositeFormat SendUserEmailVerificationCompositeFormat = CompositeFormat.Parse(
				"admin/realms/{0}/users/{1}/execute-actions-email");

			internal static readonly CompositeFormat UpdateUserEnabledCompositeFormat = CompositeFormat.Parse(
				"admin/realms/{0}/users/{1}");

			internal static readonly CompositeFormat CreateUserCompositeFormat = CompositeFormat.Parse(
					"admin/realms/{0}/users");
		}
    }
}
