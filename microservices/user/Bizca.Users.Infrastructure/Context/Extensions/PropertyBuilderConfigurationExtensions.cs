using Bizca.Sdk.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Extensions;

internal static class PropertyBuilderConfigurationExtensions
{
	internal static void ToIntValueObjectConverter<T>(this PropertyBuilder<T> builder, string columName) where T : IValueObject<T, int>
	{
		builder
			.HasConversion<IntValueObjectConverter<T>>()
			.HasColumnName(columName);
	}
}