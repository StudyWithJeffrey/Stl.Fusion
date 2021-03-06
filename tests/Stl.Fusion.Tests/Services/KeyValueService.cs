using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Stl.Fusion.Tests.Services
{
    public interface IKeyValueService<TValue>
    {
        Task SetAsync(string key, TValue value, CancellationToken cancellationToken = default);
        Task RemoveAsync(string key, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<Option<TValue>> TryGetAsync(string key, CancellationToken cancellationToken = default);
        [ComputeMethod]
        Task<TValue> GetAsync(string key, CancellationToken cancellationToken = default);
    }

    public class KeyValueService<TValue> : IKeyValueService<TValue>
    {
        private readonly ConcurrentDictionary<string, TValue> _values =
            new ConcurrentDictionary<string, TValue>();

        public virtual Task<Option<TValue>> TryGetAsync(string key, CancellationToken cancellationToken = default)
            => Task.FromResult(_values.TryGetValue(key, out var v) ? Option.Some(v) : default);

#pragma warning disable 1998
        public virtual async Task<TValue> GetAsync(string key, CancellationToken cancellationToken = default)
#pragma warning restore 1998
        {
            if (key.EndsWith("error"))
                throw new ApplicationException("Error!");
            return _values.GetValueOrDefault(key)!;
        }

        public virtual Task SetAsync(string key, TValue value, CancellationToken cancellationToken = default)
        {
            _values[key] = value;
            Computed.Invalidate(() => TryGetAsync(key, default));
            Computed.Invalidate(() => GetAsync(key, default));
            return Task.CompletedTask;
        }

        public virtual Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _values.TryRemove(key, out _);
            Computed.Invalidate(() => TryGetAsync(key, default));
            Computed.Invalidate(() => GetAsync(key, default));
            return Task.CompletedTask;
        }
    }

    [ComputeService(typeof(IKeyValueService<string>))]
    public class StringKeyValueService : KeyValueService<string> { }
}
