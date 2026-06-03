using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Application.Models;
using Bizca.Sdk.SharedKernel;
using FluentValidation;

namespace Bizca.OpenId.Application.Usecases.Tokens;

public static class Refresh
{
	public record Command(string? RefreshToken): ICommand;

	public sealed class Validator : AbstractValidator<Command>
	{
		public Validator()
		{
			RuleFor(x => x.RefreshToken)
				.NotEmpty()
				.WithMessage("{PropertyName} is required.");
		}
	}

	public sealed class Handler(ITokenProvider tokenProvider) : IRequestHandler<Command, TokenResponse>
	{
		public async Task<Result<TokenResponse>> HandleAsync(Command? request, CancellationToken cancellationToken)
		{
			return await tokenProvider.RefreshTokenAsync(request!.RefreshToken!, cancellationToken);
		}
	}
}