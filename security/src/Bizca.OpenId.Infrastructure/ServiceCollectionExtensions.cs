using System;
using Bizca.OpenId.Infrastructure.Keycloak.Extensions;
using Bizca.Sdk.Abstractions;
using Bizca.Sdk.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.OpenId.Infrastructure;

public static class ServiceCollectionExtensions
{
	public static void AddInfrastructure(this IServiceCollection services, params Type[] requestHandlerTypes)
	{
		services.AddKeycloakClients();
		services.AddJwksCache();
		services.AddPipelineBehaviors(requestHandlerTypes);
		services.AddDateTimeProvider();
	}

	private static void AddDateTimeProvider(this IServiceCollection services)
	{
		services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
	}
}