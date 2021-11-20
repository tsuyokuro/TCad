using System.Collections.Generic;

namespace Plotter
{
    public class HistoryManager
    {
        private CadObjectDB mDB;

        public CadObjectDB DB
        {
            set
            {
                Clear();
                mDB = value;
            }
        }

        public Stack<CadOpe> mUndoStack = new Stack<CadOpe>();
        public Stack<CadOpe> mRedoStack = new Stack<CadOpe>();

        public HistoryManager(CadObjectDB db)
        {
            mDB = db;
        }

        public void Clear()
        {
            mUndoStack.Clear();
            mRedoStack.Clear();
        }

        public void foward(CadOpe ope)
        {
            mUndoStack.Push(ope);

            DisposeStackItems(mRedoStack);

            mRedoStack.Clear();
        }

        private void DisposeStackItems(Stack<CadOpe> stack)
        {
            foreach (CadOpe ope in stack)
            {
                ope.Dispose(mDB);
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

            ope.Undo(mDB);

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

            ope.Redo(mDB);
            mUndoStack.Push(ope);
        }

        public void dump()
        {
            DOut.pl(this.GetType().Name);
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
