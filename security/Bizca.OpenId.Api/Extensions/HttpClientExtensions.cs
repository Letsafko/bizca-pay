using System;
using Bizca.OpenId.Infrastructure.Constants;
using Bizca.OpenId.Infrastructure.Keycloak;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.Api.Extensions;

internal static class HttpClientExtensions
{
    internal static void AddKeycloakHttpClient(
		this IServiceCollection services,
		string clientName = OAuth2Constants.Keycloak)
	{
		services.AddHttpClient(clientName)
				.ConfigureHttpClient((provider, client) =>
				{
					var options = provider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
					var authority = options.Authority;

					var baseAddressWithSlash = FormatBaseAddress(authority);
					client.BaseAddress = new Uri(baseAddressWithSlash);
					client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
				});
	}

	private static string FormatBaseAddress(string authority)
	{
		// BaseAddress MUST end with "/" to be treated as a directory
		return authority.EndsWith('/') ? authority : $"{authority}/";
	}
}

