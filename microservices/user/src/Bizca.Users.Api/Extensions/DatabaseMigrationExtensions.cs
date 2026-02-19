using System;
using System.Linq;
using System.Threading.Tasks;
using Bizca.Users.Infrastructure.Context;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bizca.Users.Api.Extensions;

internal static class DatabaseMigrationExtensions
{
	internal static async Task ApplyMigrationsAsync(this WebApplication app)
	{
		await using var scope = app.Services.CreateAsyncScope();
		var services = scope.ServiceProvider;
		var logger = services.GetRequiredService<ILogger<ApplicationDbContext>>();

		try
		{
			var context = services.GetRequiredService<ApplicationDbContext>();
			await context.Database.MigrateAsync();
		}
		catch (Exception ex)
		{
			logger.Error("An error occurred while migrating the database.", ex);
		}
	}
}

internal static partial class Log
{
	[LoggerMessage(EventId = 0, Level = LogLevel.Error, Message = "{message}")]
	public static partial void Error(this ILogger logger, string message, Exception? ex);
}