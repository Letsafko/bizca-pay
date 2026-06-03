using Microsoft.Extensions.DependencyInjection;

namespace Bizca.Sdk.SharedKernel.Extensions;

public static class TimeProviderExtensions
{
	public static IServiceCollection AddTimeProvider(this IServiceCollection services)
	{
		services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
		return services;
	}
}