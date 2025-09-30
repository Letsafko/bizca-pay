using Bizca.Users.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Bizca.Users.Infrastructure.Context;

public class ApplicationContext : DbContext
{
	private static readonly ILoggerFactory StaticLoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());

	public ApplicationContext()
	{
	}

	public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
	{
	}

	public virtual DbSet<UserChannelConfirmation> UserChannelConfirmations => Set<UserChannelConfirmation>();
	public virtual DbSet<EconomicActivity> EconomicActivities => Set<EconomicActivity>();
	public virtual DbSet<UserChannel> UserChannels => Set<UserChannel>();
	public virtual DbSet<Civility> Civilities => Set<Civility>();
	public virtual DbSet<Password> Passwords => Set<Password>();
	public virtual DbSet<Address> Addresses => Set<Address>();
	public virtual DbSet<Country> Countries => Set<Country>();
	public virtual DbSet<Partner> Partners => Set<Partner>();
	public virtual DbSet<Channel> Channels => Set<Channel>();
	public virtual DbSet<User> Users => Set<User>();

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		base.OnConfiguring(optionsBuilder);

		optionsBuilder.EnableDetailedErrors()
#if DEBUG
					.UseLoggerFactory(StaticLoggerFactory)
#endif
					.EnableSensitiveDataLogging().UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
	}

	protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
	{
		base.ConfigureConventions(configurationBuilder);
		configurationBuilder.Properties<string>().AreUnicode();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.UseCollation("French_CI_AI");
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationContext).Assembly);
	}
}
