using Bizca.Users.Infrastructure;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/", static () => "Hello World!");

await app.RunAsync();
