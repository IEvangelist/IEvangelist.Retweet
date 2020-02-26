using IEvangelist.Retweet.Models;
using IEvangelist.Retweet.Options;
using IEvangelist.Retweet.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
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
            services.Configure<TwilioSettings>(
                        _configuration.GetSection(nameof(TwilioSettings)));

            Auth.SetUserCredentials(
                _configuration["Authentication:Twitter:ConsumerKey"],
                _configuration["Authentication:Twitter:ConsumerSecret"],
                _configuration["Authentication:Twitter:AccessToken"],
                _configuration["Authentication:Twitter:AccessTokenSecret"]);

            services.AddApplicationInsightsTelemetry();
            services.AddSingleton<ITweetStatusCache<TweetText>, TweetStatusCache>();
            services.AddSingleton<ITwitterClient, TwitterClient>();
            services.AddHostedService<MentionsListener>();
            services.AddMemoryCache();
            services.AddControllers();
            services.AddSwaggerGen(
                config => config.SwaggerDoc(
                    "v1", 
                    new OpenApiInfo { Title = "Twitter Retweet API", Version = "v1" }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(
                config => config.SwaggerEndpoint("/swagger/v1/swagger.json", "Twitter Retweet API"));

            app.UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints => endpoints.MapControllers());
        }
    }
}
