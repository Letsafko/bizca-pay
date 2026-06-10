using System;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bizca.OpenId.ApiModels.Requests;
using Bizca.OpenId.ApiModels.Responses;
using Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;
using FluentAssertions;
using Xunit;

namespace Bizca.OpenId.IntegrationTests.Endpoints.Tokens;

[Collection(KeycloakFixtureCollection.Name)]
[Trait("Category", "Integration")]
public sealed class RefreshTokenTests(KeycloakOpenIdApiFixture apiFixture)
{
	[Fact]
	public async Task AValidRefreshToken_ReturnsNewAccessToken()
	{
		// Arrange - Get a valid refresh token first
		var (_, refreshToken) = await apiFixture.KeycloakAdminService.GetRefreshableTokenAsync();
		var request = new RefreshTokenRequest
		{
			RefreshToken = refreshToken
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/refresh", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var token = await response.Content.ReadFromJsonAsync<TokenViewModel>();

		token.Should().NotBeNull();
		token.AccessToken.Should().NotBeNullOrWhiteSpace();
		token.ExpiresIn.Should().BeGreaterThan(TimeSpan.Zero);
	}

	[Fact]
	public async Task AnEmptyRefreshToken_ReturnsBadRequest()
	{
		// Arrange
		var request = new RefreshTokenRequest
		{
			RefreshToken = ""
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/refresh", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task AnInvalidRefreshToken_ReturnsBadRequest()
	{
		// Arrange
		var request = new RefreshTokenRequest
		{
			RefreshToken = "invalid-token"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/refresh", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}

