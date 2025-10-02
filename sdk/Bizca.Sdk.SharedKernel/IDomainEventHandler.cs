using System.Threading;
using System.Threading.Tasks;

namespace Bizca.Sdk.SharedKernel;

public interface IDomainEventHandler<in T> where T : DomainEvent
{
	Task Handle(T domainEvent, CancellationToken cancellationToken);
}
