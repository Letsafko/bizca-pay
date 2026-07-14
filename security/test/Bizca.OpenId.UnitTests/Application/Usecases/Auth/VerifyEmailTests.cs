using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Application.Usecases.Auth;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bizca.OpenId.UnitTests.Application.Usecases.Auth;

[Trait("Category", "Unit")]
public sealed class VerifyEmailTests
{
	private readonly Mock<IIdentityProvider> _identityProviderMock;
	private readonly VerifyEmail.Handler _handler;

	public VerifyEmailTests()
	{
		_identityProviderMock = new Mock<IIdentityProvider>();
		_handler = new VerifyEmail.Handler(_identityProviderMock.Object);
	}

	[Fact]
	public async Task AValidVerificationToken_VerifiesEmailAndEnablesUser()
	{
		// Arrange
		var token = "user-123";
		var command = new VerifyEmail.Command(Token: token);

		_identityProviderMock
			.Setup(x => x.VerifyEmailAsync(
				token,
				It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		_identityProviderMock
			.Setup(x => x.EnableUserAsync(
				token,
				It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.Success.Should().BeTrue();

		_identityProviderMock.Verify(
			x => x.VerifyEmailAsync(
				token,
				It.IsAny<CancellationToken>()),
			Times.Once);

		_identityProviderMock.Verify(
			x => x.EnableUserAsync(
				token,
				It.IsAny<CancellationToken>()),
			Times.Once);
	}
}

