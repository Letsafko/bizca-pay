using System;

namespace Bizca.Sdk.SharedKernel;

public class Result
{
	public bool IsSuccess { get; }

	public Error Error { get; }

	protected Result(bool isSuccess, Error error)
	{
		if((isSuccess && error != Error.None) || (!isSuccess && error == Error.None))
		{
			throw new ArgumentException("A successful result cannot contain an error", nameof(error));
		}

		IsSuccess = isSuccess;
		Error = error;
	}

	public static Result Success()
	{
		return new Result(true, Error.None);
	}

	protected static Result<TValue> Success<TValue>(TValue value)
	{
		return new Result<TValue>(value, true, Error.None);
	}

	protected static Result<TValue> Failure<TValue>(Error error)
	{
		return new Result<TValue>(default, false, error);
	}

	public static implicit operator Result(Error error)
	{
		return new Result(false, error);
	}
}
