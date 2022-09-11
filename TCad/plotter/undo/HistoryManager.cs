using Plotter.Controller;
using System.Collections.Generic;

namespace Plotter
{
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
            DOut.pl(this.GetType().Name + " " + ope.GetType().Name);

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

            DOut.pl(this.GetType().Name + " " + "Undo ope:" + ope.GetType().Name);

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

            DOut.pl(this.GetType().Name + " " + "Redo ope:" + ope.GetType().Name);

            ope.Redo(mPC);
            mUndoStack.Push(ope);
        }

        public void dump()
        {
            DOut.pl(GetType().Name);
            DOut.pl("{");
            DOut.Indent++;
            DOut.pl("UndoStack [");
            DOut.Indent++;



            DOut.Indent--;
            DOut.Indent--;
            DOut.pl("}");
        }
    }
}
