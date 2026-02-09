using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using WebSocketDemo.Server.Services;
using WebSocketDemo.Shared.Models;

namespace WebSocketDemo.Server.Middleware
{
    /// <summary>
    /// WebSocket 中间件，处理 WebSocket 连接和消息
    /// </summary>
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<WebSocketMiddleware> _logger;
        private readonly ChatService _chatService;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// WebSocket 端点路径
        /// </summary>
        public static readonly string WebSocketPath = "/ws";

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="next">请求委托</param>
        /// <param name="logger">日志记录器</param>
        /// <param name="chatService">聊天服务</param>
        public WebSocketMiddleware(RequestDelegate next, ILogger<WebSocketMiddleware> logger, ChatService chatService)
        {
            _next = next;
            _logger = logger;
            _chatService = chatService;
        }

        /// <summary>
        /// 中间件调用方法
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            // 检查是否为 WebSocket 请求
            if (!IsWebSocketRequest(context))
            {
                // 如果不是 WebSocket 请求，继续处理管道
                await _next(context);
                return;
            }

            _logger.LogInformation("接收到 WebSocket 连接请求: {Path}", context.Request.Path);

            try
            {
                // 接受 WebSocket 连接
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                // 生成连接 ID
                var connectionId = Guid.NewGuid().ToString();
                _logger.LogInformation("WebSocket 连接已建立，连接ID: {ConnectionId}", connectionId);

                // 从查询字符串获取用户名，默认为"匿名用户"
                var userName = context.Request.Query["username"].ToString();
                if (string.IsNullOrWhiteSpace(userName))
                {
                    userName = $"匿名用户_{connectionId[..8]}";
                }

                _logger.LogInformation("用户 {UserName} 正在连接...", userName);

                // 添加到连接管理
                var userInfo = _chatService.AddConnection(connectionId, userName, webSocket);

                // 发送欢迎消息给新用户
                var welcomeMessage = ChatMessage.CreateSystemMessage($"欢迎 {userName} 加入聊天室！");
                await _chatService.SendMessageToUserAsync(connectionId, welcomeMessage);

                // 处理 WebSocket 消息
                await HandleWebSocketConnection(webSocket, connectionId, userInfo);
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "WebSocket 处理过程中发生错误");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理 WebSocket 连接时发生未知错误");
            }
        }

        /// <summary>
        /// 检查是否为 WebSocket 请求
        /// </summary>
        /// <param name="context">HTTP 上下文</param>
        /// <returns>如果是 WebSocket 请求返回 true，否则返回 false</returns>
        private bool IsWebSocketRequest(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments(WebSocketPath) &&
                   context.WebSockets.IsWebSocketRequest;
        }

        /// <summary>
        /// 处理 WebSocket 连接
        /// </summary>
        /// <param name="webSocket">WebSocket 连接</param>
        /// <param name="connectionId">连接 ID</param>
        /// <param name="userInfo">用户信息</param>
        /// <returns>表示异步操作的任务</returns>
        private async Task HandleWebSocketConnection(WebSocket webSocket, string connectionId, UserInfo userInfo)
        {
            var buffer = new byte[1024 * 4]; // 4KB 缓冲区

            try
            {
                // 持续接收消息
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("接收到关闭消息，连接ID: {ConnectionId}", connectionId);
                        await HandleConnectionClose(webSocket, connectionId, "客户端请求关闭连接");
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        await ProcessMessage(buffer, result, connectionId, userInfo);
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        _logger.LogWarning("接收到二进制消息，当前不支持，连接ID: {ConnectionId}", connectionId);
                    }
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                _logger.LogInformation("WebSocket 连接异常关闭，连接ID: {ConnectionId}", connectionId);
                await HandleConnectionClose(webSocket, connectionId, "连接异常关闭");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理 WebSocket 消息时发生错误，连接ID: {ConnectionId}", connectionId);
                await HandleConnectionClose(webSocket, connectionId, "处理消息时发生错误");
            }
            finally
            {
                _logger.LogInformation("WebSocket 连接处理结束，连接ID: {ConnectionId}", connectionId);
                webSocket?.Dispose();
            }
        }

        /// <summary>
        /// 处理连接关闭
        /// </summary>
        /// <param name="webSocket">WebSocket 连接</param>
        /// <param name="connectionId">连接 ID</param>
        /// <param name="closeReason">关闭原因</param>
        /// <returns>表示异步操作的任务</returns>
        private async Task HandleConnectionClose(WebSocket webSocket, string connectionId, string closeReason)
        {
            try
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        closeReason,
                        CancellationToken.None);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "关闭 WebSocket 连接时发生错误，连接ID: {ConnectionId}", connectionId);
            }
            finally
            {
                // 从连接管理中移除
                _chatService.RemoveConnection(connectionId);
            }
        }

        /// <summary>
        /// 处理接收到的消息
        /// </summary>
        /// <param name="buffer">消息缓冲区</param>
        /// <param name="result">接收结果</param>
        /// <param name="connectionId">连接 ID</param>
        /// <param name="userInfo">用户信息</param>
        /// <returns>表示异步操作的任务</returns>
        private async Task ProcessMessage(byte[] buffer, WebSocketReceiveResult result, string connectionId, UserInfo userInfo)
        {
            try
            {
                // 获取消息内容
                var messageBytes = new byte[result.Count];
                Array.Copy(buffer, messageBytes, result.Count);
                var messageText = Encoding.UTF8.GetString(messageBytes);

                _logger.LogDebug("接收到消息，连接ID: {ConnectionId}, 消息: {Message}", connectionId, messageText);

                // 解析消息
                ChatMessage? receivedMessage;
                try
                {
                    receivedMessage = JsonSerializer.Deserialize<ChatMessage>(messageText, _jsonOptions);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "消息 JSON 解析失败，连接ID: {ConnectionId}", connectionId);

                    // 发送错误消息给客户端
                    var errorMessage = ChatMessage.CreateSystemMessage($"消息格式错误: {ex.Message}");
                    await _chatService.SendMessageToUserAsync(connectionId, errorMessage);
                    return;
                }

                if (receivedMessage == null)
                {
                    _logger.LogWarning("解析到的消息为空，连接ID: {ConnectionId}", connectionId);
                    return;
                }

                // 更新消息发送者信息
                receivedMessage.UserId = userInfo.UserId;
                receivedMessage.UserName = userInfo.UserName;
                receivedMessage.Timestamp = DateTime.UtcNow;

                // 记录消息
                _logger.LogInformation("用户 {UserName} 发送消息: {Content}", userInfo.UserName, receivedMessage.Content);

                // 广播消息给所有用户
                await _chatService.BroadcastMessageAsync(receivedMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理消息时发生错误，连接ID: {ConnectionId}", connectionId);

                // 发送错误消息给客户端
                var errorMessage = ChatMessage.CreateSystemMessage($"处理消息时发生错误: {ex.Message}");
                await _chatService.SendMessageToUserAsync(connectionId, errorMessage);
            }
        }
    }
}
