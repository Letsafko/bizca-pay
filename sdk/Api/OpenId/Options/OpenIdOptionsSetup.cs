using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bizca.Sdk.Api.OpenId.Options;

public sealed class OpenIdOptionsSetup(IConfiguration configuration) : IConfigureOptions<OpenIdOptions>
{
    private const string ConfigurationSectionName = nameof(OpenIdOptions);

    public void Configure(OpenIdOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}

