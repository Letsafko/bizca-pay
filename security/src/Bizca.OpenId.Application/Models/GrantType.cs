using System;
using System.Linq;
using Ardalis.SmartEnum;

namespace Bizca.OpenId.Application.Models;

public sealed class GrantType : SmartEnum<GrantType>
{
	public static readonly GrantType AuthorizationCode   = new ("authorization_code", 1);
	public static readonly GrantType ClientCredentials   = new ("client_credentials", 2);
	public static readonly GrantType RefreshToken        = new ("refresh_token", 3);
	private GrantType(string name, int value) : base(name, value)
	{
	}

	public static bool IsDefined(string? name)
	{
		return !string.IsNullOrWhiteSpace(name) && List.Any(x => string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
	}
}