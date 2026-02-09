using System;

namespace WebSocketDemo.Shared.Models
{
    /// <summary>
    /// 用户连接信息模型
    /// </summary>
    [Serializable]
    public class UserInfo
    {
        /// <summary>
        /// 用户唯一标识符
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// WebSocket连接ID
        /// </summary>
        public string ConnectionId { get; set; } = string.Empty;

        /// <summary>
        /// 用户在线状态
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// 最后活动时间
        /// </summary>
        public DateTime LastActiveTime { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public UserInfo()
        {
            UserId = Guid.NewGuid().ToString();
            LastActiveTime = DateTime.UtcNow;
            IsOnline = true;
        }

        /// <summary>
        /// 带参数的构造函数
        /// </summary>
        /// <param name="userName">用户昵称</param>
        /// <param name="connectionId">WebSocket连接ID</param>
        public UserInfo(string userName, string connectionId)
        {
            UserId = Guid.NewGuid().ToString();
            UserName = userName;
            ConnectionId = connectionId;
            IsOnline = true;
            LastActiveTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 带完整参数的构造函数
        /// </summary>
        /// <param name="userId">用户ID</param>
        /// <param name="userName">用户昵称</param>
        /// <param name="connectionId">WebSocket连接ID</param>
        /// <param name="isOnline">在线状态</param>
        /// <param name="lastActiveTime">最后活动时间</param>
        public UserInfo(string userId, string userName, string connectionId, bool isOnline, DateTime lastActiveTime)
        {
            UserId = userId;
            UserName = userName;
            ConnectionId = connectionId;
            IsOnline = isOnline;
            LastActiveTime = lastActiveTime;
        }

        /// <summary>
        /// 更新最后活动时间
        /// </summary>
        public void UpdateLastActiveTime()
        {
            LastActiveTime = DateTime.UtcNow;
        }

        /// <summary>
        /// 用户上线
        /// </summary>
        public void SetOnline()
        {
            IsOnline = true;
            UpdateLastActiveTime();
        }

        /// <summary>
        /// 用户离线
        /// </summary>
        public void SetOffline()
        {
            IsOnline = false;
            UpdateLastActiveTime();
        }

        /// <summary>
        /// 更新连接ID
        /// </summary>
        /// <param name="newConnectionId">新的连接ID</param>
        public void UpdateConnectionId(string newConnectionId)
        {
            ConnectionId = newConnectionId;
            UpdateLastActiveTime();
        }

        /// <summary>
        /// 创建匿名用户
        /// </summary>
        /// <param name="connectionId">连接ID</param>
        /// <returns>匿名用户信息</returns>
        public static UserInfo CreateAnonymousUser(string connectionId)
        {
            return new UserInfo($"游客_{Guid.NewGuid().ToString()[..8]}", connectionId);
        }
    }
}
