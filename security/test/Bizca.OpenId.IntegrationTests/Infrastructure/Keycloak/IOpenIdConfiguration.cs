namespace Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;

public interface IOpenIdConfiguration
{
	string ClientSecret { get; }
	string BaseAddress { get; }
	string Authority { get; }
	string ClientId { get; }
	string Realm { get; }
}