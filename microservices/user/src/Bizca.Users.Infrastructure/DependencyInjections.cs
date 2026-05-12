using Bizca.Sdk.SharedKernel;
using Bizca.Users.Infrastructure.Context;
using Bizca.Users.Infrastructure.Context.Options;
using Bizca.Users.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bizca.Users.Infrastructure;

public static class DependencyInjections
{
	private static readonly ILoggerFactory StaticLoggerFactory = LoggerFactory.Create(static builder => builder.AddConsole());

	public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.ConfigureOptions<DatabaseOptionsSetup>();
		services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
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

			options.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);
			options.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
			options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
			options.UseLoggerFactory(StaticLoggerFactory);
			options.UseSnakeCaseNamingConvention();
		});
	}
}