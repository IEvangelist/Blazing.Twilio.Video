using Blazing.Twilio.WasmVideo.Server.Hubs;
using Blazing.Twilio.WasmVideo.Server.Options;
using Blazing.Twilio.WasmVideo.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using static System.Environment;

namespace Blazing.Twilio.WasmVideo.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration) => Configuration = configuration;

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR(options => options.EnableDetailedErrors = true)
                    .AddMessagePackProtocol();
            services.Configure<TwilioSettings>(settings =>
            {
                settings.AccountSid = GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
                settings.ApiSecret = GetEnvironmentVariable("TWILIO_API_SECRET");
                settings.ApiKey = GetEnvironmentVariable("TWILIO_API_KEY");
            });
            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddResponseCompression(opts =>
                opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                    new[] { "application/octet-stream" }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseResponseCompression();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>(HubEndpoints.NotificationHub);
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
