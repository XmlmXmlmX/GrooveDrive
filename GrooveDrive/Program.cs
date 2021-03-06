using GrooveDrive;
using GrooveDrive.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<OneDriveMusicCrawlerService>();
builder.Services.AddScoped<PlayerState>();

builder.Services.AddMsalAuthentication(options =>
{
    options.ProviderOptions.DefaultAccessTokenScopes.Add("openid");
    //options.ProviderOptions.DefaultAccessTokenScopes.Add("offline_access");
    //options.ProviderOptions.AdditionalScopesToConsent.Add("User.Read");
    //options.ProviderOptions.AdditionalScopesToConsent.Add("Files.ReadWrite.All");

    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
});

foreach (string scope in Constants.GRAPH_SCOPES)
{
    builder.Services.AddGraphClient(scope);
}

await builder.Build().RunAsync();
