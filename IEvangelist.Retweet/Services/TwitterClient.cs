using System;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace IEvangelist.Retweet.Services
{
    public class TwitterClient : ITwitterClient
    {
        long? _lastMentionId = null;

        async Task<IMention> ITwitterClient.GetMostRecentMentionedTweetAsync()
        {
            var mentions = await Task.Run(() =>
            {
                var parameters = new MentionsTimelineParameters
                {
                    MaximumNumberOfTweetsToRetrieve = 1,
                    TrimUser = false
                };

                if (_lastMentionId.HasValue)
                {
                    parameters.SinceId = _lastMentionId.Value;
                }

                return Timeline.GetMentionsTimeline(parameters);
            });

            var mostRecentMention =
                mentions?.OrderByDescending(mention => mention.CreatedAt)
                        .FirstOrDefault();

            
            if (mostRecentMention != null)
            {
                _lastMentionId = mostRecentMention.Id;
            }

            return mostRecentMention;
        }

        Task<ITweet> ITwitterClient.RetweetMostRecentMentionAsync() =>
            Task.Run(() => Tweet.PublishRetweet(_lastMentionId.Value));
    }
}
