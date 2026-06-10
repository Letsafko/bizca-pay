using System.Linq;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bizca.Sdk.Api.Options;

public class FluentValidationOptions<TOptions>(
	IServiceScopeFactory serviceScopeFactory) : IValidateOptions<TOptions>
	where TOptions : class
{
	public ValidateOptionsResult Validate(string? name, TOptions options)
	{
		var scope = serviceScopeFactory.CreateScope();
		var validator = scope.ServiceProvider.GetService<IValidator<TOptions>>();
		if(validator is null)
		{
			return ValidateOptionsResult.Skip;
		}

		var result = validator.Validate(options);
		if(result.IsValid)
		{
			return ValidateOptionsResult.Success;
		}

		var typeName = options.GetType().Name;
		var errors = result.Errors.Select(e => $"Validation failed for {typeName}.{e.PropertyName} with error: '{e.ErrorMessage}'").ToArray();

		return ValidateOptionsResult.Fail(errors);
	}
}