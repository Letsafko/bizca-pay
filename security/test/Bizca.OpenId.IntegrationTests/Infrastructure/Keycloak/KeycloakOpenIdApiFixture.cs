using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.Keycloak;
using Xunit;

namespace Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;

#pragma warning disable CA1001
public sealed class KeycloakOpenIdApiFixture : IAsyncLifetime
#pragma warning restore CA1001
{
	private OpenIdWebApplicationFactory _factory = null!;
	internal HttpClient HttpClient { get; private set; } = null!;
	internal KeycloakAdminService KeycloakAdminService { get; private set; } = null!;

	private readonly KeycloakContainer _keycloakContainer = new KeycloakBuilder()
			.WithImage("quay.io/keycloak/keycloak:25.0.6")
			.WithUsername(Constant.OpenIdProvider.Keycloak.AdminUser)
			.WithPassword(Constant.OpenIdProvider.Keycloak.AdminPassword)
			.Build();

	public async Task InitializeAsync()
	{
		await _keycloakContainer.StartAsync();
		var baseAddress = _keycloakContainer.GetBaseAddress();
		var keycloakConfiguration = new KeycloakConfiguration
		{
			Authority = $"{baseAddress.TrimEnd('/')}/realms/{Constant.OpenIdProvider.Keycloak.RealmName}",
			ClientSecret = Constant.OpenIdProvider.Keycloak.ClientSecret,
			ClientId = Constant.OpenIdProvider.Keycloak.ClientId,
			Realm = Constant.OpenIdProvider.Keycloak.RealmName,
			BaseAddress = baseAddress
		};

		_factory = new OpenIdWebApplicationFactory(keycloakConfiguration);
		HttpClient = _factory.CreateClient();

		KeycloakAdminService = _factory.Services.GetRequiredService<KeycloakAdminService>();
		await KeycloakAdminService.ConfigureRealmAndClientAsync();
	}

	public async Task DisposeAsync()
	{
		await _keycloakContainer.DisposeAsync();
		await _factory.DisposeAsync();
	}
}