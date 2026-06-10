using Bizca.OpenId.Server.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bizca.OpenId.Server.Extensions;

internal static class JwtBearerExtensions
{
	internal static void AddJwtBearerAuthentication(this IServiceCollection services)
	{
		services.AddScoped<TokenValidationParametersFactory>();
		services.AddScoped<JwtBearerOptionsConfigurator>();
		services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>, JwtBearerPostConfigureOptions>();
		services.AddAuthentication(options =>
		{
			options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
			options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
		})
		.AddJwtBearer();
	}
}

public sealed class JwtBearerPostConfigureOptions(IServiceScopeFactory serviceScopeFactory) : IPostConfigureOptions<JwtBearerOptions>
{
	public void PostConfigure(string? name, JwtBearerOptions options)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var configurator = scope.ServiceProvider.GetRequiredService<JwtBearerOptionsConfigurator>();
		configurator.Configure(options);
	}
}
