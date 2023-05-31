// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

public static partial class Program
{
    internal static void ConfigureApp(this WebApplication app)
    {
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
                OnPrepareResponse = context =>
                {
                    context.Context.Response.Headers[HeaderNames.CacheControl] =
                        $"public,max-age={86_400}";
                }
            });
        app.UseRouting();
        app.MapRazorPages();
        app.MapControllers();
        app.MapHub<NotificationHub>(HubEndpoints.NotificationHub);
        app.MapFallbackToFile("index.html");
    }
}
