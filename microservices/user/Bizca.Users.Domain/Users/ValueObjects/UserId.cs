using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users.ValueObjects;

public sealed class UserId : ValueObject, IValueObject<UserId, int>
{
	public int Value { get; }

	private UserId(int value)
	{
		Value = value;
	}

	public static Result<UserId> Create(int value)
	{
		const string errorCode = "INVALID_USER_ID";
		return value switch
		{
			<= 0 => Error.Problem(errorCode, "Value must be greater than 0"),
			_ => new UserId(value)
		};
	}

	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value;
	}

	public static implicit operator int(UserId userId)
	{
		return userId.Value;
	}
}