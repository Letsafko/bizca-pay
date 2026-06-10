using System.Threading;
using Bizca.OpenId.ApiModels.Requests;
using Bizca.OpenId.ApiModels.Responses;
using Bizca.OpenId.Application.Models;
using Bizca.Sdk.Api.MinimalApi;
using Bizca.Sdk.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bizca.OpenId.Server.Endpoints.Tokens;

using TokenExchangeCommand = Bizca.OpenId.Application.Usecases.Tokens.Exchange.Command;

public static class Create
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
				var command = exchangeTokenRequest.ToCommand();
				var result = await handler.HandleAsync(command, cancellationToken);

				return result.Match(
					onSuccess: x => Results.Ok(x.ToViewModel()),
					onFailure: CustomResults.Problem);
			})
			.WithName("Create")
			.WithTags(Tags.Authentication)
			.WithSummary("Create access token from either authorization code or client credentials flow.")
			.WithDescription("Supports authorization code + PKCE and client credentials flows.")
			.Produces<TokenViewModel>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
		}
	}

	private static TokenExchangeCommand? ToCommand(this ExchangeTokenRequest? request)
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
