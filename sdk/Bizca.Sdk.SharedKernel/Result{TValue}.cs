using System;

namespace Bizca.Sdk.SharedKernel;

public sealed class Result<TValue> : Result
{
	public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("The value of a failure result can't be accessed.");

	private readonly TValue? _value;

	internal Result(TValue? value, bool isSuccess, Error error) : base(isSuccess, error)
	{
		_value = value;
	}

	public static implicit operator Result<TValue>(TValue value)
	{
		return value is not null ? Success(value) : Failure<TValue>(Error.None);
	}

	public static implicit operator Result<TValue>(Error error)
	{
		return Failure<TValue>(error);
	}
}