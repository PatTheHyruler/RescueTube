namespace RescueTube.Core.Utils;

public class ConcurrentHashSet<T> : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly HashSet<T> _hashSet = new();

    public bool Add(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Add(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }
    
    public void Clear()
    {
        _lock.EnterWriteLock();
        try
        {
            _hashSet.Clear();
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public bool Contains(T item)
    {
        _lock.EnterReadLock();
        try
        {
            return _hashSet.Contains(item);
        }
        finally
        {
            if (_lock.IsReadLockHeld) _lock.ExitReadLock();
        }
    }

    public bool Remove(T item)
    {
        _lock.EnterWriteLock();
        try
        {
            return _hashSet.Remove(item);
        }
        finally
        {
            if (_lock.IsWriteLockHeld) _lock.ExitWriteLock();
        }
    }

    public int Count
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _hashSet.Count;
            }
            finally
            {
                if (_lock.IsReadLockHeld) _lock.ExitReadLock();
            }
        }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}