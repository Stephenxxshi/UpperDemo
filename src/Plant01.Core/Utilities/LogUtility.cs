using NLog;

namespace Plant01.Core.Utilities
{
    public static class LogUtility
    {
        /// <summary>
        /// 初始化
        /// </summary>
        private static Logger Logger { get; }

        #region 初始化
        /// <summary>
        /// 静态构造
        /// </summary>
        static LogUtility()
        {
            Logger = LogManager.GetLogger("*");
        }
        #endregion

        #region 跟踪日志
        /// <summary>
        /// 跟踪日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogTrace(string message)
        {
            Logger.Trace(message);
        }
        #endregion

        #region 调试日志
        /// <summary>
        /// 调试日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogDebug(string message)
        {
            Logger.Debug(message);
        }
        #endregion

        #region 信息日志
        /// <summary>
        /// 信息日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogInformation(string message)
        {
            Logger.Info(message);
        }
        #endregion

        #region 警告日志
        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogWarning(string message)
        {
            Logger.Warn(message);
        }
        #endregion

        #region 错误日志
        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="message"></param>
        public static void LogError(Exception ex, string message)
        {
            Logger.Error(ex.GetBaseException(), message);
        }
        #endregion

        #region 致命日志
        /// <summary>
        /// 致命日志
        /// </summary>
        /// <param name="message"></param>
        public static void LogFatal(string message)
        {
            Logger.Fatal(message);
        }
        #endregion
    }
}
