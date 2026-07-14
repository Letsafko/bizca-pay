using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Bizca.Sdk.Api.OpenId.Middleware;

public sealed class TokenValidationMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<TokenValidationMiddleware> _logger;
	private readonly OpenIdOptions _openIdOptions;
	private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
	private readonly JwtSecurityTokenHandler _tokenHandler = new();

	public TokenValidationMiddleware(
		RequestDelegate next,
		ILogger<TokenValidationMiddleware> logger,
		IOptions<OpenIdOptions> openIdOptionsAccessor)
	{
		_next = next;
		_logger = logger;
		_openIdOptions = openIdOptionsAccessor.Value;

		var documentRetriever = new HttpDocumentRetriever
		{
			RequireHttps = _openIdOptions.RequireHttpsMetadata
		};

		_configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
			metadataAddress: $"{_openIdOptions.Authority.TrimEnd('/')}{Constants.Paths.WellKnownConfigPath}",
			configRetriever: new OpenIdConnectConfigurationRetriever(),
			docRetriever: documentRetriever
		);
	}

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Path.StartsWithSegments(Constants.Paths.HealthPath) ||
			context.Request.Path.StartsWithSegments(Constants.Paths.AlternativeHealthPath))
		{
			await _next(context);
			return;
		}

		var token = ExtractBearerToken(context);
		if (string.IsNullOrWhiteSpace(token))
		{
			_logger.LogWarning("Missing Authorization header");
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.WriteAsJsonAsync(new
			{
				error = Constants.ErrorCodes.UnauthorizedError,
				message = Constants.ErrorMessages.MissingAuthHeaderMessage
			});
			return;
		}

		try
		{
			var configuration = await _configurationManager.GetConfigurationAsync(context.RequestAborted);

			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = _openIdOptions.Issuer,
				ValidateAudience = true,
				ValidAudience = _openIdOptions.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.FromSeconds(_openIdOptions.ClockSkewSeconds),
				IssuerSigningKeys = configuration.SigningKeys,
				ValidateIssuerSigningKey = true
			};

			var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

			// Attach validated claims to HttpContext
			context.User = principal;
			context.Items[Constants.ValidatedTokenKey] = validatedToken;

			await _next(context);
		}
		catch (SecurityTokenExpiredException)
		{
			_logger.LogWarning("Token has expired");
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.WriteAsJsonAsync(new
			{
				error = Constants.ErrorCodes.TokenExpiredError,
				message = Constants.ErrorMessages.TokenExpiredMessage
			});
		}
		catch (SecurityTokenException ex)
		{
			_logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.WriteAsJsonAsync(new
			{
				error = Constants.ErrorCodes.InvalidTokenError,
				message = Constants.ErrorMessages.InvalidTokenMessage
			});
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occured while validating token.");
			context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
			await context.Response.WriteAsJsonAsync(new
			{
				error = Constants.ErrorCodes.ServiceUnavailableError,
				message = Constants.ErrorMessages.ServiceUnavailableMessage
			});
		}
	}

	private static string? ExtractBearerToken(HttpContext context)
	{
		var authHeader = context.Request.Headers.Authorization.ToString();
		if (string.IsNullOrWhiteSpace(authHeader) ||
			!authHeader.StartsWith(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		return authHeader[JwtBearerDefaults.AuthenticationScheme.Length..].Trim();
	}

	private static class Constants
	{
		public const string ValidatedTokenKey = "ValidatedToken";
		internal static class Paths
		{
			internal const string HealthPath = "/health";
			internal const string AlternativeHealthPath = "/_health";
			internal const string WellKnownConfigPath = "/.well-known/openid-configuration";
		}

		internal static class ErrorCodes
		{
			internal const string UnauthorizedError = "unauthorized";
			internal const string TokenExpiredError = "token_expired";
			internal const string InvalidTokenError = "invalid_token";
			internal const string ServiceUnavailableError = "service_unavailable";
		}

		internal static class ErrorMessages
		{
			internal const string ServiceUnavailableMessage = "Authentication service is temporarily unavailable";
			internal const string MissingAuthHeaderMessage = "Missing or invalid Authorization header";
			internal const string TokenExpiredMessage = "The access token has expired";
			internal const string InvalidTokenMessage = "Token validation failed";
		}
	}
}

