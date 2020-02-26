using IEvangelist.Retweet.Extensions;
using IEvangelist.Retweet.Models;
using IEvangelist.Retweet.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace IEvangelist.Retweet.Services
{
    public class MentionsListener : BackgroundService
    {
        readonly ILogger<MentionsListener> _logger;
        readonly TwilioSettings _twilioSettings;
        readonly ITwitterClient _twitterClient;
        readonly ITweetStatusCache<TweetText> _tweetStatusCache;

        public MentionsListener(
            ILogger<MentionsListener> logger,
            IOptions<TwilioSettings> options,
            ITwitterClient twitterClient,
            ITweetStatusCache<TweetText> tweetStatusCache)
        {
            _logger = logger;
            _twilioSettings = options.Value;
            _twitterClient = twitterClient;
            _tweetStatusCache = tweetStatusCache;

            TwilioClient.Init(
                _twilioSettings.AccountSid,
                _twilioSettings.AuthToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var lastId = await InitializeCacheAsync();
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Rate limited to 75 calls per 15 minutes, 5 calls per minute.
                    // This is one call every 12 seconds, but to be safe - 1 call / 15 seconds.
                    var mention = await _twitterClient.GetMostRecentMentionedTweetAsync();
                    if (mention != null && lastId != mention.Id)
                    {
                        if (mention.FullText.Contains(_twilioSettings.TwitterHandle, StringComparison.OrdinalIgnoreCase))
                        {
                            lastId = mention.Id;
                            var message = await MessageResource.CreateAsync(
                                body: $"{mention.Url}. Someone mentioned you on Twitter! Reply with 'Yes' to retweet this...",
                                from: new PhoneNumber(_twilioSettings.FromPhoneNumber),
                                to: new PhoneNumber(_twilioSettings.ToPhoneNumber),
                                statusCallback: new Uri("https://twilio-retweet.azurewebsites.net/api/twitter/status"));

                            await UpdateCacheAsync((mention.Id, message.Sid, mention.CreatedAt));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message, ex);
                }
                finally
                {
                    await Task.Delay(
                        _twilioSettings.DelayBetweenMentionCalls,
                        stoppingToken);
                }
            }
        }

        async ValueTask<long> InitializeCacheAsync()
        {
            var filePath = GetLastMentionFilePath();
            if (!File.Exists(filePath))
            {
                return 0;
            }

            var json = await File.ReadAllTextAsync(filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return 0;
            }

            try
            {
                var tweetTexts = json.FromJson<TweetText[]>();
                if (tweetTexts != null)
                {
                    for (var i = 0; i < tweetTexts.Length; ++i)
                    {
                        var text = tweetTexts[i];
                        _tweetStatusCache.Set(text.TwilioId, text);
                    }

                    return tweetTexts.Where(t => t.WasTexted).OrderByDescending(t => t.CreatedAt).First().TweetId;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return 0;
        }

        async Task UpdateCacheAsync(TweetText text)
        {
            try
            {
                _tweetStatusCache.Set(text.TwilioId, text);

                var keys = _tweetStatusCache.AllKeys;
                var list = new List<TweetText>();
                foreach (var key in keys)
                {
                    list.Add(_tweetStatusCache.GetOrCreate(key, _ => null));
                }

                var json = list.ToArray().ToJson();
                var filePath = GetLastMentionFilePath();
                await File.WriteAllTextAsync(filePath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        static string GetLastMentionFilePath() =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "recent-twitter-mentions.json");
    }
}
