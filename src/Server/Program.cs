// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.StaticFiles;

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
        {
            if (context.File.Exists is false)
            {
                var file = context.File;
                if (file.PhysicalPath is not null)
                {

                }
            }

            context.Context.Response.Headers[HeaderNames.CacheControl] =
                $"public,max-age={86_400}";
        },
        ContentTypeProvider = new FileExtensionContentTypeProvider
        {
            Mappings =
            {
                [".gz"] = "application/gzip",
                [".css"] = "text/css",
                [".html"] = "text/html",
                [".js"] = "text/javascript"
            }
        }
    });
app.UseRouting();
app.MapRazorPages();
app.MapControllers();
app.MapHub<NotificationHub>(HubEndpoints.NotificationHub);
app.MapFallbackToFile("index.html");

app.Run();
