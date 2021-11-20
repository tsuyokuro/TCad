#define MOUSE_THREAD

using System;
using System.Windows.Forms;

namespace Plotter
{
    public class ContextMenuEx : ContextMenuStrip
    {
        public enum State
        {
            OPENED,
            CLOSED,
        }

        private Action<State> mStateChanged = (a) => { };

        public Action<State> StateChanged
        {
            get => mStateChanged;

            set
            {
                mStateChanged = value == null ? (a) => { } : value;
            }
        }

        public ContextMenuEx()
        {
        }

        protected override void OnItemClicked(ToolStripItemClickedEventArgs e)
        {
            Close();
            base.OnItemClicked(e);
        }

        protected override void OnOpened(EventArgs e)
        {
            StateChanged(State.OPENED);
        }

        protected override void OnClosed(ToolStripDropDownClosedEventArgs e)
        {
            StateChanged(State.CLOSED);
        }
    }
}
