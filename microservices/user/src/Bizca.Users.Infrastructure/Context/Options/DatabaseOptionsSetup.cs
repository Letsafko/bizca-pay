using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Bizca.Users.Infrastructure.Context.Options;

public sealed class DatabaseOptionsSetup(IConfiguration configuration) : IConfigureOptions<DatabaseOptions>
{
	private const string ConfigurationSectionName = nameof(DatabaseOptions);
	private readonly IConfiguration _configuration = configuration;

	public void Configure(DatabaseOptions options)
	{
		_configuration.GetSection(ConfigurationSectionName).Bind(options);
		var connectionString = _configuration.GetConnectionString("database");
		options.ConnectionString = connectionString!;
	}
}