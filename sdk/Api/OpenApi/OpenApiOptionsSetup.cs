using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bizca.Sdk.Api.OpenApi;

internal sealed class OpenApiOptionsSetup(IConfiguration configuration)
    : IConfigureOptions<OpenApiOptions>
{
    private const string SectionName = nameof(OpenApiOptions);

	public void Configure(OpenApiOptions options)
	{
		configuration.GetSection(SectionName).Bind(options);
	}
}