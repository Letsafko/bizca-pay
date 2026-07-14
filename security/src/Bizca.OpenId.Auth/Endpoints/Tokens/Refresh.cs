using System.Threading;
using Bizca.OpenId.ApiModels.Requests;
using Bizca.OpenId.Application.Models;
using Bizca.Sdk.Api.MinimalApi;
using Bizca.Sdk.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bizca.OpenId.Auth.Endpoints.Tokens;

using RefreshTokenCommand = Bizca.OpenId.Application.Usecases.Tokens.Refresh.Command;

public static class Refresh
{
	public class Endpoint : IEndpoint
	{
		public void MapEndpoint(IEndpointRouteBuilder app)
		{
			app.MapPost("/auth/refresh", async (
				RefreshTokenRequest refreshTokenRequest,
				IRequestHandler<RefreshTokenCommand, TokenResponse> handler,
				CancellationToken cancellationToken) =>
			{
				var command = refreshTokenRequest.ToCommand();
				var result = await handler.HandleAsync(command, cancellationToken);
				return result.Match(
					onSuccess: x => Results.Ok(x.ToViewModel()),
					onFailure: CustomResults.Problem);
			})
			.WithName("RefreshToken")
			.WithTags(Tags.Authentication)
			.WithSummary("Refresh an access token")
			.WithDescription("Exchanges a refresh token for a new access token.")
			.Produces<TokenResponse>()
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
		}
	}

	private static RefreshTokenCommand? ToCommand(this RefreshTokenRequest? request)
	{
		return request is null
			? null
			: new RefreshTokenCommand(request.RefreshToken);
	}
}