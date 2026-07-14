using System.Net;
using System.Threading.Tasks;
using Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;
using FluentAssertions;
using Xunit;

namespace Bizca.OpenId.IntegrationTests.Endpoints;

[Collection(KeycloakFixtureCollection.Name)]
[Trait("Category", "Integration")]
public sealed class HealthTests(KeycloakOpenIdApiFixture apiFixture)
{
	[Fact]
	public async Task HealthEndpoint_ReturnsOk_WithHealthyStatus()
	{
		// Act
		var response = await apiFixture.HttpClient.GetAsync("/health");

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
	}
}

