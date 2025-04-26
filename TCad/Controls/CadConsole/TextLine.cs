using System;
using System.Collections.Generic;
using System.Text;

namespace TCad.Controls.CadConsole;

public struct TextAttr
{
    public int FColor;
    public int BColor;
}

public struct AttrSpan
{
    public TextAttr Attr;
    public int Start;
    public int Len;

    public AttrSpan(TextAttr attr, int start, int len)
    {
        Attr = attr;
        Start = start;
        Len = len;
    }
}

public class TextLine
{
    public string Data = "";
    public List<AttrSpan> Attrs = new List<AttrSpan>();

    private AttrSpan LastAttrSpan
    {
        get
        {
            return Attrs[Attrs.Count - 1];
        }

        set
        {
            Attrs[Attrs.Count - 1] = value;
        }
    }

    public TextLine(TextAttr attr)
    {
        Attrs.Add(new AttrSpan(attr, 0, 0));
    }

    public void Clear()
    {
        AttrSpan lastAttrSpan = LastAttrSpan;
        Attrs.Clear();
        Attrs.Add(new AttrSpan(lastAttrSpan.Attr, 0, 0));
        Data = "";
    }

    private void AppendAttr(TextAttr attr)
    {
        AttrSpan lastSpan = LastAttrSpan;

        int sp = lastSpan.Start + lastSpan.Len;
        Attrs.Add(new AttrSpan(attr, sp, 0));
    }

    private void AddLastAttrSpanLen(int len)
    {
        AttrSpan attrItem = LastAttrSpan;
        attrItem.Len += len;
        LastAttrSpan = attrItem;
    }

    public void Parse(string str)
    {
        TextAttr attr = LastAttrSpan.Attr;

        StringBuilder builder = new StringBuilder(str.Length);

        int blen = 0;

        int state = 0;

        int x = 0;

        ReadOnlySpan<char> s = str;

        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '\x1b')
            {
                state = 1;

                AddLastAttrSpanLen(blen);

                blen = 0;
                continue;
            }

            switch (state)
            {
                case 0:
                    if (s[i] == '\r')
                    {
                        // Ignore CR

                        //Clear();
                        //blen = 0;
                        //builder.Clear();
                    }
                    else
                    {
                        builder.Append(s[i]);
                        blen++;
                    }
                    break;
                case 1:
                    if (s[i] == '[')
                    {
                        state = 2;
                    }
                    break;
                case 2:
                    if (s[i] >= '0' && s[i] <= '9')
                    {
                        state = 3;
                        x = s[i] - '0';
                    }
                    else if (s[i] == 'm')
                    {
                        if (x == 0)
                        {
                            attr.BColor = 0;
                            attr.FColor = 7;
                        }

                        AppendAttr(attr);

                        blen = 0;
                        state = 0;
                    }
                    else
                    {
                        builder.Append(s[i]);
                        blen++;
                        state = 0;
                    }
                    break;
                case 3:
                    if (s[i] >= '0' && s[i] <= '9')
                    {
                        x = x * 10 + (s[i] - '0');
                    }
                    else if (s[i] == 'm')
                    {
                        if (x == 0)
                        {
                            attr.BColor = 0;
                            attr.FColor = 7;
                        }
                        else if (x >= 30 && x <= 37) // front std
                        {
                            attr.FColor = (byte)(x - 30);
                        }
                        else if (x >= 40 && x <= 47) // back std
                        {
                            attr.BColor = (byte)(x - 40);
                        }
                        else if (x >= 90 && x <= 97) // front strong
                        {
                            attr.FColor = (byte)(x - 90 + 8);
                        }
                        else if (x >= 100 && x <= 107) // back std
                        {
                            attr.BColor = (byte)(x - 100 + 8);
                        }

                        AppendAttr(attr);
                        blen = 0;
                        state = 0;
                    }
                    else
                    {
                        builder.Append(s[i]);
                        blen++;
                        state = 0;
                    }

                    break;
            }
        }

        if (blen > 0)
        {
            AddLastAttrSpanLen(blen);
            blen = 0;
        }

        Data += builder.ToString();
    }
}
