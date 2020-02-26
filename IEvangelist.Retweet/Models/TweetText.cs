using System;
using System.Text.Json.Serialization;

namespace IEvangelist.Retweet.Models
{
    public class TweetText
    {
        public long TweetId { get; set; }

        public string TwilioId { get; set; }

        public string TextStatus { get; set; }

        public DateTime CreatedAt { get; set; }

        [JsonIgnore] internal bool WasTexted
            => string.Equals(TextStatus, "sent", StringComparison.OrdinalIgnoreCase)
            || string.Equals(TextStatus, "delivered", StringComparison.OrdinalIgnoreCase);

        public static implicit operator TweetText((long tweetId, string twilioId, DateTime createdAt) tuple) =>
            new TweetText
            {
                TweetId = tuple.tweetId,
                TwilioId = tuple.twilioId,
                CreatedAt = tuple.createdAt
            };

        public void Deconstruct(out long tweetId, out string twilioId)
            => (tweetId, twilioId) = (TweetId, TwilioId);

        public void Deconstruct(out long tweetId, out string twilioId, out string textStatus)
            => (tweetId, twilioId, textStatus) = (TweetId, TwilioId, TextStatus);

        public void Deconstruct(out long tweetId, out string twilioId, out DateTime createdAt)
            => (tweetId, twilioId, createdAt) = (TweetId, TwilioId, CreatedAt);

        public void Deconstruct(out long tweetId, out string twilioId, out string textStatus, out DateTime createdAt)
            => (tweetId, twilioId, textStatus, createdAt) = (TweetId, TwilioId, TextStatus, CreatedAt);
    }
}
