using FluentValidation;

namespace Bizca.Sdk.Api.OpenApi;

/// <summary>
/// Configuration options for the Bizca OpenAPI layer.
/// </summary>
public sealed class OpenApiOptions
{
	internal const string SectionName = nameof(OpenApiOptions);

    /// <summary>Gets or sets the API title shown in the spec and the Scalar UI.</summary>
    public string? Title { get; init; }

    /// <summary>Gets or sets an optional description shown in the spec info block.</summary>
    public string? Description { get; init; }

	/// <summary>
	/// Gets or sets the list of API version document names to register.
	/// Each entry produces a separate spec at <c>/openapi/{version}.json</c>.
	/// </summary>
	public string[] Versions { get; init; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether to add a Bearer/JWT security scheme
    /// </summary>
    public bool EnableBearerSecurity { get; init; } = true;

    /// <summary>Gets or sets the scheme name used to register the Bearer security scheme.</summary>
    public string BearerSchemeName { get; init; } = "Bearer";

    /// <summary>Gets or sets the bearer token format label shown in the spec (e.g. <c>JWT</c>).</summary>
    public string BearerFormat { get; init; } = "JWT";
}

public sealed class OpenApiOptionsValidator : AbstractValidator<OpenApiOptions>
{
	public OpenApiOptionsValidator()
	{
		RuleFor(x => x.Versions)
			.NotEmpty()
			.WithMessage("{PropertyName} must be specified.");

		RuleFor(x => x.BearerSchemeName)
			.NotEmpty()
			.WithMessage("{PropertyName} must be specified.");

		RuleFor(x => x.BearerFormat)
			.NotEmpty()
			.WithMessage("{PropertyName} must be specified.");

		RuleFor(x => x.Title)
			.NotEmpty()
			.WithMessage("{PropertyName} must be specified.");
	}
}

