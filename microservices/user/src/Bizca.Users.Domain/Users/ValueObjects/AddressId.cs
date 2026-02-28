using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users.ValueObjects;

public sealed class AddressId : ValueObject, IValueObject<AddressId, int>
{
	public int Value { get; }

	private AddressId(int value)
	{
		Value = value;
	}

	public static implicit operator int(AddressId addressId)
	{
		return addressId.Value;
	}

	public static Result<AddressId> Create(int value)
	{
		const string errorCode = "INVALID_ADDRESS_ID";
		return value switch
		{
			<= 0 => Error.Problem(errorCode, "Value must be greater than 0"),
			_ => new AddressId(value)
		};
	}

	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value;
	}
}