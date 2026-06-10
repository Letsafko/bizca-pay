using Aspire.Hosting;
using Bizca.Services.AppHost.Constants;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var keycloak = builder
	.AddContainer(KeycloakConstants.ResourceName, KeycloakConstants.Image, KeycloakConstants.Version)
	.WithHttpEndpoint(port: KeycloakConstants.Port, targetPort: KeycloakConstants.Port, name: "http")
	.WithEnvironment("KEYCLOAK_ADMIN", builder.Configuration[ConfigurationKeys.Keycloak.AdminUser] ?? "admin")
	.WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", builder.Configuration[ConfigurationKeys.Keycloak.AdminPassword] ?? "admin")
	.WithEnvironment("KC_HEALTH_ENABLED", "true")
	.WithEnvironment("KC_METRICS_ENABLED", "true")
	.WithVolume("keycloak-data", "/opt/keycloak/data")
	.WithArgs("start-dev");

var postgres = builder
    .AddPostgres(PostgresConstants.ResourceName)
    .WithDataVolume()
    .WithPgWeb()
    .WithEnvironment("POSTGRES_INITDB_ARGS", PostgresConstants.InitDbArgs);

var database = postgres.AddDatabase(PostgresConstants.DatabaseResourceName, PostgresConstants.DatabaseName);

builder
    .AddProject<Bizca_OpenId_Server>(ServiceConstants.OpenIdApiName)
    .WithReplicas(ServiceConstants.DefaultReplicas)
    .WaitFor(keycloak);

builder
    .AddProject<Bizca_Users_Api>(ServiceConstants.UsersApiName)
    .WithReference(database, connectionName: PostgresConstants.DatabaseResourceName)
    .WithReplicas(ServiceConstants.DefaultReplicas)
    .WaitFor(database);

await builder.Build().RunAsync();
