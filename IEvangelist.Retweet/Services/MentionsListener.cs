using IEvangelist.Retweet.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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

        long _lastMentionId = 0;

        public MentionsListener(
            ILogger<MentionsListener> logger,
            IOptions<TwilioSettings> options,
            ITwitterClient twitterClient)
        {
            _logger = logger;
            _twilioSettings = options.Value;
            _twitterClient = twitterClient;

            TwilioClient.Init(
                _twilioSettings.AccountSid,
                _twilioSettings.AuthToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _lastMentionId = await GetLastMentionedIdAsync();

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Rate limited to 75 calls per 15 minutes, 5 calls per minute.
                    // This is one call every 12 seconds, but to be safe - 1 call / 15 seconds.
                    var mention = await _twitterClient.GetMostRecentMentionedTweetAsync();
                    if (mention != null && _lastMentionId != mention.Id)
                    {
                        if (mention.UserMentions.Any(m => m.ScreenName == _twilioSettings.TwitterHandle))
                        {
                            await UpdateLastMentionedIdAsync(_lastMentionId = mention.Id);
                            _ = await MessageResource.CreateAsync(
                                body: $"{mention.Url}. Someone mentioned you on Twitter! Reply with 'Yes' to retweet this...",
                                from: new PhoneNumber(_twilioSettings.FromPhoneNumber),
                                to: new PhoneNumber(_twilioSettings.ToPhoneNumber));
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

        async Task<long> GetLastMentionedIdAsync()
        {
            var filePath = GetLastMentionFilePath();
            return File.Exists(filePath) 
                && long.TryParse(await File.ReadAllTextAsync(filePath), out var id) ? id : default;
        }

        Task UpdateLastMentionedIdAsync(long id) =>
            File.WriteAllTextAsync(GetLastMentionFilePath(), id.ToString());

        static string GetLastMentionFilePath() =>
            Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "last-mention.txt");
    }
}
