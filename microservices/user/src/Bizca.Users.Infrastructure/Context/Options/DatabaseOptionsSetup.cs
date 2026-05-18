using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bizca.Users.Infrastructure.Context.Options;

public sealed class DatabaseOptionsSetup(IConfiguration configuration) : IConfigureOptions<DatabaseOptions>
{
	private const string ConfigurationSectionName = nameof(DatabaseOptions);

	public void Configure(DatabaseOptions options)
	{
		configuration.GetSection(ConfigurationSectionName).Bind(options);
		var connectionString = configuration.GetConnectionString("database");
		options.ConnectionString = connectionString!;
	}
}