using System.Collections.Generic;

namespace Plotter
{
    using System;
    using System.Linq;

    public class CadLayer
    {
        public uint ID;

        private String mName = null;
        public String Name
        {
            get
            {
                if (mName != null)
                {
                    return mName;
                }

                return "layer" + ID;
            }

            set => mName = value;
        }

        private bool mLocked = false;
        public bool Locked
        {
            set
            {
                mLocked = value;
                mFigureList.ForEach(a => a.Locked = value);
            }

            get => mLocked;
        }

        public bool Visible = true;

        private List<CadFigure> mFigureList = new List<CadFigure>();
        public List<CadFigure> FigureList
        {
            get => mFigureList;
            set => mFigureList = value;
        }

        public CadLayer()
        {
        }

        public void AddFigure(CadFigure fig)
        {
            fig.LayerID = ID;
            mFigureList.Add(fig);
        }

        public CadFigure GetFigureByID(uint id)
        {
            return mFigureList.Find(fig => fig.ID == id);
        }

        public void InsertFigure(int index, CadFigure fig)
        {
            fig.LayerID = ID;
            mFigureList.Insert(index, fig);
        }

        public void RemoveFigureByID(CadObjectDB db, uint id)
        {
            CadFigure fig = db.GetFigure(id);
            mFigureList.Remove(fig);
            fig.LayerID = 0;
        }

        public void RemoveFigureByID(uint id)
        {
            int index = GetFigureIndex(id);

            if (index < 0)
            {
                return;
            }

            mFigureList[index].LayerID = 0;

            mFigureList.RemoveAt(index);
        }


        public void RemoveFigureByIndex(int index)
        {
            mFigureList[index].LayerID = 0;
            mFigureList.RemoveAt(index);
        }

        public int GetFigureIndex(uint figID)
        {
            int index = 0;
            foreach (CadFigure fig in mFigureList)
            {
                if (fig.ID == figID)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public void ClearSelectedFlags()
        {
            ForEachFig(fig =>
            {
                fig.ClearSelectFlags();
            });
        }

        public CadOpeList Clear()
        {
            CadOpeList opeList = new CadOpeList();

            CadOpe ope;

            IEnumerable<CadFigure> figList = mFigureList;
            var revFig = figList.Reverse();

            foreach (CadFigure fig in revFig)
            {
                ope = new CadOpeRemoveFigure(this, fig.ID);
                opeList.OpeList.Add(ope);
            }

            mFigureList.Clear();

            return opeList;
        }

        /// <summary>
        /// 全てのFigureを列挙(中止不可版)
        /// Figureが子を持つ場合もフラットに列挙される
        /// </summary>
        /// <param name="d"></param>
        public void ForEachFig(Action<CadFigure> d)
        {
            int i;
            for (i = 0; i < mFigureList.Count; i++)
            {
                CadFigure fig = mFigureList[i];
                fig.ForEachFig(d);
            }
        }

        public void ForEachRootFig(Action<CadFigure> d)
        {
            int i;
            for (i = 0; i < mFigureList.Count; i++)
            {
                CadFigure fig = mFigureList[i];
                d(fig);
            }
        }

        public void ForEachFigRev(Action<CadFigure> d)
        {
            int i = mFigureList.Count - 1;
            for (; i>=0; i--)
            {
                CadFigure fig = mFigureList[i];
                fig.ForEachFig(d);
            }
        }

        public void sdump()
        {
            DOut.pl(
                this.GetType().Name + 
                "(" + this.GetHashCode().ToString() + ")" +
                "ID=" + ID.ToString());
        }

        public void dump()
        {
            DOut.pl(this.GetType().Name + "(" + this.GetHashCode().ToString() + ") {");
            DOut.Indent++;
            DOut.pl("ID=" + ID.ToString());

            foreach (CadFigure fig in FigureList)
            {
                DOut.pl("FigID=" + fig.ID);
            }

            DOut.Indent--;
            DOut.pl("}");
        }
    }
}