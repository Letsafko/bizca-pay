using System;
using System.Collections.Generic;
using System.Linq;

namespace Bizca.Sdk.SharedKernel;

public abstract class ValueObject : IEquatable<ValueObject>, IEqualityComparer<ValueObject>
{
	protected abstract IEnumerable<object?> GetEqualityComponents();
	public bool Equals(ValueObject? other)
	{
		return other is not null &&
				(ReferenceEquals(this, other) || (other.GetType() == GetType() && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents())));
	}

	public override bool Equals(object? obj)
	{
		return obj is ValueObject o && Equals(o);
	}

	public override int GetHashCode()
	{
		return GetEqualityComponents()
				.Select(static x => x != null ? x.GetHashCode() : 0)
				.Aggregate(static (x, y) => x ^ y);
	}

	public static bool operator ==(ValueObject? a, ValueObject? b)
	{
		return (a is null && b is null) ||
			   (a is not null && b is not null && a.Equals(b));
	}

	public static bool operator !=(ValueObject? a, ValueObject? b)
	{
		return !(a == b);
	}

	public bool Equals(ValueObject? x, ValueObject? y)
	{
		return ReferenceEquals(x, y) || (x is not null && y is not null && x.Equals(y));
	}

	public int GetHashCode(ValueObject obj)
	{
		return obj.GetHashCode();
	}
}