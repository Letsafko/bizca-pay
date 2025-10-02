using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users.ValueObjects;

public sealed class UserChannelConfirmationId : ValueObject, IValueObject<UserChannelConfirmationId, int>
{
	public int Value { get; }
	private UserChannelConfirmationId(int value)
	{
		Value = value;
	}

	public static implicit operator int(UserChannelConfirmationId userChannelValue)
	{
		return userChannelValue.Value;
	}

	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value;
	}

	public static Result<UserChannelConfirmationId> Create(int value)
	{
		const string errorCode = "INVALID_CHANNEL_CONFIRMATION_ID";
		return value switch
		{
			<= 0 => Error.Problem(errorCode, "Value must be greater than 0"),
			_ => new UserChannelConfirmationId(value)
		};
	}
}