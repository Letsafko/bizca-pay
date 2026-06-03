namespace Bizca.Sdk.Api.OpenId.Middleware;

internal static class Constants
{
	internal const string RolesSeparator = ",";
	internal static class ClaimsTypes
	{
		internal const string PreferredUsernameClaimType = "preferred_username";
		internal const string OrganizationIdClaimType = "organization_id";
		internal const string TenantIdClaimType = "tenant_id";
		internal const string EmailClaimType = "email";
		internal const string RoleClaimType = "role";
		internal const string SubClaimType = "sub";
	}

	internal static class Headers
	{
		internal const string UserEmailHeader = "X-User-Email";
		internal const string TenantIdHeader = "X-Tenant-Id";
		internal const string UserNameHeader = "X-User-Name";
		internal const string UserIdHeader = "X-User-Id";
		internal const string RolesHeader = "X-Roles";
	}
}