using System.Windows.Forms;

namespace Plotter
{
    class CadKeyboard
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern short GetKeyState(int nVirtKey);

        public static bool IsKeyPressed(Keys keyCode)
        {
            return GetKeyState((int)keyCode) < 0;
        }

        public static bool IsCtrlKeyDown()
        {
            bool ret = IsKeyPressed(Keys.ControlKey);
            return ret;
        }

        public static bool IsShiftKeyDown()
        {
            bool ret = IsKeyPressed(Keys.ShiftKey);
            return ret;
        }
    }
}
