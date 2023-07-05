using System.Collections.Concurrent;

namespace XVNML.Utility.Diagnostics
{
    public static class XVNMLLogger
    {
        internal static ConcurrentQueue<XVNMLLogMessage> LoggerQueue = new ConcurrentQueue<XVNMLLogMessage>();

        internal static void Log(string msg, object? context)
        {
            XVNMLLogMessage message = new XVNMLLogMessage()
            {
                Message = msg,
                Context = context,
                Level = XVNMLLogLevel.Standard
            };
            LoggerQueue.Enqueue(message);
        }

        internal static void LogError(string msg, object? context, object? blame)
        {
            XVNMLLogMessage message = new XVNMLLogMessage()
            {
                Message = msg,
                Context = context,
                Blame = blame,
                Level = XVNMLLogLevel.Error
            };
            LoggerQueue.Enqueue(message);
        }

        internal static void LogWarning(string msg, object? context)
        {
            XVNMLLogMessage message = new XVNMLLogMessage()
            {
                Message = msg,
                Context = context,
                Level = XVNMLLogLevel.Warning
            };
            LoggerQueue.Enqueue(message);
        }

        public static void CollectLog(out XVNMLLogMessage? message)
        {
            if (LoggerQueue.Count == 0)
            {
                message = null;
                return;
            }
            LoggerQueue.TryDequeue(out message);
        }
    }
}
