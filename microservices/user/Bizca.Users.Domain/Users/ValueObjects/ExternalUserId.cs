using System;
using System.Collections.Generic;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Domain.Users.ValueObjects;

public sealed class ExternalUserId : ValueObject, IValueObject<ExternalUserId, Guid>
{
	public Guid Value { get; }

	private ExternalUserId(Guid value)
	{
		Value = value;
	}

	public static implicit operator Guid(ExternalUserId externalUserId)
	{
		return externalUserId.Value;
	}

	public static Result<ExternalUserId> Create(Guid value)
	{
		const string errorCode = "INVALID_EXTERNAL_USER_ID";
		return value == Guid.Empty
			? Error.Problem(errorCode, "Value must not be empty")
			   : new ExternalUserId(value);
	}

	protected override IEnumerable<object?> GetEqualityComponents()
	{
		yield return Value;
	}

}