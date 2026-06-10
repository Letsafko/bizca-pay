using System.Threading;
using System.Threading.Tasks;

namespace Bizca.OpenId.Application.Abstractions;

/// <summary>
/// Abstraction for identity provider operations (user management, email verification).
/// Keycloak-agnostic interface - implementation details are in Infrastructure layer.
/// </summary>
public interface IIdentityProvider
{
	/// <summary>
	/// Creates a new user identity in the identity provider.
	/// </summary>
	/// <param name="username">Unique username</param>
	/// <param name="email">User email address</param>
	/// <param name="password">User password (will be hashed by IDP)</param>
	/// <param name="firstName">User first name</param>
	/// <param name="lastName">User last name</param>
	/// <param name="emailVerified">Whether email is pre-verified (false = verification required)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>The user's unique identifier (sub claim)</returns>
	Task<string> CreateUserAsync(
		string username,
		string email,
		string password,
		string? firstName,
		string? lastName,
		bool emailVerified,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sends an email verification link to the user.
	/// </summary>
	/// <param name="userId">User identifier (sub)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task SendEmailVerificationAsync(
		string userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Marks a user's email as verified.
	/// </summary>
	/// <param name="userId">User identifier (sub)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task VerifyEmailAsync(
		string userId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Enables a user account.
	/// </summary>
	/// <param name="userId">User identifier (sub)</param>
	/// <param name="cancellationToken">Cancellation token</param>
	Task EnableUserAsync(
		string userId,
		CancellationToken cancellationToken = default);
}

