using System.Collections.Generic;

namespace Bizca.Sdk.SharedKernel;

public sealed record ValidationError : Error
{
	public ValidationError(params Error[] errors) : base("Validation.General", "One or more validation errors occurred.", ErrorType.Validation)
	{
		Errors = errors;
	}

	public IReadOnlyList<Error> Errors { get; }
}