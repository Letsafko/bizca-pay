using System;
using System.Collections.Generic;
using System.Net.Http;
using Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.OpenId.IntegrationTests.Infrastructure;

public sealed class OpenIdWebApplicationFactory(IOpenIdConfiguration configuration) : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((_, config) =>
		{
			config.AddInMemoryCollection([
				new KeyValuePair<string, string?>("KeycloakOptions:Authority", configuration.Authority),
				new KeyValuePair<string, string?>("KeycloakOptions:ClientId", configuration.ClientId),
				new KeyValuePair<string, string?>("KeycloakOptions:ClientSecret", configuration.ClientSecret),
				new KeyValuePair<string, string?>("KeycloakOptions:Realm", configuration.Realm),
				new KeyValuePair<string, string?>("KeycloakOptions:Scopes", "openid profile email"),
				new KeyValuePair<string, string?>("KeycloakOptions:JwksCacheDurationSeconds", "3600"),
				new KeyValuePair<string, string?>("KeycloakOptions:HttpTimeoutSeconds", "30")
			]!);
		});

		builder.ConfigureTestServices(services =>
		{
			services.AddHttpClient(Constant.HttpClientName, client =>
			{
				if (!string.IsNullOrWhiteSpace(configuration.BaseAddress))
				{
					client.BaseAddress = new Uri(configuration.BaseAddress);
				}
			});

			services.AddSingleton(configuration);
			services.AddSingleton<KeycloakAdminService>();
		});

		builder.UseEnvironment("IntegrationTest");
	}

	protected override void ConfigureClient(HttpClient client)
	{
		base.ConfigureClient(client);
		client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
	}
}

