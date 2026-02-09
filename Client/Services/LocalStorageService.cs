using Microsoft.JSInterop;
using System.Text.Json;
using WebSocketDemo.Shared.Services;

namespace WebSocketDemo.Client.Services
{
    /// <summary>
    /// 本地存储服务，用于在浏览器 localStorage 中保存和读取数据
    /// </summary>
    public class LocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly IConsoleLogger _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="jsRuntime">JavaScript 运行时</param>
        /// <param name="logger">日志记录器</param>
        public LocalStorageService(IJSRuntime jsRuntime, IConsoleLogger logger)
        {
            _jsRuntime = jsRuntime;
            _logger = logger;
        }

        /// <summary>
        /// 保存字符串值到本地存储
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">字符串值</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task SetItemAsync(string key, string value)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
            }
            catch (Exception ex)
            {
                // 在开发环境中可以记录错误，但不应影响应用运行
                _logger.Error($"保存到本地存储失败 (key: {key}): {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从本地存储读取字符串值
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>字符串值，如果不存在则返回 null</returns>
        public async Task<string?> GetItemAsync(string key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            }
            catch (Exception ex)
            {
                _logger.Error($"从本地存储读取失败 (key: {key}): {ex.Message}", ex);
                return null;
            }
        }

        /// <summary>
        /// 保存对象到本地存储（自动序列化为 JSON）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键名</param>
        /// <param name="value">对象值</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task SetObjectAsync<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await SetItemAsync(key, json);
            }
            catch (Exception ex)
            {
                _logger.Error($"保存对象到本地存储失败 (key: {key}): {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 从本地存储读取对象（自动从 JSON 反序列化）
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="key">键名</param>
        /// <returns>对象值，如果不存在或反序列化失败则返回默认值</returns>
        public async Task<T?> GetObjectAsync<T>(string key)
        {
            try
            {
                var json = await GetItemAsync(key);
                if (string.IsNullOrEmpty(json))
                {
                    return default;
                }

                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.Error($"从本地存储读取对象失败 (key: {key}): {ex.Message}", ex);
                return default;
            }
        }

        /// <summary>
        /// 从本地存储删除指定键的值
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>表示异步操作的任务</returns>
        public async Task RemoveItemAsync(string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            catch (Exception ex)
            {
                _logger.Error($"从本地存储删除失败 (key: {key}): {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 清空本地存储中的所有数据
        /// </summary>
        /// <returns>表示异步操作的任务</returns>
        public async Task ClearAsync()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.clear");
            }
            catch (Exception ex)
            {
                _logger.Error($"清空本地存储失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 检查本地存储中是否存在指定键
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>如果存在返回 true，否则返回 false</returns>
        public async Task<bool> ContainsKeyAsync(string key)
        {
            try
            {
                var value = await GetItemAsync(key);
                return value != null;
            }
            catch (Exception ex)
            {
                _logger.Error($"检查本地存储键是否存在失败 (key: {key}): {ex.Message}", ex);
                return false;
            }
        }

        /// <summary>
        /// 获取本地存储中所有键的数量
        /// </summary>
        /// <returns>键的数量</returns>
        public async Task<int> GetLengthAsync()
        {
            try
            {
                return await _jsRuntime.InvokeAsync<int>("eval", "localStorage.length");
            }
            catch (Exception ex)
            {
                _logger.Error($"获取本地存储长度失败: {ex.Message}", ex);
                return 0;
            }
        }

        /// <summary>
        /// 获取本地存储中指定索引的键名
        /// </summary>
        /// <param name="index">索引</param>
        /// <returns>键名，如果索引无效则返回 null</returns>
        public async Task<string?> KeyAsync(int index)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("localStorage.key", index);
            }
            catch (Exception ex)
            {
                _logger.Error($"获取本地存储键名失败 (index: {index}): {ex.Message}", ex);
                return null;
            }
        }
    }

    /// <summary>
    /// 用户设置模型
    /// </summary>
    public class UserSettings
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// 服务器地址
        /// </summary>
        public string ServerUrl { get; set; } = "ws://localhost:5015/ws";

        /// <summary>
        /// 是否自动连接
        /// </summary>
        public bool AutoConnect { get; set; } = false;

        /// <summary>
        /// 主题偏好
        /// </summary>
        public string Theme { get; set; } = "light";

        /// <summary>
        /// 消息通知设置
        /// </summary>
        public bool EnableNotifications { get; set; } = true;

        /// <summary>
        /// 消息声音设置
        /// </summary>
        public bool EnableSound { get; set; } = true;
    }
}
