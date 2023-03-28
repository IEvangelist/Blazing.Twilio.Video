// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

if (!OperatingSystem.IsBrowser())
{
    throw new PlatformNotSupportedException("""
        The Blazing Chat app only supports running in the browser!
        """);
}

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });
builder.Services.AddSingleton<AppState>();
builder.Services.AddLocalStorageServices();
builder.Services.AddMudServices();

await JSHost.ImportAsync(
    moduleName: nameof(SiteJavaScriptModule), moduleUrl: $"../js/blazing-video.js?{Guid.NewGuid()}");

var app = builder.Build();
await app.RunAsync();
