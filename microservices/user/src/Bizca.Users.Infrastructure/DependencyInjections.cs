using Bizca.Sdk.SharedKernel;
using Bizca.Sdk.SharedKernel.Extensions;
using Bizca.Users.Infrastructure.Context;
using Bizca.Users.Infrastructure.Context.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bizca.Users.Infrastructure;

public static class DependencyInjections
{
	public static void AddInfrastructure(this IServiceCollection services)
	{
		services.ConfigureOptions<DatabaseOptionsSetup>();
		services.AddTimeProvider();
		services.AddDatabase();
	}
	private static void AddDatabase(this IServiceCollection services)
	{
		services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
		{
			var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
			options.UseNpgsql(databaseOptions.ConnectionString, npgsqlOptions =>
			{
				npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
				npgsqlOptions.EnableRetryOnFailure(maxRetryCount: databaseOptions.MaxRetryCount);
				npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeout);
			});

			var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
			options.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
			options.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
			options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			options.UseLoggerFactory(loggerFactory);
			options.UseCamelCaseNamingConvention();
		});
	}
}