using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;

namespace Plotter
{
    public abstract class EventSequencer<EventT> where EventT : EventSequencer<EventT>.Event, new()
    {
        public class Event
        {
            public int What = 0;
            public long ExpireTime = 0;

            public virtual void Clean()
            {
                What = 0;
                ExpireTime = 0;
            }

            public Event() { }

            public new String ToString()
            {
                return "Event What=" + What.ToString();
            }
        }

        private Task Looper;

        private bool ContinueLoop;

        private FlexBlockingQueue<EventT> Events;

        private List<EventT> DelayedEvents;

        private FlexBlockingQueue<EventT> FreeEvents;

        private int QueueSize = 5;

        private Timer CheckTimer;

        private Object LockObj = new Object();

        public EventSequencer(int queueSize)
        {
            QueueSize = queueSize;

            Events = new FlexBlockingQueue<EventT>(QueueSize);
            FreeEvents = new FlexBlockingQueue<EventT>(QueueSize);

            DelayedEvents = new List<EventT>();

            for (int i = 0; i < QueueSize; i++)
            {
                FreeEvents.Push(new EventT());
            }

            CheckTimer = new System.Timers.Timer();
            CheckTimer.Elapsed += new ElapsedEventHandler(OnElapsed_TimersTimer);

            //CheckTimer = new System.Threading.Timer(TimerCallback);

            Looper = new Task(Loop);
        }

        public EventT ObtainEvent()
        {
            EventT evt = FreeEvents.Pop();
            evt.Clean();
            return evt;
        }

        public void Post(EventT evt)
        {
            lock (LockObj)
            {
                Events.Push(evt);
            }
        }

        private long GetCurrentMilliSec()
        {
            return DateTime.Now.Ticks / 10000;
        }


        public void Post(EventT evt, int delay)
        {
            lock (LockObj)
            {
                evt.ExpireTime = GetCurrentMilliSec() + delay;
                DelayedEvents.Add(evt);
                UpdateTimer();
            }
        }

        public abstract void HandleEvent(EventT evt);

        public void Loop()
        {
            while (ContinueLoop)
            {
                EventT evt = Events.Pop();

                HandleEvent(evt);

                FreeEvents.Push(evt);
            }
        }

        public void Stop()
        {
            ContinueLoop = false;
        }

        public void Start()
        {
            ContinueLoop = true;
            Looper.Start();
        }

        private void TimerCallback(object state)
        {
            UpdateTimer();
        }

        void OnElapsed_TimersTimer(object sender, ElapsedEventArgs e)
        {
            CheckTimer.Stop();
            UpdateTimer();
        }

        private void UpdateTimer()
        {
            lock (LockObj)
            {
                long now = GetCurrentMilliSec();

                long minDt = long.MaxValue;

                if (DelayedEvents.Count > 0)
                {
                    foreach (EventT evt in DelayedEvents)
                    {
                        if (evt.ExpireTime == 0)
                        {
                            continue;
                        }

                        long t = evt.ExpireTime - now;

                        if (t <= 0)
                        {
                            evt.ExpireTime = 0;
                            Events.Push(evt);
                        }
                        else
                        {
                            if (t < minDt)
                            {
                                minDt = t;
                            }
                        }
                    }
                }
                DelayedEvents.RemoveAll(m => m.ExpireTime == 0);

                if (minDt != long.MaxValue)
                {
                    CheckTimer.Stop();
                    CheckTimer.Interval = minDt;
                    CheckTimer.Start();
                }
            }
        }

        public void RemoveAll(Predicate<Event> match)
        {
            lock (LockObj)
            {
                Events.RemoveAll(match, Removed);

                for (int i = DelayedEvents.Count - 1; i >= 0; i--)
                {
                    EventT item = DelayedEvents[i];

                    if (match(item))
                    {
                        DelayedEvents.RemoveAt(i);
                        Removed(item);
                    }
                }
            }
        }

        public void RemoveAll(int what)
        {
            RemoveAll(e => e.What == what);
        }

        private void Removed(EventT ev)
        {
            FreeEvents.Push(ev);
        }
    }
}
