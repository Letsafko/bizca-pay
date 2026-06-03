using System.Threading;
using Bizca.OpenId.Api.Endpoints.Shared;
using Bizca.OpenId.ApiModels.Requests;
using Bizca.OpenId.ApiModels.Responses;
using Bizca.Sdk.Api.MinimalApi;
using Bizca.Sdk.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bizca.OpenId.Api.Endpoints.Tokens;

using LogoutCommand = Bizca.OpenId.Application.Usecases.Tokens.Logout.Command;
using LogoutResponse = Bizca.OpenId.Application.Usecases.Tokens.Logout.Response;

public static class Logout
{
	public sealed class Endpoint : IEndpoint
	{
		public void MapEndpoint(IEndpointRouteBuilder app)
		{
			app.MapPost("/auth/logout", async (
				LogoutRequest logoutRequest,
				IRequestHandler<LogoutCommand, LogoutResponse> handler,
				CancellationToken cancellationToken) =>
			{
				var command = logoutRequest.ToCommand();
				var result = await handler.HandleAsync(command, cancellationToken);
				return result.Match(
					onSuccess: x => Results.Ok(x.ToResponse()),
					onFailure: CustomResults.Problem);
			})
			.WithName("Logout")
			.WithTags(Tags.Authentication)
			.WithSummary("Revoke a token (logout)")
			.WithDescription("Revokes an access or refresh token, invalidating the session.")
			.Produces(StatusCodes.Status204NoContent)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
		}
	}

	private static LogoutCommand? ToCommand(this LogoutRequest? request)
	{
		return request is null
			? null
			: new LogoutCommand(request.Token, request.TokenTypeHint);
	}

	private static LogoutViewModel ToResponse(this LogoutResponse response)
	{
		return new LogoutViewModel
		{
			Revoked = response.Success
		};
	}
}