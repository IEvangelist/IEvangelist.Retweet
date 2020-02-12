namespace IEvangelist.Retweet.Options
{
    public class TwilioSettings
    {
        /// <summary>
        /// The primary Twilio account SID, displayed prominently on your twilio.com/console dashboard.
        /// </summary>
        public string AccountSid { get; set; }
        
        /// <summary>
        /// The auth token for your primary Twilio account, hidden on your twilio.com/console dashboard.
        /// </summary>
        public string AuthToken { get; set; }

        /// <summary>
        /// The Twilio phone number that to text from.
        /// </summary>
        public string FromPhoneNumber { get; set; }

        /// <summary>
        /// The user's phone number to send texts to.
        /// </summary>
        public string ToPhoneNumber { get; set; }

        /// <summary>
        /// The amout of time to wait between Twitter API mention timeline calls.
        /// </summary>
        public int DelayBetweenMentionCalls { get; set; } = 60_000;

        /// <summary>
        /// The user's twitter handle to monitor for mentions (exclude the @ prefix).
        /// </summary>
        public string TwitterHandle { get; set; } = "davidpine7";
    }
}