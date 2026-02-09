using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using WebSocketDemo.Client;
using WebSocketDemo.Client.Services;
using WebSocketDemo.Shared.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// 注册 HttpClient
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 注册 WebSocket 服务（单例模式，因为需要保持连接状态）
builder.Services.AddSingleton<IWebSocketService, WebSocketService>();

await builder.Build().RunAsync();
