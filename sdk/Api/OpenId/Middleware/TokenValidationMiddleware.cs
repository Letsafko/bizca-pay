using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Bizca.Sdk.Api.OpenId.Options;

namespace Bizca.Sdk.Api.OpenId.Middleware;

public sealed class TokenValidationMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<TokenValidationMiddleware> _logger;
	private readonly OpenIdOptions _options;
	private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;
	private readonly JwtSecurityTokenHandler _tokenHandler = new();

	public TokenValidationMiddleware(
		RequestDelegate next,
		ILogger<TokenValidationMiddleware> logger,
		IOptions<OpenIdOptions> options)
	{
		_next = next;
		_logger = logger;
		_options = options.Value;

		var documentRetriever = new HttpDocumentRetriever
		{
			RequireHttps = _options.RequireHttpsMetadata
		};

		_configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
			metadataAddress: $"{_options.Authority.TrimEnd('/')}{Constants.Paths.WellKnownConfigPath}",
			configRetriever: new OpenIdConnectConfigurationRetriever(),
			docRetriever: documentRetriever
		);
	}

	public async Task InvokeAsync(HttpContext context)
	{
		// Skip validation for health checks and non-protected routes
		if (context.Request.Path.StartsWithSegments(Constants.Paths.HealthPath) ||
			context.Request.Path.StartsWithSegments(Constants.Paths.AlternativeHealthPath))
		{
			await _next(context).ConfigureAwait(false);
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
			}).ConfigureAwait(false);
			return;
		}

		try
		{
			var configuration = await _configurationManager.GetConfigurationAsync(context.RequestAborted).ConfigureAwait(false);

			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = _options.Issuer,
				ValidateAudience = true,
				ValidAudience = _options.Audience,
				ValidateLifetime = true,
				ClockSkew = TimeSpan.FromSeconds(_options.ClockSkewSeconds),
				IssuerSigningKeys = configuration.SigningKeys,
				ValidateIssuerSigningKey = true
			};

			var principal = _tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

			// Attach validated claims to HttpContext
			context.User = principal;
			context.Items[Constants.ValidatedTokenKey] = validatedToken;

			await _next(context).ConfigureAwait(false);
		}
		catch (SecurityTokenExpiredException)
		{
			_logger.LogWarning("Token has expired");
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.WriteAsJsonAsync(new
			{
				error = Constants.ErrorCodes.TokenExpiredError,
				message = Constants.ErrorMessages.TokenExpiredMessage
			}).ConfigureAwait(false);
		}
		catch (SecurityTokenException ex)
		{
			_logger.LogWarning(ex, "Token validation failed: {Message}", ex.Message);
			context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			await context.Response.WriteAsJsonAsync(new
			{
				error = Constants.ErrorCodes.InvalidTokenError,
				message = Constants.ErrorMessages.InvalidTokenMessage
			}).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "An error occured while validating token.");
			context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
			await context.Response.WriteAsJsonAsync(new
			{
				error = Constants.ErrorCodes.ServiceUnavailableError,
				message = Constants.ErrorMessages.ServiceUnavailableMessage
			}).ConfigureAwait(false);
		}
	}

	private static string? ExtractBearerToken(HttpContext context)
	{
		var authHeader = context.Request.Headers.Authorization.ToString();

		if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith(Constants.BearerScheme, StringComparison.OrdinalIgnoreCase))
		{
			return null;
		}

		return authHeader[Constants.BearerScheme.Length..].Trim();
	}

	private static class Constants
	{
		public const string ValidatedTokenKey = "ValidatedToken";
		public const string BearerScheme = "Bearer ";
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

