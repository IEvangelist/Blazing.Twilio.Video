// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(_ =>
    new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    });
builder.Services.AddLocalStorageServices();
builder.Services.AddSingleton<ISiteVideoJavaScriptModule, SiteVideoJavaScriptModule>();
builder.Services.AddMudServices();

await builder.Build().RunAsync();
