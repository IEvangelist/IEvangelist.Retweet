namespace IEvangelist.Retweet.Models
{
    public class TwilioStatus
    {
        public string MessageStatus { get; set; }
        public string MessageSid { get; set; }
        public string MessagingServiceSid { get; set; }
        public string AccountSid { get; set; }
        public string From { get; set; }
        public string ApiVersion { get; set; }
        public string To { get; set; }
        public string SmsStatus { get; set; }
        public string SmsSid { get; set; }
    }
}
