using TCad.Properties;
using Microsoft.Scripting.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TCad.Controls;
using OpenTK;
using System.Threading;
using IronPython.Hosting;
using IronPython.Runtime.Exceptions;
using Microsoft.Scripting;
using System.Diagnostics;

namespace Plotter.Controller
{
    public partial class ScriptEnvironment
    {
        public PlotterController Controller;

        private ScriptEngine Engine;

        private ScriptScope mScope;
        public ScriptScope Scope
        {
            get => mScope;
        }

        private ScriptSource Source;

        private List<string> mAutoCompleteList = new List<string>();
        public List<string> AutoCompleteList
        {
            get => mAutoCompleteList;
        }

        private ScriptFunctions mScriptFunctions;

        private SipmleCommands mSimpleCommands;

        private TestCommands mTestCommands;

        public ScriptEnvironment(PlotterController controller)
        {
            Controller = controller;

            mScriptFunctions = new ScriptFunctions();

            mSimpleCommands = new SipmleCommands(controller);

            mTestCommands = new TestCommands(controller);

            InitScriptingEngine();

            mScriptFunctions.Init(this, mScope);
        }

        Regex AutoCompPtn = new Regex(@"#\[AC\][ \t]*(.+)\n");

        private string getBaseSacript()
        {
            string script;

            //string path = AppDomain.CurrentDomain.BaseDirectory;
            //string filePath = path + "BaseScript.py";
            //if (File.Exists(filePath))
            //{
            //    script = File.ReadAllText(filePath);
            //}
            //else
            //{
            //    script = Encoding.GetEncoding("Shift_JIS").GetString(Resources.BaseScript);
            //}

            script = Encoding.GetEncoding("Shift_JIS").GetString(Resources.BaseScript);

            return script;
        }

        private void InitScriptingEngine()
        {
            string script = getBaseSacript();

            //string script = "";

            Engine = Python.CreateEngine();
            
            mScope = Engine.CreateScope();
            Source = Engine.CreateScriptSourceFromString(script);

            mScope.SetVariable("SE", mScriptFunctions);
            Source.Execute(mScope);

            MatchCollection matches = AutoCompPtn.Matches(script);

            foreach (Match m in matches)
            {
                string s = m.Groups[1].Value.TrimEnd('\r', '\n');
                mAutoCompleteList.Add(s);
            }

            mAutoCompleteList.AddRange(mSimpleCommands.GetAutoCompleteForSimpleCmd());
        }

        bool StopScript = false;

        /*
        private TracebackDelegate OnTraceback
            (TraceBackFrame frame, string result, object payload)
        {
            if (StopScript)
            {
                throw new KeyboardInterruptException("");
            }

            return OnTraceback;
        }
        */

        public async void ExecuteCommandAsync(string s)
        {
            s = s.Trim();
            ItConsole.println(s);

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

            await Task.Run( () =>
            {
                RunScript(s);
            });

            Controller.Clear();
            Controller.DrawAll();
            Controller.ReflectToView();
        }

        public dynamic RunScript(string s)
        {
            mScriptFunctions.StartSession();

            dynamic ret = null;

            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                
                ret = Engine.Execute(s, mScope);

                sw.Stop();
                ItConsole.println("Exec time:" + sw.ElapsedMilliseconds + "(msec)" );

                if (ret != null)
                {
                    if (ret is Double || ret is Int32 || ret is float)
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
                }
            }
            catch (KeyboardInterruptException e)
            {
                mScriptFunctions.EndSession();
                ItConsole.println(AnsiEsc.BRed + "Canceled");
            }
            catch (Exception e)
            {
                mScriptFunctions.EndSession();
                ItConsole.println(AnsiEsc.BRed + "Error: " + e.Message);
            }

            mScriptFunctions.EndSession();

            return ret;
        }

        public async void RunScriptAsync(string s, RunCallback callback)
        {
            if (callback != null)
            {
                callback.OnStart();
            }

            StopScript = false;

            await Task.Run(() =>
            {
                Engine.SetTrace(OnTraceback);
                RunScript(s);
            });

            Controller.Clear();
            Controller.DrawAll();
            Controller.ReflectToView();

            if (callback != null)
            {
                callback.OnEnd();
            }

            TracebackDelegate OnTraceback
                (TraceBackFrame frame, string result, object payload)
            {
                if (StopScript)
                {
                    throw new KeyboardInterruptException("");
                }

                if (callback != null && callback.onTrace != null)
                {
                    if (!callback.onTrace(frame, result, payload))
                    {
                        throw new KeyboardInterruptException("");
                    }
                }

                return OnTraceback;
            }
        }

        public void CancelScript()
        {
            StopScript = true;
        }

        public class RunCallback
        {
            public Action OnStart = () => { };
            public Action OnEnd = () => { };
            public Func<TraceBackFrame, string, object, bool> onTrace = 
                (frame, result, payload) => { return true; };
        }

        public void RunOnMainThread(Action action)
        {
            ThreadUtil.RunOnMainThread(action, true);
        }

        public void OpenPopupMessage(string text, PlotterCallback.MessageType type)
        {
            Controller.Callback.OpenPopupMessage(text, type);
        }

        public void ClosePopupMessage()
        {
            Controller.Callback.ClosePopupMessage();
        }
    }
}
