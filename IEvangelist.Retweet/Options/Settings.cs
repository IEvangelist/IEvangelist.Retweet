namespace IEvangelist.Retweet.Options
{
    public class Settings
    {
        /// <summary>
        /// The primary Twilio account SID, displayed prominently on your twilio.com/console dashboard.
        /// </summary>
        public string TwilioAccountSid { get; set; }
        
        /// <summary>
        /// The auth token for your primary Twilio account, hidden on your twilio.com/console dashboard.
        /// </summary>
        public string TwilioAuthToken { get; set; }

        /// <summary>
        /// The Twilio phone number to text from (in the following format: +12345678900).
        /// </summary>
        public string TwilioFromPhoneNumber { get; set; }

        /// <summary>
        /// The user's phone number to send texts to (in the following format: +12345678900).
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
