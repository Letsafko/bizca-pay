using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users.ValueObjects;

public sealed class UserChannelId : ValueObject, IValueObject<UserChannelId, int>
{
	public int Value { get; }
	private UserChannelId(int value)
	{
		Value = value;
	}

	public static implicit operator int(UserChannelId userChannelId)
	{
		return userChannelId.Value;
	}

	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value;
	}

	public static Result<UserChannelId> Create(int value)
	{
		const string errorCode = "INVALID_CHANNEL_ID";
		return value switch
		{
			<= 0 => Error.Problem(errorCode, "Value must be greater than 0"),
			_ => new UserChannelId(value)
		};
	}
}