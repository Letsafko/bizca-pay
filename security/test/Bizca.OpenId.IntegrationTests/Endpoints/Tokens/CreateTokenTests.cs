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
public sealed class CreateTokenTests(KeycloakOpenIdApiFixture apiFixture)
{
	private const int DefaultTokenDurationTimeInMinutes = 1;

	[Fact]
	public async Task CreateToken_ShouldReturnsAccessToken_WhenProcessingValidClientCredentialsRequest()
	{
		// Arrange
		var request = new CreateTokenRequest
		{
			GrantType = "client_credentials"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/token", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var token = await response.Content.ReadFromJsonAsync<TokenViewModel>();

		token.Should().NotBeNull();
		token.AccessToken.Should().NotBeNullOrWhiteSpace();
		token.ExpiresIn.Should().BeGreaterThan(TimeSpan.FromMinutes(DefaultTokenDurationTimeInMinutes));
	}

	[Fact]
	public async Task CreateToken_ShouldReturnBadRequest_WhenRequestedWithEmptyGrantType()
	{
		// Arrange
		var request = new CreateTokenRequest
		{
			GrantType = ""
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/token", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CreateToken_ShouldReturnsBadRequest_WhenRequestedWithUnsupportedGrantType()
	{
		// Arrange
		var request = new CreateTokenRequest
		{
			GrantType = "password"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/token", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CreateToken_ShouldReturnsBadRequest_WhenProcessingAuthorizationCodeRequestWithoutCode()
	{
		// Arrange
		var request = new CreateTokenRequest
		{
			GrantType = "authorization_code",
			RedirectUri = "http://localhost:3000/callback"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/token", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task CreateToken_ShouldReturnsBadRequest_WhenProcessingAuthorizationCodeRequestWithoutRedirectUri()
	{
		// Arrange
		var request = new CreateTokenRequest
		{
			GrantType = "authorization_code",
			Code = "test-code"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/token", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}
}

