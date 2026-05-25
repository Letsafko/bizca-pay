using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.Api.Options;

public sealed class KeycloakOptionsSetup(IConfiguration configuration) : IConfigureOptions<KeycloakOptions>
{
    private const string ConfigurationSectionName = nameof(KeycloakOptions);

    public void Configure(KeycloakOptions options)
    {
        configuration.GetSection(ConfigurationSectionName).Bind(options);
    }
}

