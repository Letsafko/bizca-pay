namespace Bizca.Sdk.Api.OpenApi;

/// <summary>
/// Configuration options for the Bizca OpenAPI layer.
/// </summary>
public sealed class BizcaOpenApiOptions
{
    /// <summary>Gets or sets the API title shown in the spec and the Scalar UI.</summary>
    public string Title { get; init; } = "Bizca API";

    /// <summary>Gets or sets an optional description shown in the spec info block.</summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the list of API version document names to register.
    /// Each entry produces a separate spec at <c>/openapi/{version}.json</c>.
    /// </summary>
    public string[] Versions { get; init; } = ["v1"];

    /// <summary>
    /// Gets or sets a value indicating whether to add a Bearer/JWT security scheme
    /// </summary>
    public bool EnableBearerSecurity { get; init; } = true;

    public string BearerSchemeName { get; init; } = "Bearer";

    public string BearerFormat { get; init; } = "JWT";
}

