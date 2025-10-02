using Bizca.Sdk.SharedKernel;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Bizca.Users.Infrastructure.Context;

internal sealed class IntValueObjectConverter<T>() : ValueConverter<T, int>(static v => ToProvider(v), static v => FromProvider(v)) where T : IValueObject<T, int>
{
	private static int ToProvider(T value)
	{
		return value.Value;
	}

	private static T FromProvider(int value)
	{
		return T.Create(value).Value;
	}
}