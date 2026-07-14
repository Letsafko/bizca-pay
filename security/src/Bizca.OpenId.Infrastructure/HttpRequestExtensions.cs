using System.Net.Http;
using System.Net.Http.Headers;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;

namespace Bizca.OpenId.Infrastructure;

internal static class HttpRequestExtensions
{
	internal static void AddAuthorizationHeader(this HttpRequestMessage request, string accessToken)
	{
		request.Headers.Authorization = new AuthenticationHeaderValue(
			OAuth2KeycloakConstants.AuthorizationBearerScheme,
			accessToken);
	}
}