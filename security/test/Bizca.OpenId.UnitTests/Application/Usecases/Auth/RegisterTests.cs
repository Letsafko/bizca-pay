using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Application.Abstractions;
using Bizca.OpenId.Application.Usecases.Auth;
using FluentAssertions;
using Moq;
using Xunit;

namespace Bizca.OpenId.UnitTests.Application.Usecases.Auth;

[Trait("Category", "Unit")]
public sealed class RegisterTests
{
	private readonly Mock<IIdentityProvider> _identityProviderMock;
	private readonly Register.Handler _handler;

	public RegisterTests()
	{
		_identityProviderMock = new Mock<IIdentityProvider>();
		_handler = new Register.Handler(_identityProviderMock.Object);
	}

	[Fact]
	public async Task AValidRegistrationCommand_CreatesUserAndSendsVerificationEmail()
	{
		// Arrange
		var expectedUserId = "user-123";
		var command = new Register.Command(
			Username: "testuser",
			Email: "test@example.com",
			Password: "SecurePass123!",
			FirstName: "Test",
			LastName: "User");

		_identityProviderMock
			.Setup(x => x.CreateUserAsync(
				It.Is<string>(s => s == command.Username),
				It.Is<string>(s => s == command.Email),
				It.Is<string>(s => s == command.Password),
				command.FirstName,
				command.LastName,
				false,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedUserId);

		_identityProviderMock
			.Setup(x => x.SendEmailVerificationAsync(
				expectedUserId,
				It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.UserId.Should().Be(expectedUserId);

		_identityProviderMock.Verify(
			x => x.CreateUserAsync(
				It.Is<string>(s => s == command.Username),
				It.Is<string>(s => s == command.Email),
				It.Is<string>(s => s == command.Password),
				command.FirstName,
				command.LastName,
				false,
				It.IsAny<CancellationToken>()),
			Times.Once);

		_identityProviderMock.Verify(
			x => x.SendEmailVerificationAsync(
				expectedUserId,
				It.IsAny<CancellationToken>()),
			Times.Once);
	}

	[Fact]
	public async Task ARegistrationCommandWithoutOptionalFields_CreatesUserSuccessfully()
	{
		// Arrange
		var expectedUserId = "user-456";
		var command = new Register.Command(
			Username: "simpleuser",
			Email: "simple@example.com",
			Password: "SimplePass123!",
			FirstName: null,
			LastName: null);

		_identityProviderMock
			.Setup(x => x.CreateUserAsync(
				It.Is<string>(s => !string.IsNullOrEmpty(s)),
				It.Is<string>(s => !string.IsNullOrEmpty(s)),
				It.Is<string>(s => !string.IsNullOrEmpty(s)),
				null,
				null,
				false,
				It.IsAny<CancellationToken>()))
			.ReturnsAsync(expectedUserId);

		_identityProviderMock
			.Setup(x => x.SendEmailVerificationAsync(
				It.IsAny<string>(),
				It.IsAny<CancellationToken>()))
			.Returns(Task.CompletedTask);

		// Act
		var result = await _handler.HandleAsync(command, CancellationToken.None);

		// Assert
		result.IsSuccess.Should().BeTrue();
		result.Value.UserId.Should().Be(expectedUserId);
	}
}



