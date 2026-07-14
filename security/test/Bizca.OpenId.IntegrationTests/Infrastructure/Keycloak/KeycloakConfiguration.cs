namespace Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;

public sealed class KeycloakConfiguration : IOpenIdConfiguration
{
	public required string ClientSecret { get; init; }
	public required string BaseAddress { get; init; }
	public required string Authority { get; init; }
	public required string ClientId { get; init; }
	public required string Realm { get; init; }
}