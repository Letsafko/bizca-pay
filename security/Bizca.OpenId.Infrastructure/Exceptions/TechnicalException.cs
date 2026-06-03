namespace Bizca.OpenId.Infrastructure.Exceptions;

public sealed class TechnicalException(string errorCode, string message) : Exception(message)
{
	public string ErrorCode { get; } = errorCode;
}