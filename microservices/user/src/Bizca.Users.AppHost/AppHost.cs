using Aspire.Hosting;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var sqlPassword = builder.AddParameter("sql-password", secret: false);

var database = builder
	.AddSqlServer("sqlserver", password: sqlPassword, port: 1433)
	.WithDataVolume("mssql-data")
	.WithDbGate()
	.AddDatabase("database", "bizca-users");

builder.AddProject<Bizca_Users_Api>("api")
	.WithReference(database)
	.WaitFor(database);

await builder.Build().RunAsync();