using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IEvangelist.Retweet.Services
{
    public interface ITweetStatusCache<T>
    {
        ValueTask<T> InitializeCacheAsync();

        Task PersistCacheAsync(T text);

        T Set(string key, T value);

        T GetOrCreate(string key, Func<string, T> factory);

        T AddOrUpdate(string key, Func<string, T, T> updateFactory);

        ICollection<string> AllKeys { get; }
    }
}
