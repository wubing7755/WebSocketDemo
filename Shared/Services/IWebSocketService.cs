using System;
using System.Threading.Tasks;
using WebSocketDemo.Shared.Models;

namespace WebSocketDemo.Shared.Services
{
    /// <summary>
    /// WebSocket 服务接口
    /// </summary>
    public interface IWebSocketService : IDisposable
    {
        /// <summary>
        /// 连接状态改变事件
        /// </summary>
        event Action<bool> ConnectionStateChanged;

        /// <summary>
        /// 接收到消息事件
        /// </summary>
        event Action<ChatMessage> MessageReceived;

        /// <summary>
        /// 接收到在线用户列表更新事件
        /// </summary>
        event Action<UserInfo[]> OnlineUsersUpdated;

        /// <summary>
        /// 接收到错误事件
        /// </summary>
        event Action<string> ErrorOccurred;

        /// <summary>
        /// 获取当前连接状态
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// 获取当前用户信息
        /// </summary>
        UserInfo CurrentUser { get; }

        /// <summary>
        /// 获取在线用户列表
        /// </summary>
        UserInfo[] OnlineUsers { get; }

        /// <summary>
        /// 连接到 WebSocket 服务器
        /// </summary>
        /// <param name="serverUrl">服务器URL</param>
        /// <param name="userName">用户名</param>
        /// <returns>连接任务</returns>
        Task ConnectAsync(string serverUrl, string userName);

        /// <summary>
        /// 发送文本消息
        /// </summary>
        /// <param name="content">消息内容</param>
        /// <returns>发送任务</returns>
        Task SendMessageAsync(string content);

        /// <summary>
        /// 发送聊天消息对象
        /// </summary>
        /// <param name="message">聊天消息</param>
        /// <returns>发送任务</returns>
        Task SendMessageAsync(ChatMessage message);

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns>断开连接任务</returns>
        Task DisconnectAsync();

        /// <summary>
        /// 尝试重新连接
        /// </summary>
        /// <returns>重连任务</returns>
        Task ReconnectAsync();
    }
}
