using Microsoft.AspNetCore.Routing;

namespace Bizca.Sdk.Api.MinimalApi;

public interface IEndpoint
{
	void MapEndpoint(IEndpointRouteBuilder app);
}