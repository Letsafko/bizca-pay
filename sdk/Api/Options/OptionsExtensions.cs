using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bizca.Sdk.Api.Options;

public static class OptionsExtensions
{
	public static IServiceCollection AddOptionsWithValidation<TOptions>(
		this IServiceCollection services,
		string sectionName)
		where TOptions : class
	{
		services
			.AddOptions<TOptions>()
			.BindConfiguration(sectionName)
			.ValidateFluently()
			.ValidateOnStart();

		return services;
	}

	public static IServiceCollection AddOptionsWithSetup<TOptions, TSetup>(
		this IServiceCollection services)
		where TOptions : class
		where TSetup : class, IConfigureOptions<TOptions>
	{
		services.ConfigureOptions<TSetup>();
		services
			.AddOptions<TOptions>()
			.ValidateFluently()
			.ValidateOnStart();

		return services;
	}
}

internal static class FluentValidationOptionsExtensions
{
	internal static OptionsBuilder<TOptions> ValidateFluently<TOptions>(
		this OptionsBuilder<TOptions> optionsBuilder)
		where TOptions : class
	{
		optionsBuilder.Services.AddSingleton<IValidateOptions<TOptions>, FluentValidationOptions<TOptions>>();
		return optionsBuilder;
	}
}