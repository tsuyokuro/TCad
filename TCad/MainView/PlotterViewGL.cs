#define MOUSE_THREAD

using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using TCad.Plotter.Settings;
using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Resources;

using TCad.Plotter.Controller;
using TCad.MathFunctions;
using TCad.Plotter.DrawContexts;
using TCad.Util;
using TCad.ViewModel;
using TCad.Logger;

namespace TCad.Plotter;

class PlotterViewGL : GLControl, IPlotterView, IPlotterViewForDC
{
    private DrawContextGL mDrawContext = null;

    private IPlotterController mController = null;

    private vector3_t PrevMousePos = default;

    private MouseButtons DownButton = MouseButtons.None;

    private ContextMenuEx mCurrentContextMenu = null;
    private ContextMenuEx mContextMenu = null;

    private MyEventHandler mEventSequencer;

    private Cursor PointCursorWhite;
    private Cursor PointCursorBlack;


    public DrawContext DrawContext => mDrawContext;

    public Control FormsControl => this;


    private DrawContextGLOrtho mDrawContextOrtho;

    private DrawContextGLPers mDrawContextPers;


    public static PlotterViewGL Create(IPlotterController controller)
    {
        Log.plx("in");
        PlotterViewGL v = new PlotterViewGL(controller);
        v.MakeCurrent();
        Log.plx("out");
        return v;
    }

    private PlotterViewGL(IPlotterController controller)
    {
        mController = controller;

        SetupContextMenu();

        base.Flags = OpenTK.Windowing.Common.ContextFlags.Default;
        base.Profile = OpenTK.Windowing.Common.ContextProfile.Compatability;

        Load += OnLoad;
        SizeChanged += OnResize;
        Paint += OnPaint;
        MouseMove += OnMouseMove;
        MouseDown += OnMouseDown;
        MouseUp += OnMouseUp;
        MouseWheel += OnMouseWheel;

        Disposed += OnDisposed;

        SetupCursor();

#if MOUSE_THREAD
        mEventSequencer = new MyEventHandler(this, 100);
        mEventSequencer.Start();
#endif
    }

    private void OnDisposed(object sender, EventArgs e)
    {
        mDrawContext.Dispose();
    }

    private void OnLoad(object sender, EventArgs e)
    {
        Log.plx("in");

        GL.ClearColor(Color4.Black);
        GL.Enable(EnableCap.DepthTest);

        mDrawContextOrtho = new DrawContextGLOrtho(this);
        mDrawContextOrtho.SetupTools(SettingsHolder.Settings.DrawMode);

        mDrawContextPers = new DrawContextGLPers(this);
        mDrawContextPers.SetupTools(SettingsHolder.Settings.DrawMode);

        mDrawContext = mDrawContextOrtho;

        mDrawContextOrtho.PlotterView = this;
        mDrawContextPers.PlotterView = this;

        SwapBuffers();

        Log.plx("out");
    }

    protected void SetupCursor()
    {
        StreamResourceInfo si = System.Windows.Application.GetResourceStream(
            new Uri("/Resources/Cursors/dot_wt.cur", UriKind.Relative));

        PointCursorWhite = new Cursor(si.Stream);
        si.Stream.Close();


        si = System.Windows.Application.GetResourceStream(
            new Uri("/Resources/Cursors/dot_bk.cur", UriKind.Relative));

        PointCursorBlack = new Cursor(si.Stream);
        si.Stream.Close();

        ChangeMouseCursor(UITypes.MouseCursorType.CROSS);
    }

    private void SetPointCursor()
    {
        if (SettingsHolder.Settings.DrawMode == DrawModes.DARK)
        {
            base.Cursor = PointCursorWhite;
        }
        else
        {
            base.Cursor = PointCursorBlack;
        }
    }


    private void OnMouseUp(object sender, MouseEventArgs e)
    {
#if MOUSE_THREAD
        int what = MyEventHandler.MOUSE_UP;

        mEventSequencer.RemoveAll(what);

        MyEvent evt = mEventSequencer.ObtainEvent();

        evt.What = what;
        evt.EventArgs = e;

        mEventSequencer.Post(evt);
#else
        HandleMouseUp(e);
#endif
    }

    private void OnMouseDown(object sender, MouseEventArgs e)
    {
#if MOUSE_THREAD
        int what = MyEventHandler.MOUSE_DOWN;

        mEventSequencer.RemoveAll(what);

        MyEvent evt = mEventSequencer.ObtainEvent();

        evt.What = what;
        evt.EventArgs = e;

        mEventSequencer.Post(evt);
#else
        HandleMouseDown(e);
#endif
    }

    private void OnMouseWheel(object sender, MouseEventArgs e)
    {
#if MOUSE_THREAD
        int what = MyEventHandler.MOUSE_WHEEL;

        mEventSequencer.RemoveAll(what);

        MyEvent evt = mEventSequencer.ObtainEvent();

        evt.What = what;
        evt.EventArgs = e;

        mEventSequencer.Post(evt);
#else
        HandleMouseWheel(e);
#endif
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
#if MOUSE_THREAD
        int what = MyEventHandler.MOUSE_MOVE;

        mEventSequencer.RemoveAll(what);

        MyEvent evt = mEventSequencer.ObtainEvent();

        evt.What = what;
        evt.EventArgs = e;

        mEventSequencer.Post(evt);
#else
        HandleMouseMove(e);
#endif
    }

    private void Redraw()
    {
        //#if MOUSE_THREAD
        //        ThreadUtil.RunOnMainThread(mController.Redraw, false);
        //#else
        //        mController.Redraw(mController.DC);
        //#endif

        mController.Drawer.Redraw(mController.DC);
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        if (mController != null)
        {
            Redraw();
        }
    }

    private int sizeChangeCnt = 0;

    private void OnResize(object sender, EventArgs e)
    {
        if (mDrawContext == null)
        {
            return;
        }

        if (sizeChangeCnt == 1)
        {
            vector3_t org = default;
            org.X = Width / 2;
            org.Y = Height / 2;

            mDrawContext.SetViewOrg(org);

            mController.Input.SetCursorWoldPos(vector3_t.Zero);
        }

        sizeChangeCnt++;

        mDrawContext.SetViewSize(Size.Width, Size.Height);

        if (mController != null)
        {
            Redraw();
        }
    }

    public void EnablePerse(bool enable)
    {
        if (enable)
        {
            if (mDrawContext != mDrawContextPers)
            {
                mDrawContext = mDrawContextPers;
            }
        }
        else
        {
            if (mDrawContext != mDrawContextOrtho)
            {
                mDrawContext = mDrawContextOrtho;
            }
        }

        if (mDrawContext == null)
        {
            return;
        }

        mDrawContext.SetViewSize(Size.Width, Size.Height);
    }

    public void SwapBuffers(DrawContext dc)
    {
        if (dc == mDrawContext)
        {
            SwapBuffers();
        }
    }

    public void CursorLocked(bool locked)
    {
        if (locked)
        {
            ChangeMouseCursor(UITypes.MouseCursorType.NORMAL_ARROW);
        }
        else
        {
            ChangeMouseCursor(UITypes.MouseCursorType.CROSS);
        }
    }

    public void ChangeMouseCursor(UITypes.MouseCursorType cursorType)
    {
        switch (cursorType)
        {
            case UITypes.MouseCursorType.CROSS:
                SetPointCursor();
                break;
            case UITypes.MouseCursorType.NORMAL_ARROW:
                base.Cursor = Cursors.Arrow;
                break;
            case UITypes.MouseCursorType.HAND:
                base.Cursor = Cursors.SizeAll;
                break;
        }
    }

    private void SetupContextMenu()
    {
        mContextMenu = new ContextMenuEx();

        mContextMenu.StateChanged = (s) =>
        {
            if (s == ContextMenuEx.State.OPENED)
            {
                ChangeMouseCursor(UITypes.MouseCursorType.NORMAL_ARROW);
            }
            else if (s == ContextMenuEx.State.CLOSED)
            {
                ChangeMouseCursor(UITypes.MouseCursorType.CROSS);
            }
        };
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

    public void ShowContextMenu(MenuInfo menuInfo, int x, int y)
    {
        ThreadUtil.RunOnMainThread(() =>
        {
            ShowContextMenuProc(menuInfo, x, y);
        }, true);
    }

    private void ShowContextMenuProc(MenuInfo menuInfo, int x, int y)
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

    public void SetWorldScale(vcompo_t scale)
    {
        mDrawContextPers.WorldScale = scale;
        mDrawContextOrtho.WorldScale = scale;
    }

    private void HandleMouseUp(MouseEventArgs e)
    {
        DownButton = MouseButtons.None;
        mController.Input.Mouse.MouseUp(mDrawContext, e.Button, e.X, e.Y);

        Redraw();
    }

    private void HandleMouseDown(MouseEventArgs e)
    {
        if (mCurrentContextMenu != null)
        {
            if (mCurrentContextMenu.Visible)
            {
                mCurrentContextMenu.Close();
                return;
            }
        }

        if (mDrawContext is DrawContextGLOrtho)
        {
            mController.Input.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);
        }
        else
        {
            VectorExt.Set(out PrevMousePos, e.X, e.Y, 0);
            DownButton = e.Button;

            //if (DownButton != MouseButtons.Middle)
            {
                mController.Input.Mouse.MouseDown(mDrawContext, e.Button, e.X, e.Y);
            }
        }

        Redraw();
    }

    private void HandleMouseWheel(MouseEventArgs e)
    {
        if (mDrawContext is DrawContextGLOrtho)
        {
            mController.Input.Mouse.MouseWheel(mDrawContext, e.X, e.Y, e.Delta);
            Redraw();
        }
        else
        {
            DrawContextGLPers dc = mDrawContext as DrawContextGLPers;

            if (CadKeyboard.IsCtrlKeyDown())
            {
                if (e.Delta > 0)
                {
                    dc.MoveForwardEyePoint(3);
                }
                else if (e.Delta < 0)
                {
                    dc.MoveForwardEyePoint(-3);
                }

                Redraw();
            }
        }
    }

    private void HandleMouseMove(MouseEventArgs e)
    {
        if (mDrawContext is DrawContextGLOrtho)
        {
            mController.Input.Mouse.MouseMove(mDrawContext, e.X, e.Y);
            Redraw();
        }
        else
        {
            DrawContextGLPers dc = mDrawContext as DrawContextGLPers;

            if (DownButton == MouseButtons.Middle)
            {
                vector3_t t = new vector3_t(e.X, e.Y, 0);

                Vector2 prev = default;

                prev.X = (float)PrevMousePos.X;
                prev.Y = (float)PrevMousePos.Y;

                Vector2 current = default;

                current.X = (float)t.X;
                current.Y = (float)t.Y;

                if (CadKeyboard.IsCtrlKeyDown())
                {
                    //MoveCamera(DC, prev, current);
                    PanCamera(dc, prev, current);
                }
                else
                {
                    dc.RotateEyePoint(prev, current);
                }

                Redraw();

                PrevMousePos = t;
            }
            else
            {
                mController.Input.Mouse.MouseMove(mDrawContext, e.X, e.Y);
                Redraw();
            }
        }
    }

    private void MoveCamera(DrawContext dc, Vector2 prev, Vector2 current)
    {
        vector3_t pv = new vector3_t(prev.X, prev.Y, 0);
        vector3_t cv = new vector3_t(current.X, current.Y, 0);

        vector3_t dv = cv - pv;

        dv.X *= (vcompo_t)(-1.0);

        dc.WorldVectorToDevVector(dv);

        vector3_t lookAt = dc.LookAt + dv;
        vector3_t eye = dc.Eye + dv;

        dc.SetCamera(eye, lookAt, dc.UpVector);
    }

    public void PanCamera(DrawContext dc, Vector2 prev, Vector2 current)
    {
        Vector2 d = current - prev;

        vcompo_t rx = d.X * (vcompo_t)(Math.PI / 1000);
        vcompo_t ry = d.Y * (vcompo_t)(Math.PI / 1000);

        CadQuaternion q;
        CadQuaternion r;
        CadQuaternion qp;

        vector3_t lookv = dc.LookAt - dc.Eye;
        vector3_t upv = dc.UpVector;

        q = CadQuaternion.RotateQuaternion(upv, rx);
        r = q.Conjugate();

        qp = CadQuaternion.FromVector(lookv);
        qp = r * qp;
        qp = qp * q;
        lookv = qp.ToVector3();

        vector3_t ev = dc.LookAt - dc.Eye;

        vector3_t a = new vector3_t(ev);
        vector3_t b = new vector3_t(upv);

        vector3_t axis = CadMath.Normal(a, b);

        if (!axis.IsZero())
        {
            q = CadQuaternion.RotateQuaternion(axis, ry);
            r = q.Conjugate();

            qp = CadQuaternion.FromVector(lookv);
            qp = r * qp;
            qp = qp * q;

            lookv = qp.ToVector3();

            qp = CadQuaternion.FromVector(upv);
            qp = r * qp;
            qp = qp * q;
            upv = qp.ToVector3();
        }

        dc.SetCamera(dc.Eye, lookv + dc.Eye, upv);
    }


    public void DrawModeChanged(DrawModes mode)
    {
        if (mDrawContextOrtho != null)
        {
            mDrawContextOrtho.SetupTools(mode);
        }

        if (mDrawContextPers != null)
        {
            mDrawContextPers.SetupTools(mode);
        }

        ChangeMouseCursor(UITypes.MouseCursorType.CROSS);
    }

    public void GLMakeCurrent()
    {
        base.MakeCurrent();
    }

    class MyEvent : EventHandlerEvent
    {
        public MouseEventArgs EventArgs;
        public MyEventHandler Sequencer;
        public Action<MyEventHandler, MyEvent> Action;

        public void ExecAction()
        {
            Action(Sequencer, this);
        }
    }

    class MyEventHandler : TCad.Util.EventHandler<MyEvent>
    {
        public const int MOUSE_MOVE = 1;
        public const int MOUSE_WHEEL = 2;
        public const int MOUSE_DOWN = 3;
        public const int MOUSE_UP = 4;

        private PlotterViewGL mPlotterView;

        public MyEventHandler(PlotterViewGL view, int queueSize) : base(queueSize)
        {
            mPlotterView = view;
        }

        public override void HandleEvent(MyEvent msg)
        {
            msg.Sequencer = this;
            msg.Action = MsgExec;
            ThreadUtil.RunOnMainThread(msg.ExecAction, false);
        }

        private static void MsgExec(MyEventHandler this_, MyEvent msg)
        {
            if (msg.What == MOUSE_MOVE)
            {
                this_.mPlotterView.HandleMouseMove(msg.EventArgs);
            }
            else if (msg.What == MOUSE_WHEEL)
            {
                this_.mPlotterView.HandleMouseWheel(msg.EventArgs);
            }
            else if (msg.What == MOUSE_DOWN)
            {
                this_.mPlotterView.HandleMouseDown(msg.EventArgs);
            }
            else if (msg.What == MOUSE_UP)
            {
                this_.mPlotterView.HandleMouseUp(msg.EventArgs);
            }
        }
    }
}
