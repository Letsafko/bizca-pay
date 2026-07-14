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

public sealed class OpenIdWebApplicationFactory(
	IOpenIdConfiguration openIdConfiguration) : WebApplicationFactory<Program>
{
	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder.ConfigureAppConfiguration((_, config) =>
		{
			config.AddInMemoryCollection([
				new KeyValuePair<string, string?>("KeycloakOptions:Authority", openIdConfiguration.Authority),
				new KeyValuePair<string, string?>("KeycloakOptions:ClientId", openIdConfiguration.ClientId),
				new KeyValuePair<string, string?>("KeycloakOptions:ClientSecret", openIdConfiguration.ClientSecret),
				new KeyValuePair<string, string?>("KeycloakOptions:Realm", openIdConfiguration.Realm),
				new KeyValuePair<string, string?>("KeycloakOptions:Scopes", "openid profile email"),
				new KeyValuePair<string, string?>("KeycloakOptions:JwksCacheDurationSeconds", "3600"),
				new KeyValuePair<string, string?>("KeycloakOptions:HttpTimeoutSeconds", "30")
			]!);
		});

		builder
			.UseEnvironment("IntegrationTest")
			.ConfigureTestServices(services =>
			{
				services.AddHttpClient(Constant.KeycloakTestClientNameAdmin, client =>
				{
					client.BaseAddress = new Uri(openIdConfiguration.BaseAddress);
					client.Timeout = TimeSpan.FromSeconds(30);
				});

				services.AddSingleton(openIdConfiguration);
				services.AddSingleton<KeycloakAdminTestService>();
			});
	}

	protected override void ConfigureClient(HttpClient client)
	{
		base.ConfigureClient(client);
		client.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
	}
}

