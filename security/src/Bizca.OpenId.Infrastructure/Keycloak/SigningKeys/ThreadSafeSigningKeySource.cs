using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace Bizca.OpenId.Infrastructure.Keycloak.SigningKeys;

/// <summary>
/// Decorator that adds thread-safe synchronization to signing key access.
/// Single Responsibility: concurrency control only.
/// Uses a double-check locking pattern for optimal performance.
/// </summary>
internal sealed class ThreadSafeSigningKeySource(ISigningKeySource inner) : ISigningKeySource, IDisposable
{
	private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _disposed;

	~ThreadSafeSigningKeySource()
	{
		Dispose(disposing: false);
	}

	/// <summary>
    /// Gets signing keys in a thread-safe manner.
    /// </summary>
    public async Task<ICollection<SecurityKey>> GetSigningKeysAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _lock.WaitAsync(cancellationToken);

        try
        {
            return await inner.GetSigningKeysAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Refreshes the signing keys in a thread-safe manner.
    /// </summary>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        await _lock.WaitAsync(cancellationToken);
        try
        {
            await inner.RefreshAsync(cancellationToken);
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
		GC.SuppressFinalize(this);
    }

	private void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		if (disposing)
		{
			_lock.Dispose();
		}

		_disposed = true;
	}
}


