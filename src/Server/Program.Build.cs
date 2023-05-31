// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

public static partial class Program
{
    internal static WebApplication BuildApp(this WebApplicationBuilder builder)
    {
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

        return builder.Build();
    }
}
