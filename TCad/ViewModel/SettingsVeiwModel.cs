using System.Reflection;
using System.ComponentModel;
using OpenTK;
using OpenTK.Mathematics;
using Plotter.Settings;
using Plotter;

namespace TCad.ViewModel;

public class UserSettingDataAttribute : System.Attribute
{
}

public class SettingsVeiwModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public IPlotterViewModel mContext;

    [UserSettingData]
    public bool ContinueCreateFigure
    {
        set
        {
            if (SettingsHolder.Settings.ContinueCreateFigure != value)
            {
                SettingsHolder.Settings.ContinueCreateFigure = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));

                if (!value)
                {
                    mContext.Controller.EndCreateFigure();
                }
            }
        }

        get => SettingsHolder.Settings.ContinueCreateFigure;
    }

    [UserSettingData]
    public bool SnapToGrid
    {
        set
        {
            if (SettingsHolder.Settings.SnapToGrid != value)
            {
                SettingsHolder.Settings.SnapToGrid = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToGrid)));

                Redraw();
            }
        }

        get => SettingsHolder.Settings.SnapToGrid;
    }

    [UserSettingData]
    public bool SnapToPoint
    {
        set
        {
            SettingsHolder.Settings.SnapToPoint = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToPoint)));
        }

        get => SettingsHolder.Settings.SnapToPoint;
    }

    [UserSettingData]
    public bool SnapToSegment
    {
        set
        {
            SettingsHolder.Settings.SnapToSegment = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSegment)));
        }

        get => SettingsHolder.Settings.SnapToSegment;
    }

    [UserSettingData]
    public bool SnapToLine
    {
        set
        {
            SettingsHolder.Settings.SnapToLine = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLine)));
        }

        get => SettingsHolder.Settings.SnapToLine;
    }

    [UserSettingData]
    public bool FilterObjectTree
    {
        set
        {
            SettingsHolder.Settings.FilterObjectTree = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilterObjectTree)));

            if (mContext.Controller != null)
            {
                mContext.Controller.UpdateObjectTree(true);
            }
        }

        get => SettingsHolder.Settings.FilterObjectTree;
    }

    [UserSettingData]
    public bool DrawMeshEdge
    {
        set
        {
            if (SettingsHolder.Settings.DrawMeshEdge != value && value == false)
            {
                if (FillMesh == false)
                {
                    FillMesh = true;
                }
            }

            SettingsHolder.Settings.DrawMeshEdge = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawMeshEdge)));

            Redraw();
        }

        get => SettingsHolder.Settings.DrawMeshEdge;
    }

    [UserSettingData]
    public bool FillMesh
    {
        set
        {
            if (SettingsHolder.Settings.FillMesh != value && value == false)
            {
                if (DrawMeshEdge == false)
                {
                    DrawMeshEdge = true;
                }
            }

            SettingsHolder.Settings.FillMesh = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FillMesh)));

            Redraw();
        }

        get => SettingsHolder.Settings.FillMesh;
    }

    [UserSettingData]
    public bool DrawNormal
    {
        set
        {
            SettingsHolder.Settings.DrawNormal = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawNormal)));

            Redraw();
        }

        get => SettingsHolder.Settings.DrawNormal;
    }

    [UserSettingData]
    public bool DrawAxis
    {
        set
        {
            SettingsHolder.Settings.DrawAxis = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawAxis)));

            Redraw();
        }

        get => SettingsHolder.Settings.DrawAxis;
    }

    [UserSettingData]
    public bool DrawAxisLabel
    {
        set
        {
            SettingsHolder.Settings.DrawAxisLabel = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawAxisLabel)));

            Redraw();
        }

        get => SettingsHolder.Settings.DrawAxisLabel;
    }

    [UserSettingData]
    public bool DrawCompass
    {
        set
        {
            SettingsHolder.Settings.DrawCompass = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawCompass)));

            Redraw();
        }

        get => SettingsHolder.Settings.DrawCompass;
    }

    [UserSettingData]
    public double InitialMoveLimit
    {
        set
        {
            SettingsHolder.Settings.InitialMoveLimit = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InitialMoveLimit)));
        }

        get => SettingsHolder.Settings.InitialMoveLimit;
    }

    [UserSettingData]
    public bool SnapToZero
    {
        set
        {
            SettingsHolder.Settings.SnapToZero = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToZero)));
        }

        get => SettingsHolder.Settings.SnapToZero;
    }

    [UserSettingData]
    public bool SnapToLastDownPoint
    {
        set
        {
            SettingsHolder.Settings.SnapToLastDownPoint = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToLastDownPoint)));
        }

        get => SettingsHolder.Settings.SnapToLastDownPoint;
    }

    [UserSettingData]
    public bool SnapToSelfPoint
    {
        set
        {
            SettingsHolder.Settings.SnapToSelfPoint = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SnapToSelfPoint)));
        }

        get => SettingsHolder.Settings.SnapToSelfPoint;
    }

    [UserSettingData]
    public Vector3d GridSize
    {
        set
        {
            SettingsHolder.Settings.GridSize = value;
            mContext.Controller.Grid.GridSize = value;
        }

        get => SettingsHolder.Settings.GridSize;
    }

    [UserSettingData]
    public double PointSnapRange
    {
        set
        {
            SettingsHolder.Settings.PointSnapRange = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PointSnapRange)));
        }

        get => SettingsHolder.Settings.PointSnapRange;
    }

    [UserSettingData]
    public double LineSnapRange
    {
        set
        {
            SettingsHolder.Settings.LineSnapRange = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LineSnapRange)));
        }

        get => SettingsHolder.Settings.LineSnapRange;
    }

    [UserSettingData]
    public double MoveKeyUnitX
    {
        set
        {
            SettingsHolder.Settings.MoveKeyUnitX = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveKeyUnitX)));
        }

        get => SettingsHolder.Settings.MoveKeyUnitX;
    }

    [UserSettingData]
    public double MoveKeyUnitY
    {
        set
        {
            SettingsHolder.Settings.MoveKeyUnitY = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MoveKeyUnitY)));
        }

        get => SettingsHolder.Settings.MoveKeyUnitY;
    }

    [UserSettingData]
    public DrawTools.DrawMode DrawMode
    {
        set
        {
            SettingsHolder.Settings.DrawMode = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DrawMode)));

            UpdateDrawMode(value);
            Redraw();
        }

        get => SettingsHolder.Settings.DrawMode;
    }

    [UserSettingData]
    public bool PrintWithBitmap
    {
        set
        {
            SettingsHolder.Settings.PrintWithBitmap = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrintWithBitmap)));
        }

        get => SettingsHolder.Settings.PrintWithBitmap;
    }

    [UserSettingData]
    public double MagnificationBitmapPrinting
    {
        set
        {
            SettingsHolder.Settings.MagnificationBitmapPrinting = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MagnificationBitmapPrinting)));
        }

        get => SettingsHolder.Settings.MagnificationBitmapPrinting;
    }

    [UserSettingData]
    public bool PrintLineSmooth
    {
        set
        {
            SettingsHolder.Settings.PrintLineSmooth = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PrintLineSmooth)));
        }

        get => SettingsHolder.Settings.PrintLineSmooth;
    }

    public SettingsVeiwModel(IPlotterViewModel context)
    {
        mContext = context;
    }

    private void Redraw()
    {
        mContext.Controller.Redraw();
    }

    private void UpdateDrawMode(DrawTools.DrawMode mode)
    {
        mContext.DrawModeUpdated(mode);
    }

    public void Load()
    {
        SettingsHolder.Settings.Load();

        MemberInfo[] memberes = typeof(SettingsVeiwModel).GetMembers();

        foreach (MemberInfo member in memberes)
        {
            if (member.MemberType == MemberTypes.Property)
            {
                UserSettingDataAttribute userSetting =
                    (UserSettingDataAttribute)member.GetCustomAttribute(typeof(UserSettingDataAttribute));

                if (userSetting != null)
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(member.Name));
                }
            }
        }

        mContext.Controller.Grid.GridSize = SettingsHolder.Settings.GridSize;
    }

    public void Save()
    {
        PlotterSettings settings = SettingsHolder.Settings;

        settings.GridSize = mContext.Controller.Grid.GridSize;

        settings.Save();
    }
}
