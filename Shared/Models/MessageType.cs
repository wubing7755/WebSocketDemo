using System;

namespace WebSocketDemo.Shared.Models
{
    /// <summary>
    /// WebSocket 消息类型枚举
    /// </summary>
    public enum MessageType
    {
        /// <summary>
        /// 普通文本消息
        /// </summary>
        TextMessage = 0,

        /// <summary>
        /// 用户加入聊天室
        /// </summary>
        UserJoined = 1,

        /// <summary>
        /// 用户离开聊天室
        /// </summary>
        UserLeft = 2,

        /// <summary>
        /// 系统通知
        /// </summary>
        SystemNotification = 3,

        /// <summary>
        /// 错误消息
        /// </summary>
        Error = 4
    }
}
