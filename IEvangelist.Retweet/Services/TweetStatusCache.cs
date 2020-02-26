using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IEvangelist.Retweet.Extensions;
using IEvangelist.Retweet.Models;
using Microsoft.Extensions.Logging;

namespace IEvangelist.Retweet.Services
{
    public class TweetStatusCache : ITweetStatusCache<TweetText>
    {
        static string LastMentionFilePath => "../recent-twitter-mentions.json";

        readonly object _locker = new object();
        readonly ConcurrentDictionary<string, TweetText> _cache = new ConcurrentDictionary<string, TweetText>();
        readonly ILogger<TweetStatusCache> _logger;

        public TweetStatusCache(ILogger<TweetStatusCache> logger) => _logger = logger;

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

        public async ValueTask<TweetText> InitializeCacheAsync()
        {
            var filePath = LastMentionFilePath;
            if (!File.Exists(filePath))
            {
                return null;
            }

            var json = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var tweetTexts = json.FromJson<TweetText[]>();
                if (tweetTexts != null)
                {
                    for (var i = 0; i < tweetTexts.Length; ++i)
                    {
                        var text = tweetTexts[i];
                        Set(text.TwilioId, text);
                    }

                    return tweetTexts.Where(t => t.WasTexted)
                                     .OrderByDescending(t => t.CreatedAt)
                                     .FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return null;
        }

        public async Task PersistCacheAsync(TweetText text)
        {
            try
            {
                Set(text.TwilioId, text);

                var keys = AllKeys;
                var list = new List<TweetText>();
                foreach (var key in keys)
                {
                    list.Add(GetOrCreate(key, _ => null));
                }

                var json = list.ToArray().ToJson();
                var filePath = LastMentionFilePath;
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }
    }
}
