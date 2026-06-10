using Bizca.OpenId.Infrastructure.Keycloak.SigningKeys;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.OpenId.Infrastructure.Keycloak.Extensions;

internal static class JwksCacheExtensions
{
	internal static void AddJwksCache(this IServiceCollection services)
	{
		services.AddScoped<ISigningKeySource, KeycloakSigningKeySource>();
		services.Decorate<ISigningKeySource, CachedSigningKeySource>();
		services.Decorate<ISigningKeySource, ThreadSafeSigningKeySource>();
		services.AddScoped<CertificatesManager>();
	}
}