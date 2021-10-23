namespace Spork;

public interface INotifyDisposed
{
    public bool IsDisposed { get; }
    event EventHandler Disposed;
}

public class DisposableSet : IDisposable, INotifyDisposed
{
    private readonly LinkedList<IDisposable> _disposables = new();
    private readonly object _lock = new();
    
    public T Add<T>(T value) where T : IDisposable
    {
        lock (_lock)
        {
            if (IsDisposed) throw new Exception("Trying to add to a disposed set");
            _disposables.AddFirst(value);
            if (value is INotifyDisposed {IsDisposed: false} notifiable)
            {
                notifiable.Disposed += NotifiableDisposed;
            }
        }

        return value;
    }

    private void NotifiableDisposed(object? sender, EventArgs eventArgs)
    {
        lock (_lock)
        {
            if (sender is IDisposable disposable)
            {
                _disposables.Remove(disposable);
                if (disposable is INotifyDisposed notifiable)
                {
                    notifiable.Disposed -= NotifiableDisposed;
                }
            }
        }
    }

    private void ReleaseUnmanagedResources()
    {
        lock (_lock)
        {
            IsDisposed = true;
            
            while (_disposables.Any())
            {
                var disposable = _disposables.First();
                if (disposable is INotifyDisposed notifiable)
                {
                    notifiable.Disposed -= NotifiableDisposed;
                }
                disposable.Dispose();
                _disposables.RemoveFirst();
            }
        }

        Disposed?.Invoke(this, EventArgs.Empty);
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

    public bool IsDisposed { get; private set; }
    public event EventHandler? Disposed;
}