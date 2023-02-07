// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddSignalR(options => options.EnableDetailedErrors = true)
    .AddMessagePackProtocol();
builder.Services
    .Configure<TwilioSettings>(
    builder.Configuration.GetSection(nameof(TwilioSettings)));
builder.Services.AddSingleton<TwilioService>();
builder.Services.AddRazorPages();
builder.Services
    .AddResponseCompression(opts =>
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
    app.UseHsts();
}

app.MapTwilioApi();

app.UseHttpsRedirection();
app.UseBlazorFrameworkFiles();
app.UseStaticFiles(app.Environment.IsDevelopment()
    ? new()
    : new StaticFileOptions
    {
        HttpsCompression = HttpsCompressionMode.Compress,
        OnPrepareResponse = context =>
            context.Context.Response.Headers[HeaderNames.CacheControl] =
                $"public,max-age={86_400}"
    });
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapHub<NotificationHub>(HubEndpoints.NotificationHub);
app.MapFallbackToFile("index.html");

app.Run();
