using Plotter;
using System;
using System.Windows.Media;

namespace TCad.Controls
{
    public class AnsiEsc
    {
        public const string ESC = "\x1b[";

        public const string Reset = ESC + "0m";

        public const string Balck = ESC + "30m";
        public const string Red = ESC + "31m";
        public const string Green = ESC + "32m";
        public const string Yellow = ESC + "33m";
        public const string Blue = ESC + "34m";
        public const string Magenta = ESC + "35m";
        public const string Cyan = ESC + "36m";
        public const string White = ESC + "37m";

        public const string BBalck = ESC + "90m";
        public const string BRed = ESC + "91m";
        public const string BGreen = ESC + "92m";
        public const string BYellow = ESC + "93m";
        public const string BBlue = ESC + "94m";
        public const string BMagenta = ESC + "95m";
        public const string BCyan = ESC + "96m";
        public const string BWhite = ESC + "97m";


        public const string BalckBG = ESC + "40m";
        public const string RedBG = ESC + "41m";
        public const string GreenBG = ESC + "42m";
        public const string YellowBG = ESC + "43m";
        public const string BlueBG = ESC + "44m";
        public const string MagentaBG = ESC + "45m";
        public const string CyanBG = ESC + "46m";
        public const string WhiteBG = ESC + "47m";

        public const string BBalckBG = ESC + "100m";
        public const string BRedBG = ESC + "101m";
        public const string BGreenBG = ESC + "102m";
        public const string BYellowBG = ESC + "103m";
        public const string BBlueBG = ESC + "104m";
        public const string BMagentaBG = ESC + "105m";
        public const string BCyanBG = ESC + "106m";
        public const string BWhiteBG = ESC + "107m";
    }

    public class AnsiPalette
    {
        public Brush[] Brushes;

        public byte DefaultFColor = 7;
        public byte DefaultBColor = 0;

        public AnsiPalette()
        {
            SetupStdPalette();
        }

        public void SetupStdPalette()
        {
            Brushes = new Brush[16];

            Brushes[0] = new SolidColorBrush(Colors.Black);
            Brushes[1] = new SolidColorBrush(Colors.MediumVioletRed);
            Brushes[2] = new SolidColorBrush(Colors.SeaGreen);
            Brushes[3] = new SolidColorBrush(Colors.Goldenrod);
            Brushes[4] = new SolidColorBrush(Colors.SteelBlue);
            Brushes[5] = new SolidColorBrush(Colors.DarkMagenta);
            Brushes[6] = new SolidColorBrush(Colors.DarkCyan);
            Brushes[7] = new SolidColorBrush(Colors.LightGray);

            Brushes[8] = new SolidColorBrush(Colors.Black);
            Brushes[9] = new SolidColorBrush(Colors.LightCoral);
            Brushes[10] = new SolidColorBrush(Colors.GreenYellow);
            Brushes[11] = new SolidColorBrush(Colors.Yellow);
            Brushes[12] = new SolidColorBrush(Colors.CornflowerBlue);
            Brushes[13] = new SolidColorBrush(Colors.MediumOrchid);
            Brushes[14] = new SolidColorBrush(Colors.Turquoise);
            Brushes[15] = new SolidColorBrush(Colors.White);
        }
    }
}
