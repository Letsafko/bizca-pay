namespace Bizca.Users.Infrastructure.Context.Options;

public sealed class DatabaseOptions
{
	public string ConnectionString { get; set; } = string.Empty;
	public bool EnableSensitiveDataLogging { get; init; }
	public bool EnableDetailedErrors { get; init; }
	public int CommandTimeout { get; init; }
	public int MaxRetryCount { get; init; }
}