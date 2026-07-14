using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Abstractions;
using Bizca.Sdk.SharedKernel;
using FluentValidation;

namespace Bizca.OpenId.Application.Usecases.Auth;

public static class VerifyEmail
{
	public sealed record Command(string? Token) : ICommand;

	public sealed record Response(bool Success);

	public sealed class Validator : AbstractValidator<Command>
	{
		public Validator()
		{
			RuleFor(x => x.Token)
				.NotEmpty()
				.WithMessage("{PropertyName} is required.");
		}
	}

	public sealed class Handler(IIdentityProvider identityProvider) : IRequestHandler<Command, Response>
	{
		public async Task<Result<Response>> HandleAsync(Command? request, CancellationToken cancellationToken)
		{
			// NOTE: Decode and validate the verification token to extract userId
			// For now, we'll accept userId directly as token (simplified)
			// In production, this should be a signed JWT or encrypted token

			await identityProvider.VerifyEmailAsync(request!.Token!, cancellationToken);
			await identityProvider.EnableUserAsync(request.Token!, cancellationToken);

			return new Response(true);
		}
	}
}


