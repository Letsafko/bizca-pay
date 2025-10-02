using System;

namespace Bizca.Sdk.SharedKernel;

public abstract class DomainEvent(DateTime creationDateUtc, Guid correlationId)
{
	protected DomainEvent(DateTime creationDateUtc) : this(creationDateUtc, Guid.NewGuid())
	{
	}

	public DateTime CreationDateUtc { get; } = creationDateUtc;
	public Guid CorrelationId { get; } = correlationId;
}