using LuxfordPTAWeb.Client.Code;
using LuxfordPTAWeb.Client.Services;
using LuxfordPTAWeb.Shared.Configuration;
using LuxfordPTAWeb.Shared.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace LuxfordPTAWeb.Client
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);

			builder.Services.AddAuthorizationCore();
			builder.Services.AddCascadingAuthenticationState();
			builder.Services.AddAuthenticationStateDeserialization();

			// Register HttpClient with the correct base address for WASM
			builder.Services.AddScoped(sp => new HttpClient
			{
				BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
			});
			
			builder.Services.AddScoped<SchoolYearSupport>();

			// Configure Google Analytics options
			var googleAnalyticsOptions = builder.Configuration.GetSection("GoogleAnalytics").Get<GoogleAnalyticsOptions>() ?? new GoogleAnalyticsOptions();
			builder.Services.AddSingleton(googleAnalyticsOptions);

			// Register Cookie Consent and Google Analytics services
			builder.Services.AddScoped<ICookieConsentService, CookieConsentService>();
			builder.Services.AddScoped<GoogleAnalyticsService>();

			await builder.Build().RunAsync();
		}
	}
}