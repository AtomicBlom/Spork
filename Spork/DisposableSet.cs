namespace Spork;

public class DisposableSet : IDisposable
{
    private Stack<IDisposable> _disposables = new();
    public object _lock = new();
    private bool _isDisposed;

    public T Add<T>(T value) where T : IDisposable
    {
        lock (_lock)
        {
            if (_isDisposed) throw new Exception("Trying to add to a disposed set");
            _disposables.Push(value);
        }

        return value;
    }

    private void ReleaseUnmanagedResources()
    {
        lock (_lock)
        {
            _isDisposed = true;
            while (_disposables.TryPop(out var disposable))
            {
                disposable.Dispose();
            }
        }
    }

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    ~DisposableSet()
    {
        ReleaseUnmanagedResources();
    }
}