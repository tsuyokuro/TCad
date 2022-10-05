using MessagePack;
using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Collections;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using TCad.Controls;
using OpenTK.Mathematics;
using Plotter;
using System.Timers;
using System.Collections.Concurrent;

namespace TestApp;

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

    private BlockingCollection<EventT> Events;

    private List<EventT> DelayedEvents;

    private BlockingCollection<EventT> FreeEvents;

    private int QueueSize = 5;

    private System.Timers.Timer CheckTimer;

    private Object LockObj = new Object();

    public EventSequencer(int queueSize)
    {
        QueueSize = queueSize;

        Events = new BlockingCollection<EventT>(QueueSize);
        FreeEvents = new BlockingCollection<EventT>(QueueSize);

        DelayedEvents = new List<EventT>();

        for (int i = 0; i < QueueSize; i++)
        {
            FreeEvents.Add(new EventT());
        }

        CheckTimer = new System.Timers.Timer();
        CheckTimer.Elapsed += new ElapsedEventHandler(OnElapsed_TimersTimer);

        //CheckTimer = new System.Threading.Timer(TimerCallback);

        Looper = new Task(Loop);
    }

    public EventT ObtainEvent()
    {
        EventT evt = FreeEvents.Take();
        evt.Clean();
        return evt;
    }

    public void Post(EventT evt)
    {
        lock (LockObj)
        {
            Events.Add(evt);
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
            EventT evt = Events.Take();

            HandleEvent(evt);

            FreeEvents.Add(evt);
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
                        Events.Add(evt);
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
            BlockingCollection<EventT> newEvents = new(QueueSize);

            foreach (EventT evt in Events) {
                if (!match(evt))
                {
                    newEvents.Add(evt);
                }
                else
                {
                    Removed(evt);
                }
            }

            Events = newEvents;

            List<EventT> newList = new List<EventT>();
            foreach (EventT evt in DelayedEvents)
            {
                if (!match(evt))
                {
                    newList.Add(evt);
                }
                else
                {
                    Removed(evt);
                }
            }

            DelayedEvents = newList;
        }
    }

    public void RemoveAll(int what)
    {
        RemoveAll(e => e.What == what);
    }

    private void Removed(EventT ev)
    {
        FreeEvents.Add(ev);
    }
}

public class MyEvent : EventSequencer<MyEvent>.Event
{
    int Value;
}

internal class Program
{

    static void test002()
    {
        Console.WriteLine("");
    }

    static void Main(string[] args)
    {
        test002();
        Console.WriteLine("<<< END >>>");
        Console.ReadLine();
    }
}
