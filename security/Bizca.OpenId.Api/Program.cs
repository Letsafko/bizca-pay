using Bizca.OpenId.Api.Endpoints;
using Bizca.OpenId.Api.Keycloak;
using Bizca.OpenId.Api.Options;
using Bizca.Sdk.Api.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureOptions<KeycloakOptionsSetup>();
builder.Services.AddHttpClient<KeycloakClient>();
builder.Services.AddSingleton<JwksCache>();
builder.Services.AddBizcaOpenApi(builder.Configuration);

var app = builder.Build();
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.UseBizcaOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "bizca-openid-api" }))
    .WithTags("Health")
    .WithSummary("Health check endpoint");

app.MapTokenEndpoint();
app.MapRefreshEndpoint();
app.MapLogoutEndpoint();

await app.RunAsync();





