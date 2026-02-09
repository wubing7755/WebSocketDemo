namespace WebSocketDemo.Shared.Services
{
    /// <summary>
    /// 控制台日志记录器实现
    /// </summary>
    public class ConsoleLogger : IConsoleLogger
    {
        /// <summary>
        /// 记录日志到控制台
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">可选异常</param>
        public void Log(LogLevel level, string message, Exception? exception = null)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logEntry = $"[{timestamp}] [{level.ToString().ToUpper()}] {message}";
            
            if (exception != null)
            {
                logEntry += $"{Environment.NewLine}异常: {exception.Message}{System.Environment.NewLine}堆栈跟踪: {exception.StackTrace}";
            }

            Console.WriteLine(logEntry);
        }

        /// <summary>
        /// 记录调试级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// 记录信息级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// 记录警告级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// 记录错误级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">可选异常</param>
        public void Error(string message, Exception? exception = null)
        {
            Log(LogLevel.Error, message, exception);
        }

        /// <summary>
        /// 记录严重错误级别日志
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">可选异常</param>
        public void Critical(string message, Exception? exception = null)
        {
            Log(LogLevel.Critical, message, exception);
        }
    }
}