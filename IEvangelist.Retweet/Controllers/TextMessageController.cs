using IEvangelist.Retweet.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Twilio.AspNet.Core;
using Twilio.TwiML;

namespace IEvangelist.Retweet.Controllers
{
    [ApiController, Route("api/twitter")]
    public class TextMessageController : TwilioController
    {
        [HttpPost, Route("retweet")]
        public async Task<IActionResult> HandleTwilioWebhook(
            [FromServices] ITwitterClient twitterClient)
        {
            var requestBody = Request.Form["Body"];
            var response = new MessagingResponse();
            if (string.Equals("Yes", requestBody, StringComparison.OrdinalIgnoreCase))
            {
                await twitterClient.RetweetMostRecentMentionAsync();
                response.Message("Done!");
            }

            return TwiML(response);
        }
    }
}
