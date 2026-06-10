using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Application.Models;
using Bizca.Sdk.SharedKernel;
using FluentValidation;

namespace Bizca.OpenId.Application.Usecases.Tokens;

public static class Logout
{
	public sealed record Command(string? Token, string? TokenTypeHint) : ICommand;

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

	public sealed class Handler(ITokenProvider tokenProvider) : IRequestHandler<Command, Response>
	{
		public async Task<Result<Response>> HandleAsync(Command? request, CancellationToken cancellationToken)
		{
			var tokenTypeHint = GetTokenTypeHint(request!);
			var result = await tokenProvider.RevokeTokenAsync(
				request!.Token!,
				tokenTypeHint,
				cancellationToken);

			return new Response(result);
		}

		private static string GetTokenTypeHint(Command request)
		{
			return !string.IsNullOrWhiteSpace(request.TokenTypeHint)
				? request.TokenTypeHint
				: GrantType.RefreshToken.Name;
		}
	}
}