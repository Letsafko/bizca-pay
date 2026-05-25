using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Keycloak configuration
var keycloak = builder
    .AddContainer("keycloak", "quay.io/keycloak/keycloak", "latest")
    .WithHttpEndpoint(port: 8080, targetPort: 8080, name: "http")
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin")
    .WithArgs("start-dev")
    .WithBindMount("keycloak-data", "/opt/keycloak/data");

// PostgreSQL database for Users service
const string databaseName = "bizca-users";
const string resourceName = "database";

var postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume()
    .WithPgWeb();

var database = postgres.AddDatabase(resourceName, databaseName);

// OpenID API (authentication service)
builder
    .AddProject<Bizca_OpenId_Api>("openid-api")
    .WaitFor(keycloak);

// Users API
builder
    .AddProject<Bizca_Users_Api>("users-api")
    .WithReference(database, connectionName: resourceName)
    .WaitFor(database);

await builder.Build().RunAsync();


