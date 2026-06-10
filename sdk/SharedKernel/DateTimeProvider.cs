using System;

namespace Bizca.Sdk.SharedKernel;

public sealed class DateTimeProvider : IDateTimeProvider
{
	public DateTime UtcNow => DateTime.UtcNow;
}