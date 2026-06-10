using System;
using System.Text.Json.Serialization;

namespace Bizca.OpenId.Infrastructure.Keycloak.Exceptions;

public sealed class KeycloakException : Exception
{
	private const int MaxDescriptionLength = 1000;

	public string Code { get; private set; }

	public string? Description { get; private set; }

	public int HttpStatusCode { get; }

	[JsonConstructor]
	internal KeycloakException(
		string code,
		string? description,
		int httpStatusCode,
		Exception? innerException = null)
		: base($"{code}:{description}", innerException)
	{
		ValidateState(code, description, httpStatusCode);
		HttpStatusCode = httpStatusCode;
		Description = description;
		Code = code;
	}

	internal KeycloakException(Enum errCode, Exception? innerException = null)
		: this(
			errCode.GetName(),
			errCode.GetDescription(),
			errCode.GetHttpStatusCode(),
			innerException)
	{
	}

	private static void ValidateState(string code, string? description, int httpStatusCode)
	{
		if (string.IsNullOrWhiteSpace(code))
		{
			throw new ArgumentException("Code cannot be null or whitespace.", nameof(code));
		}

		if (description is { Length: > MaxDescriptionLength })
		{
			throw new ArgumentException($"Description is over {MaxDescriptionLength} characters long", nameof(description));
		}

		if (httpStatusCode is < 100 or > 599)
		{
			throw new ArgumentOutOfRangeException(nameof(httpStatusCode), $"Invalid value for HttpStatusCode: {httpStatusCode}. The value must be between 100 and 599, inclusive.");
		}
	}
}