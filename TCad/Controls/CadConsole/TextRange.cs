using System;
using System.Windows.Media.Media3D;
using Mono.Unix.Native;
using OpenTK.Compute.OpenCL;
using static IronPython.SQLite.PythonSQLite;

namespace TCad.Controls.CadConsole;

#pragma warning disable CS0660

public struct TextPos : IEquatable<TextPos>
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

    public static bool operator ==(TextPos left, TextPos right)
    {
        return left.Row == right.Row && left.Col == right.Col;
    }

    public static bool operator !=(TextPos left, TextPos right)
    {
        return !(left == right);
    }

    public bool Equals(TextPos other)
    {
        return Row == other.Row && Col == other.Col;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Row, Col);
    }
}

public struct TextSpan : IEquatable<TextSpan>
{
    public int Start;
    public int Len;

    public TextSpan(int start, int len)
    {
        Start = start;
        Len = len;
    }

    public bool Equals(TextSpan other)
    {
        return Start == other.Start && Len == other.Len;
    }
    public override int GetHashCode()
    {
        return HashCode.Combine(Start, Len);
    }

    public static bool operator ==(TextSpan left, TextSpan right)
    {
        return left.Start == right.Start && left.Len == right.Len;
    }

    public static bool operator !=(TextSpan left, TextSpan right)
    {
        return !(left == right);
    }
}

public struct TextRowRange : IEquatable<TextRowRange>
{
    public int SP;
    public int EP;

    public TextRowRange(int start, int end)
    {
        SP = start;
        EP = end;
    }

    public bool Equals(TextRowRange other)
    {
        return SP == other.SP && EP == other.EP;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SP, EP);
    }

    public static bool operator ==(TextRowRange left, TextRowRange right)
    {
        return left.SP == right.SP && left.EP == right.EP;
    }

    public static bool operator !=(TextRowRange left, TextRowRange right)
    {
        return !(left == right);
    }
}

public struct TextRange : IEquatable<TextRange>
{
    public readonly bool IsValid
    {
        get
        {
            if (SP.Row < 0 && EP.Row < 0) return false;
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
            (tr.EP, tr.SP) = (tr.SP, tr.EP);
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

    public static bool operator ==(TextRange left, TextRange right)
    {
        return left.SP == right.SP && left.EP == right.EP;
    }

    public static bool operator !=(TextRange left, TextRange right)
    {
        return !(left == right);
    }

    public bool Equals(TextRange other)
    {
        return SP == other.SP && EP == other.EP;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(SP.GetHashCode(), EP.GetHashCode());
    }
}
