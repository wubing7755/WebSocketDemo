namespace WebSocketDemo.Shared.Services
{
    /// <summary>
    /// 日志记录器接口
    /// </summary>
    public interface IConsoleLogger
    {
        /// <summary>
        /// 记录日志
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">可选异常</param>
        void Log(LogLevel level, string message, Exception? exception = null);

        /// <summary>
        /// 记录调试级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Debug(string message);

        /// <summary>
        /// 记录信息级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Info(string message);

        /// <summary>
        /// 记录警告级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Warning(string message);

        /// <summary>
        /// 记录错误级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">可选异常</param>
        void Error(string message, Exception? exception = null);

        /// <summary>
        /// 记录严重错误级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">可选异常</param>
        void Critical(string message, Exception? exception = null);
    }
}