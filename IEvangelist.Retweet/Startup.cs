using System;
using IEvangelist.Retweet.Options;
using IEvangelist.Retweet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tweetinvi;

namespace IEvangelist.Retweet
{
    public class Startup
    {
        readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration) =>
            _configuration = configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<Settings>(settings =>
            {
                settings.TwilioAccountSid = _configuration["TWILIO_ACCOUNT_SID"];
                settings.TwilioAuthToken = _configuration["TWILIO_AUTH_TOKEN"];
                settings.TwilioFromPhoneNumber = _configuration["TWILIO_SMS_PHONE_NUMBER"];
                settings.ToPhoneNumber = _configuration["TO_PHONE_NUMBER"];
                settings.TwitterHandle = _configuration["TWITTER_HANDLE"];
            });

            Auth.SetUserCredentials(
                _configuration["Authentication:Twitter:ConsumerKey"],
                _configuration["Authentication:Twitter:ConsumerSecret"],
                _configuration["Authentication:Twitter:AccessToken"],
                _configuration["Authentication:Twitter:AccessTokenSecret"]);

            services.AddSingleton<ITwitterClient, TwitterClient>();
            services.AddHostedService<MentionsListener>();

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
