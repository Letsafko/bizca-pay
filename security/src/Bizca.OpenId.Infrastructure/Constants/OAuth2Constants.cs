namespace Bizca.OpenId.Infrastructure.Constants;

public static class OAuth2Constants
{
	public const string AuthenticationScheme = "Bearer";
	public const string Keycloak = "Keycloak";

	internal static class ParameterNames
    {
        public const string TokenTypeHint = "token_type_hint";
        public const string ClientSecret = "client_secret";
        public const string CodeVerifier = "code_verifier";
        public const string RefreshToken = "refresh_token";
        public const string RedirectUri = "redirect_uri";
        public const string GrantType = "grant_type";
        public const string ClientId = "client_id";
        public const string Scope = "scope";
        public const string Token = "token";
        public const string Code = "code";
    }

	internal static class Endpoints
    {
        public const string UserInfo = "protocol/openid-connect/userinfo";
        public const string Revoke = "protocol/openid-connect/revoke";
        public const string Token = "protocol/openid-connect/token";
    }
}
