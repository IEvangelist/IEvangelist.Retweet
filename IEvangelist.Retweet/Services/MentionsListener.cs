using System;
using System.Threading;
using System.Threading.Tasks;
using IEvangelist.Retweet.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace IEvangelist.Retweet.Services
{
    public class MentionsListener : BackgroundService
    {
        readonly ILogger<MentionsListener> _logger;
        readonly Settings _settings;
        readonly ITwitterClient _twitterClient;

        public MentionsListener(
            ILogger<MentionsListener> logger,
            IOptions<Settings> options,
            ITwitterClient twitterClient)
        {
            _logger = logger;
            _settings = options.Value;
            _twitterClient = twitterClient;

            TwilioClient.Init(
                _settings.TwilioAccountSid,
                _settings.TwilioAuthToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            long lastId = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Rate limited to 75 calls per 15 minutes, 5 calls per minute.
                    // We rely on the configured delay: DelayBetweenMentionCalls...
                    var mention = await _twitterClient.GetMostRecentMentionedTweetAsync();
                    if (mention != null && lastId != mention.Id)
                    {
                        if (mention.FullText
                                   .Contains(_settings.TwitterHandle, StringComparison.OrdinalIgnoreCase))
                        {
                            _ = await MessageResource.CreateAsync(
                                body: $"{mention.Url}. Someone mentioned you on Twitter! Reply with 'Yes' to retweet this...",
                                from: new PhoneNumber(_settings.TwilioFromPhoneNumber),
                                to: new PhoneNumber(_settings.ToPhoneNumber));

                            lastId = mention.Id;
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
                        _settings.DelayBetweenMentionCalls,
                        stoppingToken);
                }
            }
        }
    }
}
