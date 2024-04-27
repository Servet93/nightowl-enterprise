using System.Collections.Concurrent;

namespace NightOwlEnterprise.Api;

public class LockManager
{
    private readonly ConcurrentDictionary<string, object> _locks = new ConcurrentDictionary<string, object>();

    public bool AcquireLock(string key, int milliseconds)
    {
        // Key'e özgü bir kilit oluştur
        var lockObject = _locks.GetOrAdd(key, _ => new object());
        
        // Belirtilen süre boyunca kilit alınmaya çalış
        if (Monitor.TryEnter(lockObject, milliseconds))
        {
            Console.WriteLine($"Lock acquired for key: {key}");
            return true;
        }
        else
        {
            Console.WriteLine($"Failed to acquire lock for key: {key}");
            return false;
        }
    }

    public void ReleaseLock(string key)
    {
        // Key'e özgü kilit varsa serbest bırak
        if (_locks.TryGetValue(key, out var lockObject))
        {
            Monitor.Exit(lockObject);
            Console.WriteLine($"Lock released for key: {key}");
        }
    }
}