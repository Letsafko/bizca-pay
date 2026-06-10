using System.Threading;
using Bizca.OpenId.ApiModels.Requests;
using Bizca.OpenId.ApiModels.Responses;
using Bizca.Sdk.Api.MinimalApi;
using Bizca.Sdk.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bizca.OpenId.Server.Endpoints.Auth;

using VerifyEmailCommand = Bizca.OpenId.Application.Usecases.Auth.VerifyEmail.Command;
using VerifyEmailResponse = Bizca.OpenId.Application.Usecases.Auth.VerifyEmail.Response;

public static class VerifyEmail
{
	public sealed class Endpoint : IEndpoint
	{
		public void MapEndpoint(IEndpointRouteBuilder app)
		{
			app.MapPost("/auth/email/verify", async (
				VerifyEmailRequest verifyEmailRequest,
				IRequestHandler<VerifyEmailCommand, VerifyEmailResponse> handler,
				CancellationToken cancellationToken) =>
			{
				var command = verifyEmailRequest.ToCommand();
				var result = await handler.HandleAsync(command, cancellationToken);

				return result.Match(
					onSuccess: x => Results.Ok(x.ToViewModel()),
					onFailure: CustomResults.Problem);
			})
			.WithName("VerifyEmail")
			.WithTags(Tags.Auth)
			.WithSummary("Verify user email address")
			.WithDescription("Validates the email verification token and activates the user account.")
			.Produces<VerifyEmailViewModel>(StatusCodes.Status200OK)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
		}
	}

	private static VerifyEmailCommand? ToCommand(this VerifyEmailRequest? request)
	{
		return request is null
			? null
			: new VerifyEmailCommand(request.Token);
	}

	private static VerifyEmailViewModel ToViewModel(this VerifyEmailResponse response)
	{
		return new VerifyEmailViewModel
		{
			Success = response.Success,
			Message = "Email verified successfully. Your account is now active."
		};
	}
}

