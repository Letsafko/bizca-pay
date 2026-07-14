using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients;

internal sealed class KeycloakIdentityProvider(IKeycloakAdminClient keycloakAdminClient) : IIdentityProvider
{
	public async Task<string> CreateUserAsync(
		string username,
		string email,
		string password,
		string? firstName,
		string? lastName,
		bool emailVerified,
		CancellationToken cancellationToken = default)
	{
		// Create user disabled initially - will be enabled after email verification
		var userId = await keycloakAdminClient.CreateUserAsync(
			username,
			email,
			password,
			firstName,
			lastName,
			emailVerified,
			enabled: false,
			cancellationToken);

		return userId;
	}

	public Task SendEmailVerificationAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		return keycloakAdminClient.SendVerifyEmailActionAsync(userId, cancellationToken);
	}

	public Task VerifyEmailAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		return keycloakAdminClient.UpdateEmailVerifiedAsync(userId, emailVerified: true, cancellationToken);
	}

	public Task EnableUserAsync(
		string userId,
		CancellationToken cancellationToken = default)
	{
		return keycloakAdminClient.UpdateUserEnabledAsync(userId, enabled: true, cancellationToken);
	}
}

