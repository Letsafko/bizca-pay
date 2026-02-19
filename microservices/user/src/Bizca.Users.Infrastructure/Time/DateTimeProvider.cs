using System;
using Bizca.Sdk.SharedKernel;

namespace Bizca.Users.Infrastructure.Time;

public sealed class DateTimeProvider : IDateTimeProvider
{
	public DateTime UtcNow => DateTime.UtcNow;
}