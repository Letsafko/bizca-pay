using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bizca.Users.Infrastructure.Context;

public class ApplicationDbContext : DbContext
{
	private static readonly ILoggerFactory StaticLoggerFactory = LoggerFactory.Create(static builder => builder.AddConsole());

	public ApplicationDbContext()
	{
	}

	public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
	{
	}

#if DEBUG
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);
		optionsBuilder
			.EnableDetailedErrors()
			.UseLoggerFactory(StaticLoggerFactory)
			.EnableSensitiveDataLogging()
			.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
	}
#endif

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		base.ConfigureConventions(configurationBuilder);
		configurationBuilder.Properties<string>().AreUnicode(false);
		configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
		configurationBuilder.Properties<DateTime?>().HaveConversion<UtcNullableDateTimeConverter>();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.UseCollation("French_CI_AI");
		modelBuilder.ApplyConfiguration(new Configurations.ChannelTypeRefEntityConfiguration());
		modelBuilder.ApplyConfiguration(new Configurations.CivilityRefEntityConfiguration());
		modelBuilder.ApplyConfiguration(new Configurations.StatusRefEntityConfiguration());
		modelBuilder.ApplyConfiguration(new Configurations.AddressEntityConfiguration());
		modelBuilder.ApplyConfiguration(new Configurations.UserChannelConfirmationEntityConfiguration());
		modelBuilder.ApplyConfiguration(new Configurations.UserChannelEntityConfiguration());
		modelBuilder.ApplyConfiguration(new Configurations.UserEntityConfiguration());
	}
}
