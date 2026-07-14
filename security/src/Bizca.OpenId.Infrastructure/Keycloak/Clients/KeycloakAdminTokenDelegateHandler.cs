using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;
using Bizca.OpenId.Infrastructure.Keycloak.Exceptions;
using Bizca.OpenId.Infrastructure.Keycloak.Models;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class KeycloakAdminTokenDelegateHandler(ITokenRequestBuilder tokenRequestBuilder) : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		var adminToken = await GetAdminAccessTokenAsync(cancellationToken);
		request.AddAuthorizationHeader(adminToken);
		return await base.SendAsync(request, cancellationToken);
	}

	private async Task<string> GetAdminAccessTokenAsync(CancellationToken cancellationToken = default)
	{
		var parameters = tokenRequestBuilder.BuildClientCredentialsRequest();
		var content = new FormUrlEncodedContent(parameters);

		using var request = new HttpRequestMessage(HttpMethod.Post, OAuth2KeycloakConstants.Endpoints.Token);
		request.Content = content;

		var response = await base.SendAsync(request, cancellationToken);
		if (response.IsSuccessStatusCode)
		{
			return (await response.Content.ReadFromJsonAsync<TokenResult>(cancellationToken))!.AccessToken;
		}

		var errorResult = await response.Content.ReadFromJsonAsync<ErrorResult>(cancellationToken);
		throw new KeycloakException(errorResult!.Error!, errorResult.ErrorDescription, (int)response.StatusCode);
	}
}