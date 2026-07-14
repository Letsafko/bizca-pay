using System;

namespace Bizca.Sdk.SharedKernel;

public abstract class DomainEvent(DateTimeOffset creationDateUtc, Guid correlationId)
{
	protected DomainEvent(DateTimeOffset creationDateUtc) : this(creationDateUtc, Guid.NewGuid())
	{
	}

	public DateTimeOffset CreationDateUtc { get; } = creationDateUtc;
	public Guid CorrelationId { get; } = correlationId;
}