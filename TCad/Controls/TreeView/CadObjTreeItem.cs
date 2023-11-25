using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace TCad.Controls;


public enum CadObjTreeItemType
{
    NODE,
    LEAF,
}


public abstract class CadObjTreeItem
{
    public class ContextMenuTag
    {
        public string Tag;
        public CadObjTreeItem TreeItem;
    }

    public bool IsExpand
    {
        get; set;
    }

    public virtual CadObjTreeItem Parent
    {
        get; set;
    }

    public virtual bool IsChecked
    {
        get
        {
            return false;
        }

        set
        {
        }
    }

    CadObjTreeItemType ItemType = CadObjTreeItemType.NODE;
    public virtual CadObjTreeItemType Type
    {
        get => ItemType;
        set => ItemType = value;
    }


    public virtual string Text
    {
        get
        {
            return "----";
        }
    }

    protected List<CadObjTreeItem> mChildren;

    public int GetLevel()
    {
        int i = 0;

        CadObjTreeItem parent = Parent;


        while (parent != null)
        {
            i++;
            parent = parent.Parent;
        }

        return i;
    }


    public List<CadObjTreeItem> Children
    {
        get
        {
            return mChildren;
        }
    }

    public virtual void Add(CadObjTreeItem item)
    {
        if (mChildren == null)
        {
            mChildren = new List<CadObjTreeItem>();
        }

        item.Parent = this;

        mChildren.Add(item);
    }

    public virtual int GetTotalCount()
    {
        int cnt = 1;

        if (mChildren != null && IsExpand)
        {
            for (int i = 0; i < mChildren.Count; i++)
            {
                var item = mChildren[i];
                cnt += item.GetTotalCount();
            }
        }

        return cnt;
    }

    protected ContextMenuTag CreateContextMenuTag(string tagText)
    {
        var tag = new ContextMenuTag();
        tag.Tag = tagText;
        tag.TreeItem = this;
        return tag;
    }

    public virtual List<MenuItem> GetContextMenuItems()
    {
        return null;
    }

    public bool ForEach(Func<CadObjTreeItem, bool> func)
    {
        if (!func(this))
        {
            return false;
        }

        if (!IsExpand)
        {
            return true;
        }

        if (mChildren == null)
        {
            return true;
        }

        int i;
        for (i = 0; i < mChildren.Count; i++)
        {
            CadObjTreeItem item = mChildren[i];

            if (!item.ForEach(func))
            {
                return false;
            }
        }

        return true;
    }

    public void ForEachAll(Action<CadObjTreeItem> action)
    {
        action(this);

        if (mChildren == null)
        {
            return;
        }

        int i;
        for (i = 0; i < mChildren.Count; i++)
        {
            CadObjTreeItem item = mChildren[i];
            item.ForEachAll(action);
        }
    }

    public bool ForEach(Func<CadObjTreeItem, int, bool> func, int level)
    {
        if (!func(this, level))
        {
            return false;
        }

        if (!IsExpand)
        {
            return true;
        }

        if (mChildren == null)
        {
            return true;
        }

        int i;
        for (i = 0; i < mChildren.Count; i++)
        {
            CadObjTreeItem item = mChildren[i];

            if (!item.ForEach(func, level + 1))
            {
                return false;
            }
        }

        return true;
    }

    public CadObjTreeItem GetAt(int n)
    {
        int i = 0;

        CadObjTreeItem ret = null;

        ForEach(item =>
        {
            if (n == i)
            {
                ret = item;
                return false;
            }

            i++;
            return true;
        });

        return ret;
    }
}
