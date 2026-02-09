using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using WebSocketDemo.Shared.Models;
using WebSocketDemo.Shared.Services;

namespace WebSocketDemo.Client.Services;

/// <summary>
/// WebSocket 服务实现
/// </summary>
public class WebSocketService : IWebSocketService
{
    private const int ReceiveBufferSize = 4096;
    private const int MaxReconnectAttempts = 5;
    private const int ReconnectDelayMs = 3000;

    private readonly IJSRuntime _jsRuntime;
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cts;
    private bool _isDisposed;
    private int _reconnectAttempts;
    private string? _serverUrl;
    private string? _userName;
    private List<UserInfo> _onlineUsers = new();

    /// <inheritdoc/>
    public event Action<bool>? ConnectionStateChanged;

    /// <inheritdoc/>
    public event Action<ChatMessage>? MessageReceived;

    /// <inheritdoc/>
    public event Action<UserInfo[]>? OnlineUsersUpdated;

    /// <inheritdoc/>
    public event Action<string>? ErrorOccurred;

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public UserInfo CurrentUser { get; private set; } = new();

    /// <inheritdoc/>
    public UserInfo[] OnlineUsers => _onlineUsers.ToArray();

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="jsRuntime">JavaScript运行时</param>
    public WebSocketService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <inheritdoc/>
    public async Task ConnectAsync(string serverUrl, string userName)
    {
        if (IsConnected)
        {
            throw new InvalidOperationException("已连接到服务器，请先断开连接");
        }

        _serverUrl = serverUrl;
        _userName = userName;

        try
        {
            // 创建新的WebSocket实例
            _webSocket = new ClientWebSocket();
            _cts = new CancellationTokenSource();

            // 构建WebSocket URL（添加用户名查询参数）
            var webSocketUrl = $"{serverUrl}?username={Uri.EscapeDataString(userName)}";

            // 连接到服务器
            await _webSocket.ConnectAsync(new Uri(webSocketUrl), _cts.Token);

            // 更新连接状态
            IsConnected = true;
            CurrentUser = new UserInfo(userName, "temporary-connection-id");
            _reconnectAttempts = 0;

            // 触发连接状态变化事件
            OnConnectionStateChanged(true);

            // 启动消息接收循环
            _ = Task.Run(() => ReceiveMessagesAsync(_cts.Token), _cts.Token);
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"连接失败: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SendMessageAsync(string content)
    {
        if (!IsConnected || _webSocket == null)
        {
            throw new InvalidOperationException("未连接到服务器");
        }

        if (_webSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket连接未打开");
        }

        try
        {
            var message = new ChatMessage
            {
                UserId = CurrentUser.UserId,
                UserName = CurrentUser.UserName,
                Content = content,
                MessageType = MessageType.TextMessage
            };

            await SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"发送消息失败: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SendMessageAsync(ChatMessage message)
    {
        if (!IsConnected || _webSocket == null)
        {
            throw new InvalidOperationException("未连接到服务器");
        }

        if (_webSocket.State != WebSocketState.Open)
        {
            throw new InvalidOperationException("WebSocket连接未打开");
        }

        try
        {
            var messageJson = JsonSerializer.Serialize(message, _jsonOptions);
            var buffer = Encoding.UTF8.GetBytes(messageJson);

            await _webSocket.SendAsync(
                new ArraySegment<byte>(buffer),
                WebSocketMessageType.Text,
                true,
                _cts?.Token ?? CancellationToken.None);
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"发送消息失败: {ex.Message}");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync()
    {
        if (!IsConnected || _webSocket == null)
        {
            return;
        }

        try
        {
            // 取消消息接收循环
            _cts?.Cancel();

            // 发送关闭帧
            if (_webSocket.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "客户端主动断开",
                    CancellationToken.None);
            }

            // 清理资源
            CleanupResources();

            // 更新状态
            IsConnected = false;
            OnConnectionStateChanged(false);
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"断开连接时发生错误: {ex.Message}");
        }
        finally
        {
            CleanupResources();
        }
    }

    /// <inheritdoc/>
    public async Task ReconnectAsync()
    {
        if (string.IsNullOrEmpty(_serverUrl) || string.IsNullOrEmpty(_userName))
        {
            OnErrorOccurred("无法重新连接：缺少服务器URL或用户名");
            return;
        }

        try
        {
            await DisconnectAsync();

            // 重连延迟
            if (_reconnectAttempts > 0)
            {
                var delay = ReconnectDelayMs * _reconnectAttempts;
                await Task.Delay(delay);
            }

            await ConnectAsync(_serverUrl, _userName);
            _reconnectAttempts = 0;

            OnErrorOccurred("重新连接成功");
        }
        catch (Exception ex)
        {
            _reconnectAttempts++;
            OnErrorOccurred($"重新连接失败（尝试 {_reconnectAttempts}/{MaxReconnectAttempts}）: {ex.Message}");

            // 如果未达到最大重连次数，自动重试
            if (_reconnectAttempts < MaxReconnectAttempts)
            {
                await Task.Delay(ReconnectDelayMs);
                await ReconnectAsync();
            }
            else
            {
                OnErrorOccurred("已达到最大重连次数，请检查网络连接");
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        _ = DisconnectAsync();

        _webSocket?.Dispose();
        _cts?.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 接收消息的异步循环
    /// </summary>
    /// <param name="cancellationToken">取消令牌</param>
    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[ReceiveBufferSize];

        try
        {
            while (!cancellationToken.IsCancellationRequested &&
                   _webSocket != null &&
                   _webSocket.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationToken);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await HandleConnectionClosed();
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    await ProcessReceivedMessage(buffer, result.Count);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // 正常的取消操作，不记录错误
        }
        catch (WebSocketException ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                OnErrorOccurred($"WebSocket错误: {ex.Message}");
                await HandleConnectionClosed();
            }
        }
        catch (Exception ex)
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                OnErrorOccurred($"接收消息时发生错误: {ex.Message}");
                await HandleConnectionClosed();
            }
        }
    }

    /// <summary>
    /// 处理接收到的消息
    /// </summary>
    /// <param name="buffer">缓冲区</param>
    /// <param name="count">消息长度</param>
    private async Task ProcessReceivedMessage(byte[] buffer, int count)
    {
        try
        {
            var messageJson = Encoding.UTF8.GetString(buffer, 0, count);

            // 首先尝试解析为通用消息对象以确定消息类型
            using var jsonDoc = JsonDocument.Parse(messageJson);
            if (jsonDoc.RootElement.TryGetProperty("type", out var typeElement))
            {
                var messageType = typeElement.GetString();

                if (messageType == "OnlineUsersUpdate")
                {
                    // 处理在线用户更新
                    await ProcessOnlineUsersUpdate(messageJson);
                    return;
                }
            }

            // 处理聊天消息
            var message = JsonSerializer.Deserialize<ChatMessage>(messageJson, _jsonOptions);
            if (message != null)
            {
                OnMessageReceived(message);
            }
        }
        catch (JsonException ex)
        {
            OnErrorOccurred($"消息解析失败: {ex.Message}");
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"处理消息时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理在线用户列表更新
    /// </summary>
    /// <param name="messageJson">消息JSON</param>
    private async Task ProcessOnlineUsersUpdate(string messageJson)
    {
        try
        {
            var updateMessage = JsonSerializer.Deserialize<OnlineUsersUpdateMessage>(
                messageJson,
                _jsonOptions);

            if (updateMessage?.Users != null)
            {
                _onlineUsers = new List<UserInfo>(updateMessage.Users);
                OnOnlineUsersUpdated(_onlineUsers.ToArray());
            }
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"更新在线用户列表失败: {ex.Message}");
        }
    }

    /// <summary>
    /// 处理连接关闭
    /// </summary>
    private async Task HandleConnectionClosed()
    {
        try
        {
            CleanupResources();
            IsConnected = false;
            OnConnectionStateChanged(false);

            // 自动尝试重连
            if (_reconnectAttempts < MaxReconnectAttempts)
            {
                await ReconnectAsync();
            }
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"处理连接关闭时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 清理资源
    /// </summary>
    private void CleanupResources()
    {
        try
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            _webSocket?.Dispose();
            _webSocket = null;
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"清理资源时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 触发连接状态变化事件
    /// </summary>
    /// <param name="isConnected">是否已连接</param>
    private void OnConnectionStateChanged(bool isConnected)
    {
        try
        {
            ConnectionStateChanged?.Invoke(isConnected);
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"触发连接状态变化事件时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 触发消息接收事件
    /// </summary>
    /// <param name="message">聊天消息</param>
    private void OnMessageReceived(ChatMessage message)
    {
        try
        {
            MessageReceived?.Invoke(message);
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"触发消息接收事件时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 触发在线用户列表更新事件
    /// </summary>
    /// <param name="users">在线用户列表</param>
    private void OnOnlineUsersUpdated(UserInfo[] users)
    {
        try
        {
            OnlineUsersUpdated?.Invoke(users);
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"触发在线用户更新事件时发生错误: {ex.Message}");
        }
    }

    /// <summary>
    /// 触发错误事件
    /// </summary>
    /// <param name="errorMessage">错误消息</param>
    private void OnErrorOccurred(string errorMessage)
    {
        try
        {
            ErrorOccurred?.Invoke(errorMessage);
        }
        catch (Exception)
        {
            // 忽略触发错误事件时的异常
        }
    }

    /// <summary>
    /// 在线用户更新消息类
    /// </summary>
    private class OnlineUsersUpdateMessage
    {
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public List<UserInfo> Users { get; set; } = new();
    }
}
