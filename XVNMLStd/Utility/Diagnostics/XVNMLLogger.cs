using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace XVNML.Utility.Diagnostics
{
    public static class XVNMLLogger
    {
        private static ConcurrentQueue<string> LogQueue = new ConcurrentQueue<string>();

        public static void CollectLog(out string? message)
        {
            if (LogQueue.Count == 0)
            {
                message = null;
                return;
            }
            LogQueue.TryDequeue(out message);
        }

        internal static void WriteLog(string message)
        {
            LogQueue.Enqueue(message);
        }
    }
}
