
using System.Collections.Concurrent;

namespace Central_Hub.Services.Security
{
    /// <summary>
    /// Tracks used nonces to prevent replay attacks.
    /// TTL = 10 min (2× the timestamp tolerance window).
    /// Cleanup runs every 5 minutes via IHostedService.
    /// </summary>
    public interface INonceCache
    {
        /// <summary>Returns true and records the nonce if unseen. Returns false if replayed.</summary>
        bool TryConsume(string nonce);
    }
 
    public class InMemoryNonceCache : INonceCache, IHostedService, IDisposable
    {
        private readonly ConcurrentDictionary<string, DateTime> _seen = new();
        private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(10);
        private Timer? _timer;
 
        public bool TryConsume(string nonce)
        {
            if (string.IsNullOrWhiteSpace(nonce)) return false;
            return _seen.TryAdd(nonce, DateTime.UtcNow);
        }
 
        public Task StartAsync(CancellationToken ct)
        {
            _timer = new Timer(Sweep, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            return Task.CompletedTask;
        }
 
        public Task StopAsync(CancellationToken ct)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
 
        private void Sweep(object? _)
        {
            var cutoff = DateTime.UtcNow - Ttl;
            foreach (var kv in _seen.Where(kv => kv.Value < cutoff))
                _seen.TryRemove(kv.Key, out DateTime _);
        }
 
        public void Dispose() => _timer?.Dispose();
    }
}
