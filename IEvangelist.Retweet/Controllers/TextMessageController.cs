using IEvangelist.Retweet.Models;
using IEvangelist.Retweet.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace IEvangelist.Retweet.Controllers
{
    [ApiController, Route("api/twitter")]
    public class TextMessageController : TwilioController
    {
        readonly ITwitterClient _twitterClient;
        readonly ILogger<TextMessageController> _logger;
        readonly ITweetStatusCache<TweetText> _tweetStatusCache;

        public TextMessageController(
            ITwitterClient twitterClient,
            ILogger<TextMessageController> logger,
            ITweetStatusCache<TweetText> tweetStatusCache) =>
            (_twitterClient, _logger, _tweetStatusCache) = (twitterClient, logger, tweetStatusCache);

        [HttpPost, Route("retweet")]
        public async Task<IActionResult> HandleTwilioWebhook()
        {
            var requestBody = Request.Form["Body"];

            _logger.LogInformation(JsonConvert.SerializeObject(Request.Form));

            var response = new MessagingResponse();
            if (string.Equals("Yes", requestBody, StringComparison.OrdinalIgnoreCase))
            {
                await _twitterClient.RetweetMostRecentMentionAsync();
                response.Message("Done!");
            }

            return TwiML(response);
        }

        [HttpPost, Route("status")]
        public async Task<IActionResult> HandleSmsStatus([FromBody] TwilioStatus status)
        {
            var tweetTwext = _tweetStatusCache.AddOrUpdate(status.MessageSid, (key, tweetText) =>
            {
                tweetText.TextStatus = status.SmsStatus;
                return tweetText;
            });

            await _tweetStatusCache.PersistCacheAsync(tweetTwext);

            return Ok();
        }
    }
}
