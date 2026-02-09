using WebSocketDemo.Server.Middleware;
using WebSocketDemo.Server.Services;
using WebSocketDemo.Shared.Services;

namespace WebSocketDemo.Server;

public class Program
{
    public static async Task Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddRazorPages();

        // 添加 CORS 策略以支持 WebSocket 连接
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("WebSocketPolicy", policy =>
            {
                policy.WithOrigins(
                    "https://localhost:7235",
                    "http://localhost:5015"
                 )
                .WithMethods("GET", "POST", "PUT", "PATCH", "DELETE", "OPTIONS")
                .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "x-custom-header")
                .AllowCredentials();
            });
        });

        // 添加 ChatService 作为单例服务
        builder.Services.AddSingleton<ChatService>();

        // 添加日志服务
        builder.Services.AddSingleton<IConsoleLogger, ConsoleLogger>();

        var app = builder.Build();

        // 配置 HTTP 请求管道
        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
        }
        else
        {
            app.UseExceptionHandler("/Error");
            // 生产环境默认 HSTS 值为 30 天
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseBlazorFrameworkFiles();
        app.UseStaticFiles();

        // 使用 CORS 策略
        app.UseCors("WebSocketPolicy");

        app.UseRouting();

        // 启用 WebSocket 支持
        app.UseWebSockets();

        // 添加自定义 WebSocket 中间件
        app.UseMiddleware<WebSocketMiddleware>();

        app.MapRazorPages();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        app.Run();
    }
}