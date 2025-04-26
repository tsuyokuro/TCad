using TCad.Plotter;
using Plotter.Controller;
using System.Collections.Generic;

namespace TCad.Plotter.undo;

public class HistoryManager
{
    private IPlotterController mPC;

    public Stack<CadOpe> mUndoStack = new Stack<CadOpe>();
    public Stack<CadOpe> mRedoStack = new Stack<CadOpe>();

    public HistoryManager(IPlotterController pc)
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
        Log.plx(ope.GetType().Name);
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

        Log.plx("Undo ope:" + ope.GetType().Name);

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

        Log.plx("Redo ope:" + ope.GetType().Name);

        ope.Redo(mPC);
        mUndoStack.Push(ope);
    }

    public void dumpUndoStack()
    {
        Log.plx("UndoStack");
        Log.pl("{");
        Log.Indent++;
        foreach (CadOpe ope in mUndoStack)
        {
            dumpCadOpe(ope);
        }
        Log.Indent--;
        Log.pl("}");
    }

    public static void dumpCadOpe(CadOpe ope)
    {
        Log.pl(ope.GetType().Name);

        if (ope is CadOpeList)
        {
            Log.pl("{");
            Log.Indent++;
            foreach (CadOpe item in ((CadOpeList)ope).OpeList)
            {
                dumpCadOpe(item);
            }
            Log.Indent--;
            Log.pl("}");
        }
        else if (ope is CadOpeFigureSnapShotList)
        {
            Log.pl("{");
            Log.Indent++;
            foreach (CadOpe item in ((CadOpeFigureSnapShotList)ope).SnapShotList)
            {
                dumpCadOpe(item);
            }
            Log.Indent--;
            Log.pl("}");
        }
    }
}
