using System;
using System.ComponentModel;

namespace Bizca.OpenId.Infrastructure.Keycloak.Exceptions;

[AttributeUsage(AttributeTargets.Field)]
public sealed class KeycloakExceptionAttribute(string description) : DescriptionAttribute(description)
{
	public int HttpStatusCode { get; }

	public KeycloakExceptionAttribute(string description, int httpStatusCode)
		: this(description)
	{
		HttpStatusCode = httpStatusCode;
	}
}