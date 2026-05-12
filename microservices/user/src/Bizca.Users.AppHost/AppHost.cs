using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
	.AddPostgres("postgres")
	.WithDataVolume("postgres-data")
	.WithPgWeb();

const string databaseName = "bizca-users";
const string resourceName = "database";
var database	= postgres.AddDatabase(resourceName, databaseName);

builder.AddProject<Bizca_Users_Api>("api")
	.WithReference(database, connectionName: resourceName)
	.WaitFor(database);

await builder.Build().RunAsync();