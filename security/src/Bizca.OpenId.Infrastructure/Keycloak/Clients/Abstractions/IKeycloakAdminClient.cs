using System.Threading;
using System.Threading.Tasks;

namespace Bizca.OpenId.Infrastructure.Keycloak.Clients.Abstractions;

/// <summary>
/// Abstraction for Keycloak Admin API operations.
/// </summary>
internal interface IKeycloakAdminClient
{
	/// <summary>
	/// Creates a new user in Keycloak realm.
	/// </summary>
	/// <returns>The created user's unique identifier (sub)</returns>
	Task<string> CreateUserAsync(
		string username,
		string email,
		string password,
		string? firstName,
		string? lastName,
		bool emailVerified,
		bool enabled,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends an email verification action to the user.
	/// </summary>
	Task SendVerifyEmailActionAsync(
		string userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates user's email verification status.
	/// </summary>
	Task UpdateEmailVerifiedAsync(
		string userId,
		bool emailVerified,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Enables or disables a user account.
	/// </summary>
	Task UpdateUserEnabledAsync(
		string userId,
		bool enabled,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Gets an admin access token for Keycloak Admin API operations.
	/// Uses client_credentials grant type.
	/// </summary>
	Task<string> GetAdminAccessTokenAsync(
		CancellationToken cancellationToken = default);
}

