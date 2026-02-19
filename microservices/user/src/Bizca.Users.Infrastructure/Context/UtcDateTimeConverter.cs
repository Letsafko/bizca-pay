using System;

namespace Bizca.Users.Infrastructure.Context;

public abstract class UtcDateTimeConverter()
	: Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(v => v.ToUniversalTime(), v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
