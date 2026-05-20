using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bizca.Users.UnitTests.Users.ValueObjects;

[Trait("Category", "Unit")]
public sealed class ChannelValueTests
{
    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("+33612345678")]
    [InlineData("some-channel-value")]
    public void AValidChannelValue_IsAccepted_AndPreservesTheRawValue(string raw)
    {
        var result = ChannelValue.Create(raw);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankChannelValue_IsRejected_WithAnExplicitErrorCode(string raw)
    {
        var result = ChannelValue.Create(raw);

        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("INVALID_CHANNEL_VALUE");
    }
}


