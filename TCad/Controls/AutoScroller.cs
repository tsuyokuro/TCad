using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Plotter;

namespace TCad.Controls
{
    public class AutoScroller
    {
        private DispatcherTimer Timer;

        public Action<double, double> Scroll = (dx, dy) => {};

        private FrameworkElement CaptureView;

        private FrameworkElement ScrollView;

        private double CheckInterval = 0.05;

        public AutoScroller(FrameworkElement view, double checkInterval)
        {
            Init(view, checkInterval);
        }

        public AutoScroller(FrameworkElement view)
        {
            Init(view, CheckInterval);
        }

        public void Init(FrameworkElement view, double checkInterval)
        {
            CaptureView = view;
            ScrollView = view;

            CheckInterval = checkInterval;

            FrameworkElement parent = (ScrollViewer)view.Parent;

            if (parent is ScrollViewer)
            {
                ScrollView = parent;
            }

            Timer = new DispatcherTimer();
            Timer.Interval = TimeSpan.FromSeconds(CheckInterval);
            Timer.Tick += TimerTick;
        }

        public void Start()
        {
            if (!CaptureView.IsMouseCaptured)
            {
                //DOut.tpl("AutoScroller.Start");
                Mouse.Capture(CaptureView, CaptureMode.Element);
                Timer.Start();
            }
        }

        public void End()
        {
            //DOut.tpl("AutoScroller.End");
            Timer.Stop();
            Mouse.Capture(null);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            Check();
        }

        private void Check()
        {
            FrameworkElement v = ScrollView;
            
            var Pos = Mouse.GetPosition(v);

            double x = 0;
            double y = 0;

            if (Pos.X < 0)
            {
                x = Pos.X;
            }
            
            if (Pos.X > v.ActualWidth)
            {
                x = Pos.X - v.ActualWidth;
            }

            if (Pos.Y < 0)
            {
               y = Pos.Y;
            }

            if (Pos.Y > v.ActualHeight)
            {
                y = Pos.Y - v.ActualHeight;
            }

            if (x != 0 || y != 0)
            {
                Scroll(x, y);
            }
        }
    }
}
