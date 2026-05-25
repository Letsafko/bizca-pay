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
/// Log out endpoint for token revocation.
/// </summary>
internal static class LogoutEndpoint
{
    public static void MapLogoutEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/logout", HandleAsync)
            .WithName("Logout")
            .WithTags("Authentication")
            .WithSummary("Revoke a token (logout)")
            .WithDescription("Revokes an access or refresh token, invalidating the session.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] LogoutRequest request,
        [FromServices] KeycloakClient keycloakClient,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Results.Problem(
                title: "Invalid request",
                detail: "Token is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var success = await keycloakClient.RevokeTokenAsync(
                request.Token,
                request.TokenTypeHint ?? "refresh_token",
                cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                return Results.Problem(
                    title: "Token revocation failed",
                    detail: "The token could not be revoked",
                    statusCode: StatusCodes.Status400BadRequest);
            }

            return Results.NoContent();
        }
        catch (KeycloakException ex)
        {
            return Results.Problem(
                title: "Logout failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest,
                extensions: new Dictionary<string, object?>
                {
                    ["errorCode"] = ex.ErrorCode
                });
        }
    }
}

/// <summary>
/// Logout request payload.
/// </summary>
public sealed record LogoutRequest(string Token, string? TokenTypeHint)
{
    [System.Text.Json.Serialization.JsonPropertyName("token")]
    public string Token { get; init; } = Token;

    [System.Text.Json.Serialization.JsonPropertyName("token_type_hint")]
    public string? TokenTypeHint { get; init; } = TokenTypeHint;
}



