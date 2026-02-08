using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users.ValueObjects;

public sealed class ChannelValue : ValueObject, IValueObject<ChannelValue, string>
{
	public string Value { get; }
	private ChannelValue(string value)
	{
		Value = value;
	}

	public static implicit operator string(ChannelValue channelValue)
	{
		return channelValue.Value;
	}

	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value;
	}

	public static Result<ChannelValue> Create(string value)
	{
		const string errorCode = "INVALID_CHANNEL_VALUE";
		return string.IsNullOrWhiteSpace(value)
			? Error.Problem(errorCode, "Value must not be null, empty or white space")
			: new ChannelValue(value);
	}
}