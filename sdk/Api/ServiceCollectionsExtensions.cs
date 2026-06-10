using System;
using Bizca.Sdk.Api.MinimalApi.ExceptionHandlers;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.Sdk.Api;

public static class ServiceCollectionsExtensions
{
	public static IServiceCollection AddExceptionHandlers(
		this IServiceCollection services,
		Action<IServiceCollection>? configureAction = null)
	{
		configureAction?.Invoke(services);
		services.AddExceptionHandler<GlobalExceptionHandler>();
		return services;
	}

}