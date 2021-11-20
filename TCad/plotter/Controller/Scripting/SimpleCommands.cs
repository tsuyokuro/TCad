using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Plotter.Controller
{
    public class SipmleCommands
    {
        private PlotterController Controller;

        public SipmleCommands(PlotterController controller)
        {
            Controller = controller;
        }

        public List<string> GetAutoCompleteForSimpleCmd()
        {
            List<string> autoComps = new List<string>();

            autoComps.Add("@clear");
            autoComps.Add("@cls");
            autoComps.Add("@help key");
            autoComps.Add("@dump db");
            autoComps.Add("@dump dc");
            autoComps.Add("@dump fig");
            autoComps.Add("@dump layer");
            autoComps.Add("@bench draw");

            return autoComps;
        }

        public void BenchDraw()
        {
            ItConsole.println("BenchDraw start");

            Stopwatch sw = new Stopwatch();
            sw.Start();
            int i = 0;
            while (i<120)
            {
                ThreadUtil.RunOnMainThread(() =>
                {
                    Controller.ReflectToView();
                }, true);
                i++;
            }
            sw.Stop();

            ItConsole.println("BenchDraw end");
            ItConsole.println($"BenchDraw cnt:{i} time:{sw.ElapsedMilliseconds}ms");
        }

        public bool ExecCommand(string s)
        {
            string[] ss = Regex.Split(s, @"[ \t]+");

            string cmd = ss[0];


            if (cmd == "@clear" || s == "@cls")
            {
                ItConsole.Clear();
            }
            else if (cmd == "@bench")
            {
                if (ss[1] == "draw")
                {
                    BenchDraw();
                }
            }
            else if (cmd == "@dump")
            {
                if (ss[1] == "db")
                {
                    Controller.DB.dump();
                }
                else if (ss[1] == "dc")
                {
                    Controller.DC.dump();
                }
                else if (ss[1] == "fig")
                {
                    if (Controller.CurrentFigure != null)
                    {
                        Controller.CurrentFigure.Dump();
                    }
                }
                else if (ss[1] == "layer")
                {
                    if (Controller.CurrentLayer != null)
                    {
                        Controller.CurrentLayer.dump();
                    }
                }
            }
            else if (cmd == "@help")
            {
                if (ss.Length > 1)
                {
                    if (ss[1] == "key")
                    {
                        HelpOfKey(ss.Length > 2 ? ss[2] : null);
                    }
                }
            }
            else if (cmd == "@clearTemp")
            {
                Controller.TempFigureList.Clear();
            }
            else
            {
                return false;
            }

            return true;
        }

        private void HelpOfKey(string keyword)
        {
            List<string> res = Controller.Callback.HelpOfKey(keyword);

            res.ForEach((s) =>
            {
                ItConsole.println(s);
            });
        }
    }
}