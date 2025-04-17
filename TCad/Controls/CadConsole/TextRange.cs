namespace TCad.Controls;

public struct TextPos
{
    public int Row;
    public int Col;

    public TextPos(int row = -1, int col = -1)
    {
        Row = row;
        Col = col;
    }

    public static bool operator <(TextPos left, TextPos right)
    {
        if (left.Row != right.Row)
        {
            return left.Row < right.Row;
        }

        return left.Col < right.Col;
    }

    public static bool operator >(TextPos left, TextPos right)
    {
        if (left.Row != right.Row)
        {
            return left.Row > right.Row;
        }

        return left.Col > right.Col;
    }
}

public struct TextSpan
{
    public int Start;
    public int Len;

    public TextSpan(int start, int len)
    {
        Start = start;
        Len = len;
    }
}

public struct TextRange
{
    public bool IsValid
    {
        get
        {
            if (SP.Row < 0 && EP.Row < 0) return false;
            //if (SP.Row == EP.Row && SP.Col == EP.Col) return false;
            return true;
        }
    }

    public TextPos SP;
    public TextPos EP;

    public TextRange(TextPos sp, TextPos ep)
    {
        SP = sp;
        EP = ep;
    }

    public void Reset()
    {
        SP.Row = -1;
        EP.Row = -1;
    }

    public void Start(int row, int col)
    {
        SP.Row = row;
        SP.Col = col;

        EP = SP;
    }

    public void End(int row, int col)
    {
        EP.Row = row;
        EP.Col = col;
    }

    public bool IsEmpty()
    {
        return (SP.Row == EP.Row) && (SP.Col == EP.Col);
    }

    public static TextRange Naormalized(TextRange tr)
    {
        if (tr.EP < tr.SP)
        {
            TextPos t = tr.SP;
            tr.SP = tr.EP;
            tr.EP = t;
        }

        return tr;
    }

    public TextSpan GetRowSpan(int row, int maxLen)
    {
        TextSpan r = default;

        if (row == SP.Row && row == EP.Row)
        {
            r.Start = SP.Col;
            r.Len = EP.Col - SP.Col + 1;
        }
        else if (row > SP.Row && row == EP.Row)
        {
            r.Start = 0;
            r.Len = EP.Col + 1;
        }
        else if (row < EP.Row && row == SP.Row)
        {
            r.Start = SP.Col;
            r.Len = maxLen - r.Start;
        }
        else
        {
            r.Start = 0;
            r.Len = maxLen - r.Start;
        }

        if (r.Len < 0) r.Len = 0;

        return r;
    }
}
