using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bizca.Sdk.SharedKernel;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.Sdk.Abstractions.Pipelines;

public static class ValidationDecorator
{
	public sealed class RequestHandler<TRequest, TResponse>(
		IRequestHandler<TRequest, TResponse> innerHandler,
		IServiceProvider serviceProvider) : IRequestHandler<TRequest, TResponse>
		where TRequest : IRequest
	{
		public async Task<Result<TResponse>> HandleAsync(TRequest? request, CancellationToken cancellationToken)
		{
			var validationError = await ValidateAsync(request, serviceProvider);

			if (validationError is null)
			{
				return await innerHandler.HandleAsync(request, cancellationToken);
			}

			return validationError;
		}

		private static async Task<Error?> ValidateAsync<T>(
			T? request,
			IServiceProvider serviceProvider)
		{
			if (request is null)
			{
				return Error.NullValue;
			}

			var validator = serviceProvider.GetService<IValidator<T>>();
			if (validator is null)
			{
				return null;
			}

			var validationResult = await validator.ValidateAsync(request);
			if (validationResult.IsValid)
			{
				return null;
			}

			var errors = validationResult
				.Errors
				.Select(e => Error.Problem(e.ErrorCode, e.ErrorMessage))
				.ToArray();

			return new ValidationError(errors);
		}
	}
}