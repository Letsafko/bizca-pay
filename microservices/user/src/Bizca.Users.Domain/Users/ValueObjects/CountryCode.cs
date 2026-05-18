using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users.ValueObjects;

public sealed class CountryCode : ValueObject, IValueObject<CountryCode, string>
{
	public string Value { get; }
	private CountryCode(string value)
	{
		Value = value;
	}

	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value;
	}

	public static CountryCode? TryCreate(string? value)
	{
		return value is null ? null : new CountryCode(value);
	}

	public static Result<CountryCode> Create(string value)
	{
		if(string.IsNullOrWhiteSpace(value))
		{
			const string invalidCountryCode = "INVALID_COUNTRY_CODE";
			return Error.Problem(invalidCountryCode, "Value must not be null, empty or white space");
		}

		const string invalidCountryCodeLength = "INVALID_COUNTRY_CODE_LENGTH";
		const int countryCodeLength = 2;

		return value.Length != countryCodeLength
			? Error.Problem(invalidCountryCodeLength, "Country code length != 2")
			   : new CountryCode(value);
	}

	public static implicit operator string?(CountryCode? countryCode)
	{
		return countryCode?.Value;
	}
}