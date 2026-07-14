using System.Threading;
using System.Threading.Tasks;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;

internal interface IKeycloakAdminClient
{
	Task<string> CreateUserAsync(
		string username,
		string email,
		string password,
		string? firstName,
		string? lastName,
		bool emailVerified,
		bool enabled,
		CancellationToken cancellationToken = default);

	Task SendVerifyEmailActionAsync(
		string userId,
		CancellationToken cancellationToken = default);

	Task UpdateEmailVerifiedAsync(
		string userId,
		bool emailVerified,
		CancellationToken cancellationToken = default);

	Task UpdateUserEnabledAsync(
		string userId,
		bool enabled,
		CancellationToken cancellationToken = default);
}

