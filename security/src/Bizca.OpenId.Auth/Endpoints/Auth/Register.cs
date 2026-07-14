using System.Threading;
using Bizca.OpenId.ApiModels.Requests;
using Bizca.OpenId.ApiModels.Responses;
using Bizca.Sdk.Api.MinimalApi;
using Bizca.Sdk.SharedKernel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Bizca.OpenId.Auth.Endpoints.Auth;

using RegisterCommand = Bizca.OpenId.Application.Usecases.Auth.Register.Command;
using RegisterResponse = Bizca.OpenId.Application.Usecases.Auth.Register.Response;

public static class Register
{
	public sealed class Endpoint : IEndpoint
	{
		public void MapEndpoint(IEndpointRouteBuilder app)
		{
			app.MapPost("/auth/register", async (
				RegisterRequest registerRequest,
				IRequestHandler<RegisterCommand, RegisterResponse> handler,
				CancellationToken cancellationToken) =>
			{
				var command = registerRequest.ToCommand();
				var result = await handler.HandleAsync(command, cancellationToken);

				return result.Match(
					onSuccess: x => Results.Created($"/auth/users/{x.UserId}", x.ToViewModel()),
					onFailure: CustomResults.Problem);
			})
			.WithName("Register")
			.WithTags(Tags.Auth)
			.WithSummary("Register a new user")
			.WithDescription("Creates a new user identity and sends email verification if enabled.")
			.Produces<RegisterViewModel>(StatusCodes.Status201Created)
			.ProducesProblem(StatusCodes.Status400BadRequest)
			.ProducesProblem(StatusCodes.Status500InternalServerError);
		}
	}

	private static RegisterCommand? ToCommand(this RegisterRequest? request)
	{
		return request is null
			? null
			: new RegisterCommand(
				request.Username,
				request.Email,
				request.Password,
				request.FirstName,
				request.LastName);
	}

	private static RegisterViewModel ToViewModel(this RegisterResponse response)
	{
		return new RegisterViewModel
		{
			UserId = response.UserId,
			Message = "Registration successful. Please verify your email address."
		};
	}
}

