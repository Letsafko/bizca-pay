using System;
using Bizca.OpenId.Infrastructure.Keycloak.Clients;
using Bizca.OpenId.Infrastructure.Keycloak.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.Infrastructure.Keycloak;

internal static class HttpClientExtensions
{
	internal static void AddKeycloakHttpClient(this IServiceCollection services, string clientName)
	{
		services.AddHttpClient(clientName).ConfigureHttpClient((provider, client) =>
		{
			var options = provider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
			var authority = options.Authority;

			var baseAddressWithSlash = authority.FormatBaseAddress();
			client.BaseAddress = new Uri(baseAddressWithSlash);
			client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
		});
	}

	internal static void AddKeycloakAdminHttpClient(this IServiceCollection services)
	{
		services
			.AddSingleton<KeycloakAdminTokenDelegateHandler>()
			.AddHttpClient(OAuth2KeycloakConstants.KeycloakClientNameAdmin)
			.ConfigureHttpClient((provider, client) =>
			{
				var options = provider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
				var authority = options.Authority;

				var baseAddressWithSlash = authority.FormatBaseAddress();
				client.BaseAddress = new Uri(baseAddressWithSlash);
				client.Timeout = TimeSpan.FromSeconds(options.HttpTimeoutSeconds);
			})
			.AddHttpMessageHandler<KeycloakAdminTokenDelegateHandler>();
	}

	private static string FormatBaseAddress(this string authority)
	{
		return authority.EndsWith('/') ? authority : $"{authority}/";
	}
}