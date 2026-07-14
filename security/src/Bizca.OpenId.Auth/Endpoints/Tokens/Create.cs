using System.Threading;
using Bizca.OpenId.ApiModels.Requests;
using Bizca.OpenId.ApiModels.Responses;
using Bizca.OpenId.Application.Models;
using Bizca.Sdk.Api.MinimalApi;
using Bizca.Sdk.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bizca.OpenId.Auth.Endpoints.Tokens;

using CreateTokenCommand = Bizca.OpenId.Application.Usecases.Tokens.Create.Command;

public static class Create
{
	public sealed class Endpoint : IEndpoint
	{
		public void MapEndpoint(IEndpointRouteBuilder app)
		{
			app.MapPost("/auth/token", async(
				CreateTokenRequest createTokenRequest,
				IRequestHandler<CreateTokenCommand, TokenResponse> handler,
				CancellationToken cancellationToken) =>
			{
				var command = createTokenRequest.ToCommand();
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

	private static CreateTokenCommand? ToCommand(this CreateTokenRequest? request)
	{
		return request is null
			? null
			: new CreateTokenCommand(
				request.GrantType,
				request.Code,
				request.RedirectUri,
				request.CodeVerifier);
	}
}
