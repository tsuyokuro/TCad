using Plotter;
using System.ComponentModel;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

namespace TCad.ViewModel;

public class LayerHolder : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private CadLayer mLayer;

    public uint ID
    {
        get => mLayer.ID;
    }

    public bool Locked
    {
        set
        {
            mLayer.Locked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Locked)));
        }

        get => mLayer.Locked;
    }

    public bool Visible
    {
        set
        {
            mLayer.Visible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Visible)));
        }

        get => mLayer.Visible;
    }

    public string Name
    {
        set
        {
            mLayer.Name = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }

        get => mLayer.Name;
    }

    public LayerHolder(CadLayer layer)
    {
        mLayer = layer;
    }
}
