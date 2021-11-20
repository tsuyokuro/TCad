using TCad.Controls;
using TCad.Properties;
using System;

namespace Plotter
{
    public class ItConsole
    {
        public static Action<string> Print = (s) => {};
        public static Action<string> PrintLn = (s) => {};
        public static Action<string, object []> PrintF = (s, args) => { };
        public static Action Clear = () => { };
        public static Func<string, string, string> GetString = (msg, def) => { return ""; }; 

        public static void print(string s)
        {
            Print(s);
        }

        public static void println(string s)
        {
            PrintLn(s);
        }

        public static void printf(string s, params object[] args)
        {
            PrintF(s, args);
        }

        public static void printError(string s)
        {
            println(
                    AnsiEsc.RedBG +
                    " " + Resources.error_title + ": " + s + " "
                    );
        }

        public static void printFaile(string s)
        {
            println(
                    AnsiEsc.Yellow +
                    Resources.faile_title + ": " + s
                    );
        }

        public static string getString(string msg, string defString)
        {
            return GetString(msg, defString);
        }
    }
}
