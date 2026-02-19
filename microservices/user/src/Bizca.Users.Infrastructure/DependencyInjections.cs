using System;
using Bizca.Sdk.SharedKernel;
using Bizca.Users.Infrastructure.Context;
using Bizca.Users.Infrastructure.Time;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bizca.Users.Infrastructure;

public static class DependencyInjections
{
	public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
	{
		services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
		services.AddDatabase(configuration);
	}
	private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("database");
		services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString, sqlOptions =>
		{
			sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
			sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
		}));
	}
}