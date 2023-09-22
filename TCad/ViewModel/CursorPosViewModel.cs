//#define DEFAULT_DATA_TYPE_DOUBLE
using OpenTK.Mathematics;
using System.ComponentModel;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace TCad.ViewModel;

public class CursorPosViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

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

            string s = string.Format("({0:0.00}, {1:0.00}, {2:0.00})",
                mCursorPos.X, mCursorPos.Y, mCursorPos.Z);

            StrCursorPos = s;
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

            string s = string.Format("({0:0.00}, {1:0.00}, {2:0.00})",
                mCursorPos2.X, mCursorPos2.Y, mCursorPos2.Z);

            StrCursorPos2 = s;
        }

        get => mCursorPos2;
    }
}
