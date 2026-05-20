using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Bizca.Sdk.Api.OpenApi.Transformers;

/// <summary>
/// Sets the <see cref="OpenApiInfo"/> (title, description, version) on each generated document.
/// </summary>
internal sealed class DocumentInfoTransformer(BizcaOpenApiOptions options, string version)
    : IOpenApiDocumentTransformer
{
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = options.Title,
            Description = options.Description,
            Version = version
        };

        return Task.CompletedTask;
    }
}

