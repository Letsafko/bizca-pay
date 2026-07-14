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
public sealed class LogoutTests(KeycloakOpenIdApiFixture apiFixture)
{
	[Fact]
	public async Task Logout_ShouldSuccessfullyRevokeRefreshToken_WhenItIsValid()
	{
		// Arrange
		var (_, refreshToken) = await apiFixture.KeycloakAdminService.GetRefreshableTokenAsync();
		var request = new LogoutRequest
		{
			Token = refreshToken,
			TokenTypeHint = "refresh_token"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/logout", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var result = await response.Content.ReadFromJsonAsync<LogoutViewModel>();

		result.Should().NotBeNull();
		result.Revoked.Should().BeTrue();
	}

	[Fact]
	public async Task Logout_ShouldSuccessfullyRevokeAccessToken_WhenItIsValid()
	{
		// Arrange
		var accessToken = await apiFixture.KeycloakAdminService.GetClientCredentialsTokenAsync();
		var request = new LogoutRequest
		{
			Token = accessToken,
			TokenTypeHint = "access_token"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/logout", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var result = await response.Content.ReadFromJsonAsync<LogoutViewModel>();

		result.Should().NotBeNull();
		result.Revoked.Should().BeTrue();
	}

	[Fact]
	public async Task Logout_ShouldReturnsBadRequest_WhenTokenIsEmpty()
	{
		// Arrange
		var request = new LogoutRequest
		{
			Token = "",
			TokenTypeHint = "refresh_token"
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/logout", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
	}

	[Fact]
	public async Task Logout_ShouldDefaultToRefreshToken_WhenTokenTypeHintIsNotProvided()
	{
		// Arrange
		var (_, refreshToken) = await apiFixture.KeycloakAdminService.GetRefreshableTokenAsync();
		var request = new LogoutRequest
		{
			Token = refreshToken
		};

		// Act
		var response = await apiFixture.HttpClient.PostAsJsonAsync("/api/v1/auth/logout", request);

		// Assert
		response.StatusCode.Should().Be(HttpStatusCode.OK);
		var result = await response.Content.ReadFromJsonAsync<LogoutViewModel>();

		result.Should().NotBeNull();
		result.Revoked.Should().BeTrue();
	}
}


