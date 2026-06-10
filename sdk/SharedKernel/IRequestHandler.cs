using System.Threading;
using System.Threading.Tasks;

namespace Bizca.Sdk.SharedKernel;

public interface IRequestHandler<in TRequest, TResponse>
	where TRequest : IRequest
{
	Task<Result<TResponse>> HandleAsync(TRequest? request, CancellationToken cancellationToken);
}