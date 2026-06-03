using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Bizca.OpenId.Api.Configuration;
public sealed class JwtBearerOptionsConfigurator(TokenValidationParametersFactory tokenValidationParametersFactory)
{
	public void Configure(JwtBearerOptions options)
	{
		options.TokenValidationParameters = tokenValidationParametersFactory.Create();
		options.Events = CreateAuthenticationEvents();
		options.ConfigurationManager = null!;
		options.MetadataAddress = null!;
	}

	private static JwtBearerEvents CreateAuthenticationEvents()
	{
		return new JwtBearerEvents
		{
			OnAuthenticationFailed = HandleAuthenticationFailure
		};
	}

	private static Task HandleAuthenticationFailure(AuthenticationFailedContext context)
	{
		if(context.Exception is not SecurityTokenExpiredException)
		{
			return Task.CompletedTask;
		}

		context.Response.StatusCode = 401;
		context.Response.ContentType = "application/json";
		context.Fail(context.Exception);

		return Task.CompletedTask;
	}
}


