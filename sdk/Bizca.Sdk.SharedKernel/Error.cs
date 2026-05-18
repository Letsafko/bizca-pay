namespace Bizca.Sdk.SharedKernel;

public record Error
{
	public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);
	public static readonly Error NullValue = new("General.Null", "Null value was provided", ErrorType.Failure);
	protected Error(string code, string? description, ErrorType type)
	{
		Code = code;
		Type = type;
		Description = description;
	}

	public string Code { get; }

	public string? Description { get; }

	public ErrorType Type { get; }

	public static Error Failure(string code, string? description = null)
	{
		return new Error(code, description, ErrorType.Failure);
	}

	public static Error NotFound(string code, string? description = null)
	{
		return new Error(code, description, ErrorType.NotFound);
	}

	public static Error Problem(string code, string? description = null)
	{
		return new Error(code, description, ErrorType.Problem);
	}

	public static Error Conflict(string code, string? description = null)
	{
		return new Error(code, description, ErrorType.Conflict);
	}
}
