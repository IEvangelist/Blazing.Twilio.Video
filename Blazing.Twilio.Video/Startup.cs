using Blazing.Twilio.Video.Hubs;
using Blazing.Twilio.Video.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using static System.Environment;

namespace Blazing.Twilio.Video
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddSignalR(options => options.EnableDetailedErrors = true)
                    .AddMessagePackProtocol();
            services.AddServerSideBlazor();
            services.AddProtectedBrowserStorage();
            services.AddResponseCompression(opts =>
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" }));
            services.Configure<TwilioSettings>(settings =>
            {
                settings.AccountSid = GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
                settings.ApiSecret = GetEnvironmentVariable("TWILIO_API_SECRET");
                settings.ApiKey = GetEnvironmentVariable("TWILIO_API_KEY");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>(HubEndpoints.Notifications);
                endpoints.MapFallbackToPage("/_Host");
            });
        }
    }
}
