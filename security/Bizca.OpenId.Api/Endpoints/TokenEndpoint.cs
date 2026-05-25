using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bizca.OpenId.Api.Keycloak;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Bizca.OpenId.Api.Endpoints;

/// <summary>
/// Token endpoint for OAuth2 token exchange.
/// </summary>
public static class TokenEndpoint
{
    public static void MapTokenEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/token", HandleAsync)
            .WithName("GetToken")
            .WithTags("Authentication")
            .WithSummary("Exchange authorization code for access token")
            .WithDescription("Supports Authorization Code + PKCE and Client Credentials flows.")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] TokenRequest request,
        [FromServices] KeycloakClient keycloakClient,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = request.GrantType switch
            {
                "authorization_code" => await keycloakClient.ExchangeCodeForTokenAsync(
                    request.Code ?? throw new ArgumentException("Code is required for authorization_code grant"),
                    request.RedirectUri ?? throw new ArgumentException("RedirectUri is required for authorization_code grant"),
                    request.CodeVerifier,
                    cancellationToken).ConfigureAwait(false),

                "client_credentials" => await keycloakClient.GetClientCredentialsTokenAsync(
                    cancellationToken).ConfigureAwait(false),

                _ => throw new ArgumentException($"Unsupported grant_type: {request.GrantType}")
            };

            return Results.Ok(response);
        }
        catch (KeycloakException ex)
        {
            return Results.Problem(
                title: "Token request failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    ["errorCode"] = ex.ErrorCode
                });
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                title: "Invalid request",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }
}

/// <summary>
/// Token request payload.
/// </summary>
public sealed record TokenRequest(
    string GrantType,
    string? Code,
    string? RedirectUri,
    string? CodeVerifier
)
{
    [System.Text.Json.Serialization.JsonPropertyName("grant_type")]
    public string GrantType { get; init; } = GrantType;

    [System.Text.Json.Serialization.JsonPropertyName("code")]
    public string? Code { get; init; } = Code;

    [System.Text.Json.Serialization.JsonPropertyName("redirect_uri")]
    public string? RedirectUri { get; init; } = RedirectUri;

    [System.Text.Json.Serialization.JsonPropertyName("code_verifier")]
    public string? CodeVerifier { get; init; } = CodeVerifier;
}



