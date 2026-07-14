using System;
using Bizca.Sdk.Abstractions.Pipelines;
using Bizca.Sdk.SharedKernel;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.Sdk.Abstractions;

public static class PipelineBehaviorExtensions
{
	public static void AddPipelineBehaviors(this IServiceCollection services, params Type[] requestHandlerTypes)
	{
		services.Scan(scan => scan.FromAssembliesOf(requestHandlerTypes)
				.AddClasses(classes => classes.AssignableTo(typeof(IRequestHandler<,>)), publicOnly: false)
				.AsImplementedInterfaces()
				.WithScopedLifetime());

		services.Decorate(typeof(IRequestHandler<,>), typeof(ValidationDecorator.RequestHandler<,>));
		services.Decorate(typeof(IRequestHandler<,>), typeof(LoggingDecorator.RequestHandler<,>));
	}
}