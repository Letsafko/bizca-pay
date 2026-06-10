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
public sealed class TokenExchangeTests(KeycloakOpenIdApiFixture apiFixture)
{
	[Fact]
	public async Task AValidClientCredentialsRequest_ReturnsAccessToken()
	{
		// Arrange
		var request = new ExchangeTokenRequest
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
		token.ExpiresIn.Should().BeGreaterThan(TimeSpan.Zero);
	}

	[Fact]
	public async Task AnEmptyGrantType_ReturnsBadRequest_WithValidationError()
	{
		// Arrange
		var request = new ExchangeTokenRequest
		{
			GrantType = ""
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/token", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task AnUnsupportedGrantType_ReturnsBadRequest_WithValidationError()
	{
		// Arrange
		var request = new ExchangeTokenRequest
		{
			GrantType = "password"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/token", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task AnAuthorizationCodeRequest_WithMissingCode_ReturnsBadRequest()
	{
		// Arrange
		var request = new ExchangeTokenRequest
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
	public async Task AnAuthorizationCodeRequest_WithMissingRedirectUri_ReturnsBadRequest()
	{
		// Arrange
		var request = new ExchangeTokenRequest
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

