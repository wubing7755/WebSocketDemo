namespace WebSocketDemo.Shared.Models;

/// <summary>
/// 聊天消息模型
/// </summary>
[Serializable]
public class ChatMessage
{
    /// <summary>
    /// 消息唯一标识符
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// 发送者用户ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 发送者昵称
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 消息内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 消息发送时间
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType MessageType { get; set; }

    /// <summary>
    /// 默认构造函数
    /// </summary>
    public ChatMessage()
    {
        MessageId = Guid.NewGuid();
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// 带参数的构造函数
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="userName">用户名</param>
    /// <param name="content">消息内容</param>
    /// <param name="messageType">消息类型</param>
    public ChatMessage(string userId, string userName, string content, MessageType messageType = MessageType.TextMessage)
    {
        MessageId = Guid.NewGuid();
        UserId = userId;
        UserName = userName;
        Content = content;
        MessageType = messageType;
        Timestamp = DateTime.UtcNow;
    }

    /// <summary>
    /// 创建系统消息
    /// </summary>
    /// <param name="content">系统消息内容</param>
    /// <returns>系统消息</returns>
    public static ChatMessage CreateSystemMessage(string content)
    {
        return new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            UserId = "system",
            UserName = "系统",
            Content = content,
            MessageType = MessageType.SystemNotification,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建用户加入消息
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <returns>用户加入消息</returns>
    public static ChatMessage CreateUserJoinedMessage(string userName)
    {
        return new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            UserId = "system",
            UserName = "系统",
            Content = $"{userName} 加入了聊天室",
            MessageType = MessageType.UserJoined,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建用户离开消息
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <returns>用户离开消息</returns>
    public static ChatMessage CreateUserLeftMessage(string userName)
    {
        return new ChatMessage
        {
            MessageId = Guid.NewGuid(),
            UserId = "system",
            UserName = "系统",
            Content = $"{userName} 离开了聊天室",
            MessageType = MessageType.UserLeft,
            Timestamp = DateTime.UtcNow
        };
    }
}
