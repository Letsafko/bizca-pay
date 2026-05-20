using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bizca.Users.UnitTests.Users.ValueObjects;

[Trait("Category", "Unit")]
public sealed class UserIdTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void APositiveUserId_IsAccepted_AndPreservesTheRawValue(int raw)
    {
        var result = UserId.Create(raw);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void AZeroOrNegativeUserId_IsRejected_WithAnExplicitErrorCode(int raw)
    {
        var result = UserId.Create(raw);

        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("INVALID_USER_ID");
    }
}


