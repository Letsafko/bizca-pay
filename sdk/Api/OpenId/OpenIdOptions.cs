using System;
using FluentValidation;

namespace Bizca.Sdk.Api.OpenId;
/// <summary>
/// Configuration options for Bizca OpenID Connect JWT validation.
/// </summary>
public sealed class OpenIdOptions
{
	/// <summary>
	/// The name of the configuration section for OpenIdOptions (e.g., "OpenIdOptions").
	/// </summary>
	public const string SectionName = nameof(OpenIdOptions);

    /// <summary>
    /// The expected token issuer (e.g., https://keycloak.example.com/realms/bizca).
    /// </summary>
    public string Issuer { get; set; } = string.Empty;
    /// <summary>
    /// The expected token audience (typically the client_id of the API Gateway).
    /// </summary>
    public string Audience { get; set; } = string.Empty;
    /// <summary>
    /// The OIDC authority URL for JWKS discovery (same as Issuer by default).
    /// </summary>
    public string Authority { get; set; } = string.Empty;
    /// <summary>
    /// Whether to require HTTPS for the authority metadata endpoint (default: true).
    /// </summary>
    public bool RequireHttpsMetadata { get; init; } = true;
    /// <summary>
    /// Clock skew tolerance in seconds for token expiration validation (default: 300 = 5 minutes).
    /// </summary>
    public int ClockSkewSeconds { get; init; } = 300;
}

/// <summary>
/// Validator for OpenIdOptions to ensure required fields are set and valid.
/// </summary>
public sealed class OpenIdOptionsValidator : AbstractValidator<OpenIdOptions>
{
	public OpenIdOptionsValidator()
	{
		RuleFor(x => x.Issuer)
			.NotEmpty()
			.WithMessage("{PropertyName} is required.")
			.Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
			.WithMessage("{PropertyName} must be a valid absolute URI.");

		RuleFor(x => x.Audience)
			.NotEmpty()
			.WithMessage("{PropertyName} is required.");

		RuleFor(x => x.Authority)
			.NotEmpty()
			.WithMessage("{PropertyName} is required.")
			.Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
			.WithMessage("Authority must be a valid absolute URI.");
	}
}
