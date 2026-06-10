using System;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Application.Models;
using Bizca.Sdk.SharedKernel;
using FluentValidation;

namespace Bizca.OpenId.Application.Usecases.Tokens;

public static class Exchange
{
	public sealed record Command(
		string? GrantType,
		string? Code,
		string? RedirectUri,
		string? CodeVerifier) : ICommand;

	public sealed class Validator : AbstractValidator<Command>
	{
		public Validator()
		{
			RuleFor(x => x.GrantType)
				.NotEmpty()
				.WithMessage("Grant type is required.")
				.Must(GrantType.IsDefined)
				.WithMessage("{PropertyName} must be one of the following: " + string.Join(", ", GrantType.List));

			When(x => x.GrantType == GrantType.AuthorizationCode.Name, () =>
			{
				RuleFor(x => x.Code)
					.NotEmpty()
					.WithMessage($"Authorization code is required for {GrantType.AuthorizationCode} grant type.");

				RuleFor(x => x.RedirectUri)
					.NotEmpty()
					.WithMessage($"Redirect URI is required for {GrantType.AuthorizationCode} grant type.");
			});
		}
	}

	public sealed class Handler(ITokenProvider tokenProvider)
		: IRequestHandler<Command, TokenResponse>
	{
		public async Task<Result<TokenResponse>> HandleAsync(Command? request, CancellationToken cancellationToken)
		{
			var grantType = GrantType.FromName(request!.GrantType, true);
			return true switch
			{
				_ when grantType == GrantType.AuthorizationCode => await GetTokenFromAuthorizationCodeFlow(request, cancellationToken),
				_ when grantType == GrantType.ClientCredentials => await GetTokenFromClientCredentialsFlow(cancellationToken),
				_                                               => throw new NotSupportedException("Unsupported grant type.")
			};
		}

		private Task<TokenResponse> GetTokenFromAuthorizationCodeFlow(Command command, CancellationToken cancellationToken)
		{
			return tokenProvider.ExchangeCodeForTokenAsync(
				command.Code!,
				command.RedirectUri!,
				command.CodeVerifier,
				cancellationToken);
		}

		private Task<TokenResponse> GetTokenFromClientCredentialsFlow(CancellationToken cancellationToken)
		{
			return tokenProvider.GetClientCredentialsTokenAsync(cancellationToken);
		}
	}
}