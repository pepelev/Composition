#if DEBUG

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Composition
{
    internal sealed class ConcurrentSingletonBag
    {
        private readonly ConcurrentDictionary<Key, Lazy<object>?> content = new();

        public T Get<T>(Func<T> factory, [CallerMemberName] string id = "") where T : notnull
        {
            var key = new Key(id, typeof(T));
            if (content.TryGetValue(key, out var lazy) && lazy != null)
            {
                var singleton = Unwrap(key, lazy);
                return (T)singleton;
            }

            var newLazy = new Lazy<object>(() => factory());
            content[key] = newLazy;
            var newSingleton = Unwrap(key, newLazy);
            return (T)newSingleton;
        }

        public async Task<T> GetAsync<T>(Func<Task<T>> factory, [CallerMemberName] string id = "") where T : notnull
        {
            var key = new Key(id, typeof(T));
            if (content.TryGetValue(key, out var lazy) && lazy != null)
            {
                return await WaitAsync<T>(key, lazy).ConfigureAwait(false);
            }

            var newLazy = new Lazy<object>(factory);
            var storedLazy = content.AddOrUpdate(
                key,
                newLazy,
                (_, existingValue) => existingValue ?? newLazy
            );
            return await WaitAsync<T>(key, storedLazy).ConfigureAwait(false);
        }

        private object Unwrap(Key key, Lazy<object> lazy)
        {
            try
            {
                return lazy.Value;
            }
            catch
            {
                content.TryUpdate(key, null, lazy);
                throw;
            }
        }

        private async Task<T> WaitAsync<T>(Key key, Lazy<object> lazy)
        {
            var task = (Task<T>)Unwrap(key, lazy);
            try
            {
                return await task.ConfigureAwait(false);
            }
            catch
            {
                content.TryUpdate(key, null, lazy);
                throw;
            }
        }
    }
}

#endif