using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Bizca.Sdk.Api.OpenId.Middleware;

public sealed class ClaimsEnrichmentMiddleware(RequestDelegate next)
{
	public async Task InvokeAsync(HttpContext context)
	{
		if (context.User.Identity?.IsAuthenticated == true)
		{
			EnrichHeaders(context);
		}

		await next(context);
	}

	private static void EnrichHeaders(HttpContext context)
	{
		var claims = context.User.Claims.ToList();

		if(TryGetClaimValue(claims, claimTypes: [ClaimTypes.NameIdentifier, Constants.ClaimsTypes.SubClaimType], out var userId))
		{
			context.Request.Headers[Constants.Headers.UserIdHeader] = userId;
		}

		if(TryGetClaimValue(claims, claimTypes: [Constants.ClaimsTypes.TenantIdClaimType, Constants.ClaimsTypes.OrganizationIdClaimType], out var tenantId))
		{
			context.Request.Headers[Constants.Headers.TenantIdHeader] = tenantId;
		}

		if(TryGetClaimValue(claims, claimTypes: [ClaimTypes.Email, Constants.ClaimsTypes.EmailClaimType], out var email))
		{
			context.Request.Headers[Constants.Headers.UserEmailHeader] = email;
		}

		if(TryGetClaimValue(claims, claimTypes: [ClaimTypes.Name, Constants.ClaimsTypes.PreferredUsernameClaimType], out var username))
		{
			context.Request.Headers[Constants.Headers.UserNameHeader] = username;
		}

		var roles = claims
			.Where(c => c.Type is ClaimTypes.Role or Constants.ClaimsTypes.RoleClaimType)
			.Select(c => c.Value)
			.ToList();

		if(roles.Count == 0)
		{
			return;
		}

		var rolesValue = string.Join(Constants.RolesSeparator, roles);
		context.Request.Headers[Constants.Headers.RolesHeader] = rolesValue;
	}

	private static bool TryGetClaimValue(List<Claim> claims, string[] claimTypes, out string? claimValue)
	{
		claimValue = claims.FirstOrDefault(c => claimTypes.Contains(c.Type))?.Value;
		return !string.IsNullOrWhiteSpace(claimValue);
	}
}

