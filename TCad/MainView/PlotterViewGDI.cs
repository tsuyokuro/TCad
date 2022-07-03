//#define MOUSE_THREAD

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Resources;
using CadDataTypes;
using TCad;
using OpenTK;
using OpenTK.Mathematics;
using Plotter.Controller;

namespace Plotter
{
    public partial class PlotterViewGDI : PictureBox, IPlotterView, IPlotterViewForDC
    {
        private PlotterController mController = null;

        private bool firstSizeChange = true;

        ContextMenuEx mCurrentContextMenu = null;
        ContextMenuEx mContextMenu = null;

        MyEventSequencer mEventSequencer;

        private DrawContextGDI mDrawContext = null;

        private Cursor PointCursor;

        public DrawContext DrawContext
        {
            get => mDrawContext;
        }

        public Control FormsControl
        {
            get => this;
        }

        public static PlotterViewGDI Create()
        {
            return new PlotterViewGDI();
        }

        private PlotterViewGDI()
        {
            mDrawContext = new DrawContextGDI(this);
            mDrawContext.SetupTools(DrawTools.DrawMode.DARK);

            SetupContextMenu();

            DoubleBuffered = false;

            SizeChanged += onSizeChanged;

            mEventSequencer = new MyEventSequencer(this, 100);

            mEventSequencer.Start();

            mDrawContext.PlotterView = this;

            MouseMove += OnMouseMove;
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
            MouseWheel += OnMouseWheel;

            Disposed += OnDisposed;

            SetupCursor();
        }

        private void OnDisposed(object sender, EventArgs e)
        {
            mDrawContext.Dispose();
        }

        protected void SetupCursor()
        {
            StreamResourceInfo si = System.Windows.Application.GetResourceStream(
                new Uri("/TCad;component/Resources/Cursors/dot.cur", UriKind.Relative));

            PointCursor = new Cursor(si.Stream);

            base.Cursor = PointCursor;
        }

        protected override void Dispose(bool disposing)
        {
            mDrawContext.Dispose();

            base.Dispose(disposing);
        }

        public void SetWorldScale(double scale)
        {
            mDrawContext.WorldScale = scale;
        }

        override protected void OnPaintBackground(PaintEventArgs pevent)
        {
            mController.Redraw();
        }

        private void onSizeChanged(object sender, System.EventArgs e)
        {
            if (Width > 0 && Height > 0)
            {
                mDrawContext.SetViewSize(Width, Height);

                if (firstSizeChange)
                {
                    Vector3d org = default;
                    org.X = Width / 2;
                    org.Y = Height / 2;

                    mDrawContext.SetViewOrg(org);

                    //DOut.pl($"{GetType().Name} onSizeChanged firstChange {Width}, {Height}");

                    mController.SetCursorWoldPos(Vector3d.Zero);

                    firstSizeChange = false;
                }

                Redraw();
            }
        }

        public void PushToFront(DrawContext dc)
        {
            //DOut.tpl("PushDraw");

            ThreadUtil.RunOnMainThread(() =>
            {
                if (dc == mDrawContext)
                {
                    //Image = mDrawContext.Image;
                    mDrawContext.Render();
                }
            }, true);
        }

        private void OnMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
#if MOUSE_THREAD
            // Mouse eventを別スレッドで処理
            // 未処理のEventは破棄
            mEventSequencer.RemoveAll(MyEventSequencer.MOUSE_MOVE);

            MyEvent evt = mEventSequencer.ObtainEvent();

            evt.What = MyEventSequencer.MOUSE_MOVE;
            evt.x = e.X;
            evt.y = e.Y;

            mEventSequencer.Post(evt);
#else
            // Mouse eventを直接処理
            mController.Mouse.MouseMove(mDrawContext, e.X, e.Y);
            Redraw();
#endif
        }

        private void OnMouseWheel(object sender, MouseEventArgs e)
        {
#if MOUSE_THREAD
            mEventSequencer.RemoveAll(MyEventSequencer.MOUSE_WHEEL);

            MyEvent evt = mEventSequencer.ObtainEvent();

            evt.What = MyEventSequencer.MOUSE_WHEEL;
            evt.EventArgs = e;

            mEventSequencer.Post(evt);
#else
            // 直接描画
            mController.Mouse.MouseWheel(mDrawContext, e.X, e.Y, e.Delta);
            Redraw();
#endif
        }

        private void OnMouseDown(Object sender, MouseEventArgs e)
        {
            if (mCurrentContextMenu != null)
            {
                if (mCurrentContextMenu.Visible)
                {
                    mCurrentContextMenu.Close();
                    return;
                }
            }

#if MOUSE_THREAD
            mEventSequencer.RemoveAll(MyEventSequencer.MOUSE_DOWN);

            MyEvent evt = mEventSequencer.ObtainEvent();

            evt.What = MyEventSequencer.MOUSE_DOWN;
            evt.EventArgs = e;

            mEventSequencer.Post(evt);
#else
            mController.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);
            Redraw();
#endif
        }

        private void OnMouseUp(Object sender, MouseEventArgs e)
        {
#if MOUSE_THREAD
            mEventSequencer.RemoveAll(MyEventSequencer.MOUSE_UP);

            MyEvent evt = mEventSequencer.ObtainEvent();

            evt.What = MyEventSequencer.MOUSE_UP;
            evt.EventArgs = e;

            mEventSequencer.Post(evt);
#else
            mController.Mouse.MouseUp(mDrawContext, e.Button, e.X, e.Y);
            Redraw();
#endif
        }

        private void SetupContextMenu()
        {
            mContextMenu = new ContextMenuEx();

            mContextMenu.StateChanged = (s) =>
            {
                if (s == ContextMenuEx.State.OPENED)
                {
                    base.Cursor = Cursors.Arrow;
                }
                else if (s == ContextMenuEx.State.CLOSED)
                {
                    base.Cursor = PointCursor;
                }
            };
        }

        public void ShowContextMenu(PlotterController sender, MenuInfo menuInfo, int x, int y)
        {
            ThreadUtil.RunOnMainThread(() => {
                ShowContextMenuProc(sender, menuInfo, x, y);
            }, true);
        }

        private void ShowContextMenuProc(PlotterController sender, MenuInfo menuInfo, int x, int y)
        {
            mContextMenu.Items.Clear();

            foreach (MenuInfo.Item item in menuInfo.Items)
            {
                ToolStripMenuItem m = new ToolStripMenuItem(item.Text);
                m.Tag = item;
                m.Click += ContextMenueClick;

                mContextMenu.Items.Add(m);
            }

            mCurrentContextMenu = mContextMenu;
            mCurrentContextMenu.Show(this, new Point(x, y));
        }

        private void ContextMenueClick(object sender, System.EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;

            MenuInfo.Item infoItem = item.Tag as MenuInfo.Item;

            if (infoItem != null)
            {
                mController.ContextMenuMan.ContextMenuEvent(infoItem);
            }
        }

        public void Redraw()
        {
            //DOut.tpl("Redraw");

            // PushDraw is called to redraw
            // PushDrawが呼ばれて再描画が行われる
            mController.Redraw();
        }

        public void SetController(PlotterController controller)
        {
            if (mController != null)
            {
                mController.Callback.RequestContextMenu -= ShowContextMenu;
            }

            mController = controller;

            if (controller != null)
            {
                mController.Callback.RequestContextMenu += ShowContextMenu;
            }
        }

        public void CursorLocked(bool locked)
        {
            if (locked)
            {
                base.Cursor = Cursors.Arrow;
            }
            else
            {
                base.Cursor = PointCursor;
            }
        }

        public void ChangeMouseCursor(PlotterCallback.MouseCursorType cursorType)
        {
            switch (cursorType)
            {
                case PlotterCallback.MouseCursorType.CROSS:
                    base.Cursor = PointCursor;
                    break;
                case PlotterCallback.MouseCursorType.NORMAL_ARROW:
                    base.Cursor = Cursors.Arrow;
                    break;
                case PlotterCallback.MouseCursorType.HAND:
                    base.Cursor = Cursors.Hand;
                    break;
            }
        }

        public void DrawModeUpdated(DrawTools.DrawMode mode)
        {
            if (mDrawContext != null)
            {
                mDrawContext.SetupTools(mode);
            }
        }

        public void GLMakeCurrent()
        {
            // NOP
        }

        class MyEvent : EventSequencer<MyEvent>.Event
        {
            public int x;
            public int y;
            public MouseEventArgs EventArgs;
        }

        class MyEventSequencer : EventSequencer<MyEvent>
        {
            public const int MOUSE_MOVE = 1;
            public const int MOUSE_WHEEL = 2;
            public const int MOUSE_DOWN = 3;
            public const int MOUSE_UP = 4;

            private PlotterViewGDI mPlotterView;

            public MyEventSequencer(PlotterViewGDI view, int queueSize) : base(queueSize)
            {
                mPlotterView = view;
            }

            public override void HandleEvent(MyEvent msg)
            {
                if (msg.What == MOUSE_MOVE)
                {
                    HandleMouseMove(msg.x, msg.y);
                }
                else if (msg.What == MOUSE_WHEEL)
                {
                    HandleMouseWheel(msg.EventArgs);
                }
                else if (msg.What == MOUSE_DOWN)
                {
                    HandleMouseDown(msg.EventArgs);
                }
                else if (msg.What == MOUSE_UP)
                {
                    HandleMouseUp(msg.EventArgs);
                }
            }

            public void HandleMouseMove(int x, int y)
            {
                try
                {
                    mPlotterView.mController.Mouse.MouseMove(mPlotterView.mDrawContext, x, y);
                    mPlotterView.Redraw();
                }
                catch (Exception ex)
                {
                    App.ThrowException(ex);
                }
            }

            public void HandleMouseWheel(MouseEventArgs e)
            {
                try
                {
                    mPlotterView.mController.Mouse.MouseWheel(mPlotterView.mDrawContext, e.X, e.Y, e.Delta);
                    mPlotterView.Redraw();
                }
                catch (Exception ex)
                {
                    App.ThrowException(ex);
                }
            }

            public void HandleMouseDown(MouseEventArgs e)
            {
                try
                {
                    mPlotterView.mController.Mouse.MouseDown(mPlotterView.mDrawContext, e.Button, e.X, e.Y);
                    mPlotterView.Redraw();
                }
                catch (Exception ex)
                {
                    App.ThrowException(ex);
                }
            }

            public void HandleMouseUp(MouseEventArgs e)
            {
                try
                {
                    mPlotterView.mController.Mouse.MouseUp(mPlotterView.mDrawContext, e.Button, e.X, e.Y);
                    mPlotterView.Redraw();
                }
                catch (Exception ex)
                {
                    App.ThrowException(ex);
                }
            }
        }
    }
}
