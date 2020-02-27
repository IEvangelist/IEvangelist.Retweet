using System;
using System.Threading.Tasks;
using IEvangelist.Retweet.Services;
using Microsoft.AspNetCore.Mvc;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace IEvangelist.Retweet.Controllers
{
    [ApiController, Route("api/twitter")]
    public class TextMessageController : TwilioController
    {
        readonly ITwitterClient _twitterClient;

        public TextMessageController(ITwitterClient twitterClient)
            => _twitterClient = twitterClient;

        [HttpPost, Route("retweet")]
        public async ValueTask<IActionResult> HandleTwilioWebhook()
        {
            var requestBody = Request.Form["Body"];

            var response = new MessagingResponse();
            if (string.Equals("Yes", requestBody, StringComparison.OrdinalIgnoreCase))
            {
                await _twitterClient.RetweetMostRecentMentionAsync();
                response.Message("Done!");
            }

            return TwiML(response);
        }
    }
}
