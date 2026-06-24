namespace TCad.Controls.CadConsole;

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

public struct TextRowRange
{
    public int SP;
    public int EP;

    public TextRowRange(int start, int end)
    {
        SP = start;
        EP = end;
    }
}

public struct TextRange
{
    public readonly bool IsValid
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

    public readonly bool IsEmpty()
    {
        return SP.Row == EP.Row && SP.Col == EP.Col;
    }

    public static TextRange Normalized(TextRange tr)
    {
        if (tr.EP < tr.SP)
        {
            TextPos t = tr.SP;
            tr.SP = tr.EP;
            tr.EP = t;
        }

        return tr;
    }


    public readonly TextSpan GetRowSpan(int row, int maxLen = int.MaxValue)
    {
        TextSpan span = default;

        span.Start = 0;
        span.Len　= 0;
        bool inRange = (SP.Row <= row && EP.Row >= row);

        if (inRange)
        {
            if (row == SP.Row)
            {
                span.Start = SP.Col;
            }


            if (row == EP.Row)
            {
                span.Len　= EP.Col - span.Start + 1;
            }
            else
            {
                span.Len = maxLen - span.Start;
            }
        }

        return span;
    }

}
