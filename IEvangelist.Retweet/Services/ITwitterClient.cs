using System.Threading.Tasks;
using Tweetinvi.Models;

namespace IEvangelist.Retweet.Services
{
    public interface ITwitterClient
    {
        Task<IMention> GetMostRecentMentionedTweetAsync();

        Task<ITweet> RetweetMostRecentMentionAsync();
    }
}
