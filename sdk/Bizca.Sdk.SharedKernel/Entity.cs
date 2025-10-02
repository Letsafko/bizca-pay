using System;
using System.Collections.Generic;

namespace Bizca.Sdk.SharedKernel;

public abstract class Entity(DateTimeOffset createdDatetime, DateTimeOffset lastModifiedDatetime)
{
	private readonly List<DomainEvent> _domainEvents = [];
	public DateTimeOffset CreatedDatetime { get; } = createdDatetime;
	public DateTimeOffset LastModifiedDatetime { get; private set; } = lastModifiedDatetime;
	public IReadOnlyList<DomainEvent> DomainEvents => [.. _domainEvents];

	protected void AddDomainEvent(DomainEvent domainEvent)
	{
		_domainEvents.Add(domainEvent);
	}

	public void ClearDomainEvents()
	{
		_domainEvents.Clear();
	}
}
