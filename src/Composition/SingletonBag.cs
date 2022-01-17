using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Composition
{
    /// <threadsafety instance="false" />
    public sealed class SingletonBag
    {
        private readonly Dictionary<Key, object> content = new();

        public T Get<T>(Func<T> factory, [CallerMemberName] string id = "") where T : notnull
        {
            var key = new Key(id, typeof(T));
            if (content.TryGetValue(key, out var singleton))
            {
                return (T)singleton;
            }

            var newSingleton = factory();
            content[key] = newSingleton;
            return newSingleton;
        }

        public async Task<T> GetAsync<T>(Func<Task<T>> factory, [CallerMemberName] string id = "") where T : notnull
        {
            var key = new Key(id, typeof(T));
            if (content.TryGetValue(key, out var singleton))
            {
                return (T)singleton;
            }

            var newSingleton = await factory().ConfigureAwait(false);
            content[key] = newSingleton;
            return newSingleton;
        }
    }
}