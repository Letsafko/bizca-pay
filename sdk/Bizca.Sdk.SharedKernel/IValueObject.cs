namespace Bizca.Sdk.SharedKernel;

public interface IValueObject<T, TValue>
{
	TValue Value { get; }
	static abstract Result<T> Create(TValue value);
}