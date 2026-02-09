using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebSocketDemo.Shared.Models;

namespace WebSocketDemo.Server.Services
{
    /// <summary>
    /// 聊天服务，用于管理 WebSocket 连接和消息广播
    /// </summary>
    public class ChatService : IDisposable
    {
        private readonly ILogger<ChatService> _logger;
        private readonly ConcurrentDictionary<string, (UserInfo User, WebSocket Socket)> _connections = new();
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// 当前在线用户数量
        /// </summary>
        public int OnlineUserCount => _connections.Count;

        /// <summary>
        /// 当前在线用户列表
        /// </summary>
        public IEnumerable<UserInfo> OnlineUsers => _connections.Values.Select(v => v.User);

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="logger">日志记录器</param>
        public ChatService(ILogger<ChatService> logger)
        {
            _logger = logger;
            _logger.LogInformation("ChatService 已初始化");
        }

        /// <summary>
        /// 添加新连接
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <param name="userName">用户名</param>
        /// <param name="webSocket">WebSocket 连接</param>
        /// <returns>用户信息</returns>
        public UserInfo AddConnection(string connectionId, string userName, WebSocket webSocket)
        {
            var user = new UserInfo(userName, connectionId);

            if (_connections.TryAdd(connectionId, (user, webSocket)))
            {
                _logger.LogInformation("用户 {UserName} (ID: {UserId}) 已连接。连接ID: {ConnectionId}",
                    user.UserName, user.UserId, connectionId);

                // 广播用户加入消息
                var joinMessage = ChatMessage.CreateUserJoinedMessage(userName);
                _ = BroadcastMessageAsync(joinMessage);

                // 更新在线用户列表给所有客户端
                _ = BroadcastOnlineUsersAsync();

                return user;
            }

            _logger.LogWarning("添加连接失败，连接ID {ConnectionId} 可能已存在", connectionId);
            return user;
        }

        /// <summary>
        /// 移除连接
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>移除的用户名，如果连接不存在则返回空</returns>
        public string? RemoveConnection(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var connectionInfo))
            {
                var userName = connectionInfo.User.UserName;

                _logger.LogInformation("用户 {UserName} (ID: {UserId}) 已断开连接。连接ID: {ConnectionId}",
                    userName, connectionInfo.User.UserId, connectionId);

                // 广播用户离开消息
                var leaveMessage = ChatMessage.CreateUserLeftMessage(userName);
                _ = BroadcastMessageAsync(leaveMessage);

                // 更新在线用户列表给所有客户端
                _ = BroadcastOnlineUsersAsync();

                return userName;
            }

            _logger.LogDebug("尝试移除不存在的连接ID: {ConnectionId}", connectionId);
            return null;
        }

        /// <summary>
        /// 获取用户连接
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>WebSocket连接，如果不存在则返回null</returns>
        public WebSocket? GetConnection(string connectionId)
        {
            if (_connections.TryGetValue(connectionId, out var connectionInfo))
            {
                return connectionInfo.Socket;
            }

            return null;
        }

        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>用户信息，如果不存在则返回null</returns>
        public UserInfo? GetUserInfo(string connectionId)
        {
            if (_connections.TryGetValue(connectionId, out var connectionInfo))
            {
                return connectionInfo.User;
            }

            return null;
        }

        /// <summary>
        /// 广播消息给所有连接的用户
        /// </summary>
        /// <param name="message">聊天消息</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task BroadcastMessageAsync(ChatMessage message)
        {
            _logger.LogDebug("广播消息: {MessageType} - {Content}", message.MessageType, message.Content);

            var messageJson = JsonSerializer.Serialize(message, _jsonOptions);
            var buffer = Encoding.UTF8.GetBytes(messageJson);

            var tasks = new List<Task>();

            foreach (var connection in _connections.Values.ToList())
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    tasks.Add(SendMessageAsync(connection.Socket, buffer));
                }
                else
                {
                    _logger.LogDebug("跳过关闭的连接: {ConnectionId}", connection.User.ConnectionId);
                }
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "广播消息时发生错误");
            }
        }

        /// <summary>
        /// 发送消息给特定用户
        /// </summary>
        /// <param name="connectionId">目标连接ID</param>
        /// <param name="message">聊天消息</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task SendMessageToUserAsync(string connectionId, ChatMessage message)
        {
            if (!_connections.TryGetValue(connectionId, out var connectionInfo))
            {
                _logger.LogWarning("尝试发送消息给不存在的连接ID: {ConnectionId}", connectionId);
                return;
            }

            if (connectionInfo.Socket.State != WebSocketState.Open)
            {
                _logger.LogWarning("尝试发送消息给关闭的连接: {ConnectionId}", connectionId);
                return;
            }

            _logger.LogDebug("发送消息给用户 {UserName}: {MessageType} - {Content}",
                connectionInfo.User.UserName, message.MessageType, message.Content);

            var messageJson = JsonSerializer.Serialize(message, _jsonOptions);
            var buffer = Encoding.UTF8.GetBytes(messageJson);

            await SendMessageAsync(connectionInfo.Socket, buffer);
        }

        /// <summary>
        /// 发送系统消息给所有用户
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task SendSystemMessageAsync(string content)
        {
            var systemMessage = ChatMessage.CreateSystemMessage(content);
            await BroadcastMessageAsync(systemMessage);
        }

        /// <summary>
        /// 广播在线用户列表给所有客户端
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        private async Task BroadcastOnlineUsersAsync()
        {
            try
            {
                var onlineUsers = OnlineUsers.ToList();
                var usersMessage = new
                {
                    Type = "OnlineUsersUpdate",
                    Timestamp = DateTime.UtcNow,
                    Users = onlineUsers
                };

                var messageJson = JsonSerializer.Serialize(usersMessage, _jsonOptions);
                var buffer = Encoding.UTF8.GetBytes(messageJson);

                var tasks = new List<Task>();

                foreach (var connection in _connections.Values.ToList())
                {
                    if (connection.Socket.State == WebSocketState.Open)
                    {
                        tasks.Add(SendMessageAsync(connection.Socket, buffer));
                    }
                }

                await Task.WhenAll(tasks);

                _logger.LogDebug("已广播在线用户列表，当前在线用户: {Count}", onlineUsers.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "广播在线用户列表时发生错误");
            }
        }

        /// <summary>
        /// 发送消息到 WebSocket
        /// </summary>
        /// <param name="webSocket">WebSocket连接</param>
        /// <param name="buffer">消息缓冲区</param>
        /// <returns>表示异步操作的任务</returns>
        private async Task SendMessageAsync(WebSocket webSocket, byte[] buffer)
        {
            try
            {
                await webSocket.SendAsync(
                    new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            catch (WebSocketException ex)
            {
                _logger.LogWarning(ex, "发送消息到 WebSocket 时发生错误");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "发送消息时发生未知错误");
            }
        }

        /// <summary>
        /// 清理所有连接
        /// </summary>
        public async Task CleanupAllConnectionsAsync()
        {
            _logger.LogInformation("正在清理所有连接，共 {Count} 个连接", _connections.Count);

            var tasks = new List<Task>();

            foreach (var connection in _connections.Values.ToList())
            {
                if (connection.Socket.State == WebSocketState.Open)
                {
                    tasks.Add(connection.Socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "服务关闭",
                        CancellationToken.None));
                }
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "清理连接时发生错误");
            }

            _connections.Clear();
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("ChatService 正在释放资源");
            _ = CleanupAllConnectionsAsync();
            GC.SuppressFinalize(this);
        }
    }
}
