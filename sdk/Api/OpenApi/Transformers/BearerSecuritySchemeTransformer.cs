using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.OpenApi;

namespace Bizca.Sdk.Api.OpenApi.Transformers;

/// <summary>
/// Adds a Bearer/JWT <see cref="OpenApiSecurityScheme"/> to every OpenAPI document
/// and applies the global security requirement to all operations.
/// </summary>
internal sealed class BearerSecuritySchemeTransformer(OpenApiOptions options)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
		document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
		document.Components.SecuritySchemes[options.BearerSchemeName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = options.BearerSchemeName,
            BearerFormat = options.BearerFormat,
            In = ParameterLocation.Header,
            Description = $"Provide a valid {options.BearerFormat} token."
        };

        var schemeRef = new OpenApiSecuritySchemeReference(options.BearerSchemeName, document, null);
        var requirement = new OpenApiSecurityRequirement
        {
            [schemeRef] = []
        };

		foreach (var operation in document.Paths.Values
										.Where(p => p.Operations is not null)
										.SelectMany(p => p.Operations!.Values))
        {
            operation.Security ??= [];
            operation.Security.Add(requirement);
        }

        return Task.CompletedTask;
    }
}
