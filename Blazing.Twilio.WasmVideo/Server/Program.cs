using Blazing.Twilio.WasmVideo.Server.Hubs;
using Blazing.Twilio.WasmVideo.Server.Options;
using Blazing.Twilio.WasmVideo.Server.Services;
using Blazing.Twilio.WasmVideo.Shared;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Net.Http.Headers;
using static System.Environment;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSignalR(options => options.EnableDetailedErrors = true)
        .AddMessagePackProtocol();
builder.Services.Configure<TwilioSettings>(settings =>
{
    settings.AccountSid = GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
    settings.ApiSecret = GetEnvironmentVariable("TWILIO_API_SECRET");
    settings.ApiKey = GetEnvironmentVariable("TWILIO_API_KEY");
});
builder.Services.AddSingleton<TwilioService>();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddResponseCompression(opts =>
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" }));

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles(new StaticFileOptions
{
    HttpsCompression = HttpsCompressionMode.Compress,
    OnPrepareResponse = context =>
        context.Context.Response.Headers[HeaderNames.CacheControl] =
            $"public,max-age={86_400}"
});
app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages();
    endpoints.MapControllers();
    endpoints.MapHub<NotificationHub>(HubEndpoints.NotificationHub);
    endpoints.MapFallbackToFile("index.html");
});

app.Run();
