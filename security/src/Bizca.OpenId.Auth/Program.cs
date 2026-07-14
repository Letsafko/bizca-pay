using Bizca.OpenId.Auth.Extensions;
using Bizca.OpenId.Auth.Infrastructure;
using Bizca.OpenId.Infrastructure;
using Bizca.OpenId.Infrastructure.Keycloak;
using Bizca.Sdk.Abstractions.Pipelines;
using Bizca.Sdk.Api;
using Bizca.Sdk.Api.MinimalApi;
using Bizca.Sdk.Api.OpenApi;
using Bizca.Sdk.Api.Options;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandlers(services =>
{
	services.AddExceptionHandler<KeycloakExceptionHandler>();
	services.AddExceptionHandler<GlobalExceptionHandler>();
});

builder.Services.AddOptionsWithValidation<KeycloakOptions>(KeycloakOptions.SectionName);
builder.Services.AddInfrastructure(
	typeof(Bizca.OpenId.Application.Usecases.Tokens.Create.Handler),
	typeof(Bizca.OpenId.Application.Usecases.Auth.Register.Handler),
	typeof(Bizca.OpenId.Application.Usecases.Auth.VerifyEmail.Handler),
	typeof(ValidationDecorator.RequestHandler<,>),
	typeof(LoggingDecorator.RequestHandler<,>));

builder.Services.AddValidatorsFromAssemblyContaining<Bizca.OpenId.Application.Usecases.Tokens.Create.Validator>();
builder.Services.AddJwtBearerAuthentication();
builder.Services.AddBizcaOpenApi(builder.Configuration);
builder.Services.AddEndpoints(typeof(Program).Assembly);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local"))
{
    app.UseBizcaOpenApi();
}

app.UseAuthentication();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "bizca-openid-api" }))
    .WithTags("Health")
    .WithSummary("Health check endpoint");

var versionedGroup = app.GetVersionedGroupBuilder();
app.MapEndpoints(routeGroupBuilder: versionedGroup);

await app.RunAsync();





