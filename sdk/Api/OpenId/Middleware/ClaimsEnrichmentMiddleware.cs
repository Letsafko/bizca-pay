using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Bizca.Sdk.Api.OpenId.Middleware;

/// <summary>
/// Enriches HTTP headers with claims extracted from the validated JWT.
/// Downstream microservices receive X-User-Id, X-Roles, X-Tenant-Id.
/// </summary>
public sealed class ClaimsEnrichmentMiddleware(RequestDelegate next, ILogger<ClaimsEnrichmentMiddleware> logger)
{
	private static class Constants
	{
		public const string SubClaimType = "sub";
		public const string RoleClaimType = "role";
		public const string EmailClaimType = "email";
		public const string PreferredUsernameClaimType = "preferred_username";
		public const string TenantIdClaimType = "tenant_id";
		public const string OrganizationIdClaimType = "organization_id";

		public const string UserIdHeader = "X-User-Id";
		public const string RolesHeader = "X-Roles";
		public const string TenantIdHeader = "X-Tenant-Id";
		public const string UserEmailHeader = "X-User-Email";
		public const string UserNameHeader = "X-User-Name";
		public const string RolesSeparator = ",";
	}

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.User.Identity?.IsAuthenticated == true)
		{
			EnrichHeaders(context);
		}

		await next(context).ConfigureAwait(false);
	}

	private void EnrichHeaders(HttpContext context)
	{
		var claims = context.User.Claims.ToList();
		var userId = claims.FirstOrDefault(c => c.Type is ClaimTypes.NameIdentifier or Constants.SubClaimType)?.Value;
		if (!string.IsNullOrEmpty(userId))
		{
			context.Request.Headers[Constants.UserIdHeader] = userId;
			logger.LogDebug("Enriched {Header}: {UserId}", Constants.UserIdHeader, userId);
		}

		var roles = claims
			.Where(c => c.Type is ClaimTypes.Role or Constants.RoleClaimType)
			.Select(c => c.Value)
			.ToList();

		if (roles.Count > 0)
		{
			var rolesValue = string.Join(Constants.RolesSeparator, roles);
			context.Request.Headers[Constants.RolesHeader] = rolesValue;
			logger.LogDebug("Enriched {Header}: {Roles}", Constants.RolesHeader, rolesValue);
		}

		var tenantId = claims.FirstOrDefault(c => c.Type is Constants.TenantIdClaimType or Constants.OrganizationIdClaimType)?.Value;
		if (!string.IsNullOrEmpty(tenantId))
		{
			context.Request.Headers[Constants.TenantIdHeader] = tenantId;
			logger.LogDebug("Enriched {Header}: {TenantId}", Constants.TenantIdHeader, tenantId);
		}

		var email = claims.FirstOrDefault(c => c.Type is ClaimTypes.Email or Constants.EmailClaimType)?.Value;
		if (!string.IsNullOrEmpty(email))
		{
			context.Request.Headers[Constants.UserEmailHeader] = email;
			logger.LogDebug("Enriched {Header}: {Email}", Constants.UserEmailHeader, email);
		}

		var username = claims.FirstOrDefault(c => c.Type is Constants.PreferredUsernameClaimType or ClaimTypes.Name)?.Value;
		if(string.IsNullOrEmpty(username))
		{
			return;
		}

		context.Request.Headers[Constants.UserNameHeader] = username;
		logger.LogDebug("Enriched {Header}: {Username}", Constants.UserNameHeader, username);
	}
}

