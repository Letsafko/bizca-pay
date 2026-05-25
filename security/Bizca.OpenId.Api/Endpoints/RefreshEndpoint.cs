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
/// Refresh token endpoint for renewing access tokens.
/// </summary>
public static class RefreshEndpoint
{
    public static void MapRefreshEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("/auth/refresh", HandleAsync)
            .WithName("RefreshToken")
            .WithTags("Authentication")
            .WithSummary("Refresh an access token")
            .WithDescription("Exchanges a refresh token for a new access token.")
            .Produces<TokenResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] RefreshRequest request,
        [FromServices] KeycloakClient keycloakClient,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.Problem(
                title: "Invalid request",
                detail: "RefreshToken is required",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var response = await keycloakClient.RefreshTokenAsync(
                request.RefreshToken,
                cancellationToken).ConfigureAwait(false);

            return Results.Ok(response);
        }
        catch (KeycloakException ex)
        {
            return Results.Problem(
                title: "Token refresh failed",
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
/// Refresh token request payload.
/// </summary>
public sealed record RefreshRequest(string RefreshToken)
{
    [System.Text.Json.Serialization.JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = RefreshToken;
}



