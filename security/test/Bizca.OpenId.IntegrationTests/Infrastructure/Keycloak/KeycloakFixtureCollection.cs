using Xunit;

namespace Bizca.OpenId.IntegrationTests.Infrastructure.Keycloak;

[CollectionDefinition(Name)]
public sealed class KeycloakFixtureCollection : ICollectionFixture<KeycloakOpenIdApiFixture>
{
	internal const string Name = "Keycloak Fixture Collection";
}