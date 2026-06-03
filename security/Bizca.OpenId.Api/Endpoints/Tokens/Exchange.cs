using System.Threading;
using Bizca.OpenId.Api.Endpoints.Shared;
using Bizca.OpenId.ApiModels.Requests;
using Bizca.OpenId.ApiModels.Responses;
using Bizca.OpenId.Application.Models;
using Bizca.Sdk.Api.MinimalApi;
using Bizca.Sdk.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bizca.OpenId.Api.Endpoints.Tokens;

using TokenExchangeCommand = Bizca.OpenId.Application.Usecases.Tokens.Exchange.Command;

public static class Exchange
{
	public sealed class Endpoint : IEndpoint
	{
		public void MapEndpoint(IEndpointRouteBuilder app)
		{
			app.MapPost("/auth/token", async(
				ExchangeTokenRequest exchangeTokenRequest,
				IRequestHandler<TokenExchangeCommand, TokenResponse> handler,
				CancellationToken cancellationToken) =>
			{
				var query = exchangeTokenRequest.ToQuery();
				var result = await handler.HandleAsync(query, cancellationToken);
				return result.Match(
					onSuccess: x => Results.Ok(x.ToViewModel()),
					onFailure: CustomResults.Problem);
			})
			.WithName("Exchange")
			.WithTags(Tags.Authentication)
			.WithSummary("Exchange of either authorization code or client credentials for access token")
			.WithDescription("Supports authorization code + PKCE and client credentials flows.")
			.Produces<TokenViewModel>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
		}
	}

	private static TokenExchangeCommand? ToQuery(this ExchangeTokenRequest? request)
	{
		return request is null
			? null
			: new TokenExchangeCommand(
				request.GrantType,
				request.Code,
				request.RedirectUri,
				request.CodeVerifier);
	}
}
