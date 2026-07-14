using System;
using System.Reflection;

namespace Bizca.OpenId.Infrastructure.Keycloak.Exceptions;

internal static class EnumerationExtensions
{
	private const int DefaultHttpStatusCode = 400;

	internal static string? GetDescription(this Enum value)
	{
		var attribute = value.GetCustomAttribute<KeycloakExceptionAttribute>();
		return attribute?.Description;
	}

	internal static int GetHttpStatusCode(this Enum value)
	{
		var attribute = value.GetCustomAttribute<KeycloakExceptionAttribute>();
		return attribute?.HttpStatusCode ?? DefaultHttpStatusCode;
	}

	internal static string GetName(this Enum value)
	{
		return value.ToString();
	}

	private static T? GetCustomAttribute<T>(this Enum value)
		where T : Attribute
	{
		var type = value.GetType();
		var name = Enum.GetName(type, value);
		if (string.IsNullOrWhiteSpace(name))
		{
			return null;
		}

		var field = type.GetField(name);
		return field?.GetCustomAttribute<T>();
	}
}