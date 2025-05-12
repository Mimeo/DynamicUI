using BlazorPro.BlazorSize;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Mimeo.DynamicUI.Blazor.Services;
using Mimeo.DynamicUI.Data.OData;
using Mimeo.DynamicUI.Demo.Client.Services;
using Radzen;

namespace Mimeo.DynamicUI.Demo.Client
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // Dynamic UI services
            builder.Services.AddLocalization();
            builder.Services.AddSingleton<IStringLocalizer, StringLocalizer<Language>>();
            builder.Services.AddSingleton<IDateTimeConverter, DateTimeConverter>();
            builder.Services.AddSingleton<TaskRunningService>();
            builder.Services.AddSingleton<ODataExpressionGenerator>(); // Optional, but useful for OData queries

            // Radzen services (some require elements to be in the main layout)
            builder.Services.AddRadzenComponents(); // Requires <RadzenComponents/>, see Routes.razor

            // BlazorPro services (some require elements to be in the main layout)
            builder.Services.AddMediaQueryService(); // Requires <MediaQueryList>, see Routes.razor

            // Test project services
            builder.Services.AddHttpClient(Options.DefaultName, client =>
            {
                client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
            });
            builder.Services.AddTransient<TestDataService>();
            builder.Services.AddTransient<TestDataExportService>();

            await builder.Build().RunAsync();
        }
    }
}
