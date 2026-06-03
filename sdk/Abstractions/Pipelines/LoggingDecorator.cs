using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bizca.Sdk.SharedKernel;
using Microsoft.Extensions.Logging;

namespace Bizca.Sdk.Abstractions.Pipelines;

public static class LoggingDecorator
{
	public sealed class RequestHandler<TRequest, TResponse>(
		IRequestHandler<TRequest, TResponse> innerHandler,
		ILogger<IRequestHandler<TRequest, TResponse>> logger)
		: IRequestHandler<TRequest, TResponse>
		where TRequest : IRequest
	{
		public async Task<Result<TResponse>> HandleAsync(TRequest? request, CancellationToken cancellationToken)
		{
			var requestName = typeof(TRequest).Name;
			logger.LogInformation("Processing request {RequestName}", requestName);

			var result = await innerHandler.HandleAsync(request, cancellationToken);

			if (result.IsSuccess)
			{
				logger.LogInformation("Completed request {RequestName} successfully.", requestName);
			}
			else
			{
				logger.LogError("Completed request {RequestName} with error(s): {RawError}", requestName, JsonSerializer.Serialize(result.Error));
			}

			return result;
		}
	}
}