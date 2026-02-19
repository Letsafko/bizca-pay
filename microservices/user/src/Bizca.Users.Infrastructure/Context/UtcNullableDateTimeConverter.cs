using System;

namespace Bizca.Users.Infrastructure.Context;

public abstract class UtcNullableDateTimeConverter()
	: Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(v => v.HasValue ? v.Value.ToUniversalTime() : v, v => v);