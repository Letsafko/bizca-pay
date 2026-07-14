using Bizca.OpenId.Application.Usecases.Auth;
using FluentAssertions;
using Xunit;

namespace Bizca.OpenId.UnitTests.Application.Usecases.Auth;

[Trait("Category", "Unit")]
public sealed class RegisterValidatorTests
{
	private readonly Register.Validator _validator = new();

	[Theory]
	[InlineData("ab")]
	[InlineData("a")]
	public void AUsernameWithLessThan3Characters_IsRejected(string username)
	{
		// Arrange
		var command = new Register.Command(
			Username: username,
			Email: "test@example.com",
			Password: "SecurePass123!",
			FirstName: null,
			LastName: null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Username));
	}

	[Theory]
	[InlineData("invalid-email")]
	[InlineData("@example.com")]
	[InlineData("test@")]
	public void AnInvalidEmailFormat_IsRejected(string email)
	{
		// Arrange
		var command = new Register.Command(
			Username: "testuser",
			Email: email,
			Password: "SecurePass123!",
			FirstName: null,
			LastName: null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Email));
	}

	[Theory]
	[InlineData("short")]
	[InlineData("1234567")]
	public void APasswordWithLessThan8Characters_IsRejected(string password)
	{
		// Arrange
		var command = new Register.Command(
			Username: "testuser",
			Email: "test@example.com",
			Password: password,
			FirstName: null,
			LastName: null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Password));
	}

	[Fact]
	public void AValidRegistrationCommand_PassesValidation()
	{
		// Arrange
		var command = new Register.Command(
			Username: "testuser",
			Email: "test@example.com",
			Password: "SecurePass123!",
			FirstName: "Test",
			LastName: "User");

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void ARegistrationCommandWithoutOptionalFields_PassesValidation()
	{
		// Arrange
		var command = new Register.Command(
			Username: "testuser",
			Email: "test@example.com",
			Password: "SecurePass123!",
			FirstName: null,
			LastName: null);

		// Act
		var result = _validator.Validate(command);

		// Assert
		result.IsValid.Should().BeTrue();
	}
}

