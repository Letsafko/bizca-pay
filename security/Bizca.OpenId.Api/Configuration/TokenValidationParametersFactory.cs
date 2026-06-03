using System.IdentityModel.Tokens.Jwt;
using Bizca.OpenId.Infrastructure.Keycloak;
using Bizca.OpenId.Infrastructure.Keycloak.SigningKeys;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bizca.OpenId.Api.Configuration;

public sealed class TokenValidationParametersFactory(
	IOptions<KeycloakOptions> keycloakOptionsAccessor,
	CertificatesManager certificatesManager)
{
	private readonly KeycloakOptions _keycloakOptions = keycloakOptionsAccessor.Value;
	public TokenValidationParameters Create()
	{
		return new TokenValidationParameters
		{
			IssuerSigningKeyResolver = (_, _, _, _) => certificatesManager.GetSigningKeysAsync().Result,
			NameClaimType = JwtRegisteredClaimNames.Sub,
			ValidAudience = _keycloakOptions.ClientId,
			ValidIssuer = _keycloakOptions.Authority,
			ValidateIssuerSigningKey = true,
			RoleClaimType = "realm_access",
			ValidateLifetime = true,
			ValidateAudience = true,
			ValidateIssuer = true,
		};
	}
}




