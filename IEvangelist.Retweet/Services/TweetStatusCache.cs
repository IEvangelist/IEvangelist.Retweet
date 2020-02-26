using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using IEvangelist.Retweet.Models;

namespace IEvangelist.Retweet.Services
{
    public class TweetStatusCache : ITweetStatusCache<TweetText>
    {
        readonly object _locker = new object();
        readonly ConcurrentDictionary<string, TweetText> _cache = new ConcurrentDictionary<string, TweetText>();

        public ICollection<string> AllKeys
        {
            get
            {
                lock (_locker)
                {
                    return _cache.Keys;
                }
            }
        }

        public TweetText GetOrCreate(string key, Func<string, TweetText> factory)
        {
            lock (_locker)
            {
                return _cache.GetOrAdd(key, factory);
            }
        }

        public TweetText AddOrUpdate(string key, Func<string, TweetText, TweetText> updateFactory)
        {
            lock (_locker)
            {
                return _cache.AddOrUpdate(key, key => updateFactory(key, null), updateFactory);
            }
        }

        public TweetText Set(string key, TweetText value)
        {
            lock (_locker) 
            { 
                return _cache[key] = value;
            }
        }
    }
}
