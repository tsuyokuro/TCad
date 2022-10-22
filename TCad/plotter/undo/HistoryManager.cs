using Plotter.Controller;
using System.Collections.Generic;

namespace Plotter;

public class HistoryManager
{
    private PlotterController mPC;

    public Stack<CadOpe> mUndoStack = new Stack<CadOpe>();
    public Stack<CadOpe> mRedoStack = new Stack<CadOpe>();

    public HistoryManager(PlotterController pc)
    {
        mPC = pc;
    }

    public void Clear()
    {
        mUndoStack.Clear();
        mRedoStack.Clear();
    }

    public void foward(CadOpe ope)
    {
        DOut.plx(ope.GetType().Name);
        if (ope is null)
        {
            return;
        }

        mUndoStack.Push(ope);

        DisposeStackItems(mRedoStack);

        mRedoStack.Clear();
    }

    private void DisposeStackItems(Stack<CadOpe> stack)
    {
        foreach (CadOpe ope in stack)
        {
            ope.Dispose(mPC);
        }
    }

    public bool canUndo()
    {
        return mUndoStack.Count > 0;
    }

    public bool canRedo()
    {
        return mRedoStack.Count > 0;
    }

    public void undo()
    {
        if (mUndoStack.Count == 0) return;

        CadOpe ope = mUndoStack.Pop();

        if (ope == null)
        {
            return;
        }

        DOut.plx("Undo ope:" + ope.GetType().Name);

        ope.Undo(mPC);

        mRedoStack.Push(ope);
    }

    public void redo()
    {
        if (mRedoStack.Count == 0) return;

        CadOpe ope = mRedoStack.Pop();

        if (ope == null)
        {
            return;
        }

        DOut.plx("Redo ope:" + ope.GetType().Name);

        ope.Redo(mPC);
        mUndoStack.Push(ope);
    }

    public void dumpUndoStack()
    {
        DOut.plx("UndoStack");
        DOut.pl("{");
        DOut.Indent++;
        foreach (CadOpe ope in mUndoStack)
        {
            dumpCadOpe(ope);
        }
        DOut.Indent--;
        DOut.pl("}");
    }

    public static void dumpCadOpe(CadOpe ope)
    {
        DOut.pl(ope.GetType().Name);

        if (ope is CadOpeList)
        {
            DOut.pl("{");
            DOut.Indent++;
            foreach (CadOpe item in ((CadOpeList)ope).OpeList) {
                dumpCadOpe(item);
            }
            DOut.Indent--;
            DOut.pl("}");
        }
        else if (ope is CadOpeFigureSnapShotList)
        {
            DOut.pl("{");
            DOut.Indent++;
            foreach (CadOpe item in ((CadOpeFigureSnapShotList)ope).SnapShotList)
            {
                dumpCadOpe(item);
            }
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
