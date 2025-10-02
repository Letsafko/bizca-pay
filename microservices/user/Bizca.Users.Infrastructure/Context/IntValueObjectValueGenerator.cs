using Bizca.Sdk.SharedKernel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Bizca.Users.Infrastructure.Context;

internal sealed class IntValueObjectValueGenerator<T> : ValueGenerator<T> where T : IValueObject<T, int>
{
	private int _lastValue;
	public override bool GeneratesTemporaryValues => true;

	public override T Next(EntityEntry entry)
	{
		_lastValue++;
		return T.Create(_lastValue).Value;
	}
}