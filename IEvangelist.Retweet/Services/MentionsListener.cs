using IEvangelist.Retweet.Models;
using IEvangelist.Retweet.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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
            var lastMention = await _tweetStatusCache.InitializeCacheAsync();
            var lastId = lastMention?.TweetId ?? 0;

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

                            await _tweetStatusCache.PersistCacheAsync((mention.Id, message.Sid, mention.CreatedAt));
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
    }
}
