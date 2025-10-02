using System;

namespace Bizca.Sdk.SharedKernel;

public interface IDateTimeProvider
{
	DateTime UtcNow { get; }
}
