using Bizca.OpenId.Infrastructure.Keycloak.Extensions;
using Bizca.Sdk.Abstractions;
using Bizca.Sdk.SharedKernel.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.OpenId.Infrastructure;

public static class DependencyInjectionExtensions
{
	public static void AddInfrastructure(this IServiceCollection services, params Type[] requestHandlerTypes)
	{
		services.AddKeycloakClients();
		services.AddJwksCache();
		services.AddPipelineBehaviors(requestHandlerTypes);
		services.AddTimeProvider();
	}
}