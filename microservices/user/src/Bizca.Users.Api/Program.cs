using Bizca.Users.Api.Extensions;
using Bizca.Users.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if(app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
	await app.ApplyMigrationsAsync();
}

app.MapGet("/", static () => "Hello World!");

await app.RunAsync();
