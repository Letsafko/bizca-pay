using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Abstractions;
using Bizca.Sdk.SharedKernel;
using FluentValidation;

namespace Bizca.OpenId.Application.Usecases.Auth;

public static class Register
{
	public sealed record Command(
		string? Username,
		string? Email,
		string? Password,
		string? FirstName,
		string? LastName) : ICommand;

	public sealed record Response(string UserId);

	public sealed class Validator : AbstractValidator<Command>
	{
		public Validator()
		{
			RuleFor(x => x.Username)
				.NotEmpty()
				.WithMessage("{PropertyName} is required.")
				.MinimumLength(3)
				.WithMessage("{PropertyName} must be at least 3 characters.")
				.MaximumLength(50)
				.WithMessage("{PropertyName} must not exceed 50 characters.");

			RuleFor(x => x.Email)
				.NotEmpty()
				.WithMessage("{PropertyName} is required.")
				.EmailAddress()
				.WithMessage("{PropertyName} must be a valid email address.");

			RuleFor(x => x.Password)
				.NotEmpty()
				.WithMessage("{PropertyName} is required.")
				.MinimumLength(8)
				.WithMessage("{PropertyName} must be at least 8 characters.");

			RuleFor(x => x.FirstName)
				.MaximumLength(100)
				.WithMessage("{PropertyName} must not exceed 100 characters.");

			RuleFor(x => x.LastName)
				.MaximumLength(100)
				.WithMessage("{PropertyName} must not exceed 100 characters.");
		}
	}

	public sealed class Handler(IIdentityProvider identityProvider) : IRequestHandler<Command, Response>
	{
		public async Task<Result<Response>> HandleAsync(Command? request, CancellationToken cancellationToken)
		{
			// Create identity in Keycloak (email not verified initially)
			var userId = await identityProvider.CreateUserAsync(
				request!.Username!,
				request.Email!,
				request.Password!,
				request.FirstName,
				request.LastName,
				emailVerified: false,
				cancellationToken);

			// Send email verification (if enabled via options, handled by IIdentityProvider)
			await identityProvider.SendEmailVerificationAsync(userId, cancellationToken);

			// NOTE: Create user profile in Bizca.Users via internal HTTP call
			// This will be implemented once we have the Users microservice ready

			return new Response(userId);
		}
	}
}


