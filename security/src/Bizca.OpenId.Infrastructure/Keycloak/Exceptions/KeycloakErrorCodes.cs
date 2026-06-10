namespace Bizca.OpenId.Infrastructure.Keycloak.Exceptions;

public enum KeycloakErrorCodes
{
	[KeycloakException("Invalid client credentials", 401)]
	InvalidClientCredentials,

	[KeycloakException("Invalid authorization code", 400)]
	InvalidAuthorizationCode,

	[KeycloakException("Invalid refresh token", 400)]
	InvalidRefreshToken,

	[KeycloakException("User not found", 404)]
	UserNotFound,

	[KeycloakException("Invalid token", 401)]
	InvalidToken
}