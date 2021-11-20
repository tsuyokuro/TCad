using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plotter
{
    public static class ThreadUtil
    {
        public static TaskScheduler MainThreadScheduler = null;

        public static int MainThreadID = 0;

        public static void Init()
        {
            MainThreadScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            MainThreadID = System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public static bool IsMainThread()
        {
            return MainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId;
        }

        public static void RunOnMainThread(Action action, bool wait)
        {
            if (MainThreadID == System.Threading.Thread.CurrentThread.ManagedThreadId)
            {
                action();
                return;
            }

            Task task = new Task(() =>
            {
                action();
            }
            );

            task.Start(MainThreadScheduler);

            if (wait)
            {
                task.Wait();
            }
        }
    }
}
