using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using MudBlazor.Services;

using MissionControl;
using SOTM.MissionControl.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMudServices();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddScoped(typeof(DeckDataService));
builder.Services.AddScoped(typeof(DraftRandomizerService));
builder.Services.AddScoped(typeof(DraftSelectionsService));
builder.Services.AddScoped(typeof(GameService));
builder.Services.AddScoped(typeof(GameLogService));
builder.Services.AddScoped(typeof(SettingsService));

await builder.Build().RunAsync();
