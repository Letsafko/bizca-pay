using System;
using System.IdentityModel.Tokens.Jwt;
using Bizca.OpenId.Infrastructure.Keycloak;
using Bizca.OpenId.Infrastructure.Keycloak.SigningKeys;
using Bizca.Sdk.Api.OpenId;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Bizca.OpenId.Server.Configuration;

public sealed class TokenValidationParametersFactory(
	IOptions<KeycloakOptions> keycloakOptionsAccessor,
	IOptions<OpenIdOptions> openIdOptionsAccessor,
	CertificatesManager certificatesManager)
{
	private readonly KeycloakOptions _keycloakOptions = keycloakOptionsAccessor.Value;
	private readonly OpenIdOptions _openIdOptions = openIdOptionsAccessor.Value;
	public TokenValidationParameters Create()
	{
		return new TokenValidationParameters
		{
			IssuerSigningKeyResolver = (_, _, _, _) => certificatesManager.GetSigningKeysAsync().Result,
			ValidAlgorithms = [SecurityAlgorithms.RsaSha256, SecurityAlgorithms.EcdsaSha256],
			ClockSkew = TimeSpan.FromSeconds(_openIdOptions.ClockSkewSeconds),
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




