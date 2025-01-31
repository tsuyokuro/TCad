using OpenTK.Mathematics;
using System.ComponentModel;
using System.Text;

namespace TCad.ViewModel;

public class CursorPosViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private StringBuilder sb1 = new StringBuilder();
    private StringBuilder sb2 = new StringBuilder();
    private StringBuilder sb3 = new StringBuilder();


    private string mStrCursorPos = "";

    public string StrCursorPos
    {
        set
        {
            mStrCursorPos = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrCursorPos)));
        }

        get => mStrCursorPos;
    }

    private string mStrCursorPos2 = "";

    public string StrCursorPos2
    {
        set
        {
            mStrCursorPos2 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrCursorPos2)));
        }

        get => mStrCursorPos2;
    }

    private string mStrCursorPos3 = "";

    public string StrCursorPos3
    {
        set
        {
            mStrCursorPos3 = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StrCursorPos3)));
        }

        get => mStrCursorPos3;
    }

    private vector3_t mCursorPos;

    public vector3_t CursorPos
    {
        set
        {
            if (!string.IsNullOrEmpty(mStrCursorPos) && mCursorPos.Equals(value))
            {
                return;
            }

            mCursorPos = value;

            sb1.Clear();
            sb1.AppendFormat("({0:0.00}, {1:0.00}, {2:0.00})",
                mCursorPos.X, mCursorPos.Y, mCursorPos.Z);

            StrCursorPos = sb1.ToString();
        }

        get => mCursorPos;
    }

    private vector3_t mCursorPos2;

    public vector3_t CursorPos2
    {
        set
        {
            if (!string.IsNullOrEmpty(mStrCursorPos2) && mCursorPos2.Equals(value))
            {
                return;
            }

            mCursorPos2 = value;

            sb2.Clear();
            sb2.AppendFormat("({0:0.00}, {1:0.00}, {2:0.00})",
                mCursorPos2.X, mCursorPos2.Y, mCursorPos2.Z);


            StrCursorPos2 = sb2.ToString();
        }

        get => mCursorPos2;
    }

    private vector3_t mCursorPos3;

    public vector3_t CursorPos3
    {
        set
        {
            if (!string.IsNullOrEmpty(mStrCursorPos3) && mCursorPos3.Equals(value))
            {
                return;
            }

            mCursorPos3 = value;

            sb3.Clear();
            sb3.AppendFormat("({0:0.00}, {1:0.00}, {2:0.00})",
                mCursorPos3.X, mCursorPos3.Y, mCursorPos3.Z);

            StrCursorPos3 = sb3.ToString();
        }

        get => mCursorPos3;
    }

}
