using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using System.Windows;

namespace TCad
{
    public class DebugInputThread
    {
        private Action<string> mLineArrived;

        private Thread mThread;

        private bool mContinue;

        Dispatcher dispatcher = Application.Current.Dispatcher;

        public Action<string> OnLineArrived
        {
            set
            {
                mLineArrived = value;
            }
            get
            {
                return mLineArrived;
            }
        }


        public DebugInputThread()
        {
        }

        public void start()
        {
            if (mThread != null)
            {
                return;
            }

            mContinue = true;

            mThread = new Thread(run);
            mThread.Start();
        }

        public void stop()
        {
            if (mThread == null)
            {
                return;
            }

            mContinue = false;

            mThread.Join();
            mThread = null;
        }

        #pragma warning disable 168
        private void run()
        {
            while (mContinue)
            {
                string s = "";

                try
                {
                    Console.Write("> ");
                    s = Console.ReadLine();
                }
                catch (IOException e)
                {
                    break;
                }

                if (s != null && s.Length == 0)
                {
                    continue;
                }

                // Run on ui thread
                if (mLineArrived != null)
                {
                    dispatcher.Invoke(mLineArrived, s);
                }
            }
        }
        #pragma warning restore 0168
    }
}
