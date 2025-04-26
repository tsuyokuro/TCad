using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using TCad.Plotter;
using TCad.Plotter.Controller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using TCad.Controls.CadConsole;
using TCad.ViewModel;

namespace TCad.Plotter.Scripting;

public partial class ScriptEnvironment
{
    public IPlotterController Controller;

    private ScriptEngine Engine;

    private ScriptScope mScope;
    public ScriptScope Scope
    {
        get => mScope;
    }

    private ScriptSource Source;

    private readonly List<string> mAutoCompleteList = new();
    public List<string> AutoCompleteList
    {
        get => mAutoCompleteList;
    }

    private readonly ScriptFunctions mScriptFunctions;

    private readonly DirectCommands mSimpleCommands;

    private readonly TestCommands mTestCommands;

    public ScriptEnvironment(IPlotterController controller)
    {
        Log.plx("in");

        Controller = controller;

        mScriptFunctions = new ScriptFunctions();

        mSimpleCommands = new DirectCommands(controller);

        mTestCommands = new TestCommands(controller);

        InitScriptingEngine();

        mScriptFunctions.Init(this, mScope);

        Log.plx("out");
    }

    private static readonly Regex AutoCompPtn = new(@"#\[AC\][ \t]*(.+)\n");

    private static string GetBaseSacript()
    {
        string script = "";

        string path = AppDomain.CurrentDomain.BaseDirectory;
        string filePath = path + @"Resources\BaseScript.py";
        if (File.Exists(filePath))
        {
            script = File.ReadAllText(filePath);
        }
        else
        {
            MessageBox.Show(
                "BaseScript.py is not found",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        //script = Encoding.GetEncoding("Shift_JIS").GetString(Resources.BaseScript);

        return script;
    }

    private void InitScriptingEngine()
    {
        string script = GetBaseSacript();

        //string script = "";

        Engine = Python.CreateEngine();

        mScope = Engine.CreateScope();
        Source = Engine.CreateScriptSourceFromString(script);

        mScope.SetVariable("SE", mScriptFunctions);
        Source.Execute(mScope);

        MatchCollection matches = AutoCompPtn.Matches(script);

        foreach (object o in matches)
        {
            if (o is not Match m) continue;

            string s = m.Groups[1].Value.TrimEnd('\r', '\n');
            mAutoCompleteList.Add(s);
        }

        mAutoCompleteList.AddRange(mSimpleCommands.GetAutoCompleteForSimpleCmd());
    }

    public void OpenPopupMessage(string text, UITypes.MessageType type)
    {
        Controller.OpenPopupMessage(text, type);
    }

    public void ClosePopupMessage()
    {
        Controller.ClosePopupMessage();
    }

    public async void ExecuteCommandAsync(string s)
    {
        s = s.Trim();
        ItConsole.println(s);

        // Command is internal command 
        if (s.StartsWith("@"))
        {
            await Task.Run(() =>
            {
                if (!mSimpleCommands.ExecCommand(s))
                {
                    mTestCommands.ExecCommand(s);
                }
            });

            return;
        }

        // Command is python

        await Task.Run(() =>
        {
            RunScript(s, false);
        });

        Controller.Drawer.Clear();
        Controller.Drawer.DrawAll();
        Controller.Drawer.UpdateView();
    }

    private Thread mScriptThread = null;
    private TraceBack mTraceBack = null;

    public async void RunScriptAsync(string s, bool snapshotDB, RunCallback callback)
    {
        if (mScriptThread != null)
        {
            callback.OnStart();
            callback.OnEnd();
            return;
        }

        mScriptFunctions.StartSession(snapshotDB);

        if (callback != null)
        {
            callback.OnStart();
        }

        PrepareRunScript();

        //mTraceBack = new TraceBack(this, callback);

        await Task.Run(() =>
        {
            mScriptThread = new Thread(() =>
            {
                if (mTraceBack != null)
                {
                    Engine.SetTrace(mTraceBack.OnTraceback);
                }

                InternalRunScript(s);
            });

            mScriptThread.Start();
            mScriptThread.Join();

            mScriptThread = null;
            mTraceBack = null;
        });

        Controller.Drawer.Clear();
        Controller.Drawer.DrawAll();
        Controller.Drawer.UpdateView();
        Controller.UpdateObjectTree(true);

        if (callback != null)
        {
            callback.OnEnd();
        }

        mScriptFunctions.EndSession();
    }

    public dynamic RunScript(string s, bool snapshotDB)
    {
        mScriptFunctions.StartSession(snapshotDB);

        dynamic ret = InternalRunScript(s);

        mScriptFunctions.EndSession();

        return ret;
    }

    private dynamic InternalRunScript(string s)
    {
        dynamic ret = null;

        try
        {
            Stopwatch sw = new();
            sw.Start();

            ret = Engine.Execute(s, mScope);

            sw.Stop();
            ItConsole.println("Exec time:" + sw.ElapsedMilliseconds + "(msec)");

            if (ret != null)
            {
                if (ret is vcompo_t or int or float)
                {
                    ItConsole.println(AnsiEsc.BGreen + ret.ToString());
                }
                else if (ret is string)
                {
                    ItConsole.println(AnsiEsc.BGreen + ret);
                }
                else if (ret is bool)
                {
                    ItConsole.println(AnsiEsc.BGreen + ret.ToString());
                }
                else if (ret is vector3_t)
                {
                    vector3_t v = ret;
                    ItConsole.println(AnsiEsc.BGreen + "(" + v.X + "," + v.Y + "," + v.Z + ")");
                }
                else
                {
                    ItConsole.println("Object: " + AnsiEsc.BGreen + ret.ToString());
                }
            }
        }
        catch (KeyboardInterruptException)
        {
            ItConsole.println(AnsiEsc.BRed + "Canceled");
        }
        catch (ThreadInterruptedException)
        {
            // NOP
        }
        catch (Exception e)
        {
            ItConsole.println(AnsiEsc.BRed + "Error: " + e?.Message);
        }

        return ret;
    }

    public void PrepareRunScript()
    {
        Engine.Execute("reset_cancel()", mScope);
    }

    public void CancelScript()
    {
        if (mTraceBack != null)
        {
            mTraceBack.StopScript = true;
        }
        else
        {
            if (mScriptThread != null)
            {
                try
                {
                    mScriptThread.Interrupt();
                    mScriptThread = null;
                }
                catch
                {
                }
            }

            Engine.Execute("raise_cancel()", mScope);
        }
    }

    public class RunCallback
    {
        public Action OnStart = () => { };
        public Action OnEnding = () => { };
        public Action OnEnd = () => { };
        public Func<TraceBackFrame, string, object, bool> onTrace =
            (frame, result, payload) => { return true; };
    }

    public class TraceBack
    {
        private readonly ScriptEnvironment Env;

        public bool StopScript = false;

        public RunCallback CallBack;

        public TracebackDelegate OnTraceback(TraceBackFrame frame, string result, object payload)
        {
            if (StopScript)
            {
                //throw new KeyboardInterruptException("");
                Env.Engine.Execute("sys.exit()", Env.mScope);
            }

            if (CallBack != null && CallBack.onTrace != null)
            {
                if (!CallBack.onTrace(frame, result, payload))
                {
                    //throw new KeyboardInterruptException("");
                    Env.Engine.Execute("sys.exit()", Env.mScope);
                }
            }

            return OnTraceback;
        }

        public TraceBack(ScriptEnvironment env, RunCallback callback)
        {
            Env = env;
            CallBack = callback;
        }
    }
}
