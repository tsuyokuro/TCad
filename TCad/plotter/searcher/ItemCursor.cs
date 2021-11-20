using System.Collections.Generic;

namespace Plotter
{
    public class ItemCursor<T> where T : class
    {
        public List<T> ItemList;

        public int Pos = 0;

        public ItemCursor(List<T> list)
        {
            Attach(list);
        }

        public void Attach(List<T> list)
        {
            ItemList = list;
            Pos = 0;
        }

        public T Next()
        {
            if (Pos >= ItemList.Count)
            {
                return null;
            }

            T ret = ItemList[Pos];

            Pos++;

            return ret;
        }

        public T LoopNext()
        {
            if (ItemList.Count == 0)
            {
                return null;
            }

            T ret = ItemList[Pos];

            Pos++;

            Pos = Pos % ItemList.Count;

            return ret;
        }
    }
}
