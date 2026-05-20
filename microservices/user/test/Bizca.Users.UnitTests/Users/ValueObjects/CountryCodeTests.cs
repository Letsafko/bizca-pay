using Bizca.Sdk.SharedKernel;
using Bizca.Users.Domain.Users.ValueObjects;
using FluentAssertions;
using Xunit;

namespace Bizca.Users.UnitTests.Users.ValueObjects;

[Trait("Category", "Unit")]
public sealed class CountryCodeTests
{
    [Theory]
    [InlineData("FR")]
    [InlineData("US")]
    [InlineData("DE")]
    public void ATwoCharacterCountryCode_IsAccepted_AndPreservesTheRawValue(string raw)
    {
        var result = CountryCode.Create(raw);

        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(raw);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankCountryCode_IsRejected_WithAnExplicitErrorCode(string raw)
    {
        var result = CountryCode.Create(raw);

        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("INVALID_COUNTRY_CODE");
    }

    [Theory]
    [InlineData("F")]
    [InlineData("FRA")]
    [InlineData("FRANCE")]
    public void ACountryCodeWithWrongLength_IsRejected_WithAnExplicitErrorCode(string raw)
    {
        var result = CountryCode.Create(raw);

        result.IsSuccess.Should().BeFalse();
        result.Error.Type.Should().Be(ErrorType.Problem);
        result.Error.Code.Should().Be("INVALID_COUNTRY_CODE_LENGTH");
    }
}


