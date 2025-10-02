using System;

namespace Bizca.Sdk.SharedKernel;

public abstract class Entity<TId>(DateTimeOffset createdDatetime, DateTimeOffset lastModifiedDatetime) : Entity(createdDatetime, lastModifiedDatetime)
{
	public TId Id { get; private set; } = default!;
}