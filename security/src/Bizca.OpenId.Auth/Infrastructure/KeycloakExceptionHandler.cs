using System;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Infrastructure.Keycloak.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Bizca.OpenId.Auth.Infrastructure;

internal sealed class KeycloakExceptionHandler(ILogger<KeycloakExceptionHandler> logger) : IExceptionHandler
{
	public async ValueTask<bool> TryHandleAsync(
		HttpContext httpContext,
		Exception exception,
		CancellationToken cancellationToken)
	{
		if (exception is not KeycloakException keycloakException)
		{
			return false;
		}

		logger.LogError(exception, "Unhandled exception occurred");

		var problemDetails = new ProblemDetails
		{
			Status = keycloakException.HttpStatusCode,
			Detail = keycloakException.Code,
			Title = keycloakException.Description
		};

		httpContext.Response.StatusCode = keycloakException.HttpStatusCode;
		await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken: cancellationToken);
		return true;
	}
}
