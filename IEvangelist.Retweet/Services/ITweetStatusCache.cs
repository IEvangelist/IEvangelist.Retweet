using System;
using System.Collections.Generic;

namespace IEvangelist.Retweet.Services
{
    public interface ITweetStatusCache<T>
    {
        T Set(string key, T value);

        T GetOrCreate(string key, Func<string, T> factory);

        T AddOrUpdate(string key, Func<string, T, T> updateFactory);

        ICollection<string> AllKeys { get; }
    }
}
