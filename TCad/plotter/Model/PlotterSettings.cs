using OpenTK.Mathematics;
using Plotter.Controller;
using Plotter.Serializer;
using System.IO;
using System.Reflection;
using System.Text.Json;
using JObj = System.Text.Json.Nodes.JsonObject;
//using JObj = Newtonsoft.Json.Linq.JObject;

namespace Plotter.Settings;

public static class SettingsHolder
{
    public static PlotterSettings Settings = new PlotterSettings();
}

public class PlotterSettings
{
    public bool ContinueCreateFigure = false;

    public bool SnapToPoint = true;

    public bool SnapToSegment = true;

    public bool SnapToLine = true;

    public bool SnapToGrid = false;

    public vector3_t GridSize;

    public vcompo_t PointSnapRange = 6;

    public vcompo_t LineSnapRange = 8;

    public vcompo_t MoveKeyUnitX = (vcompo_t)(1.0);

    public vcompo_t MoveKeyUnitY = (vcompo_t)(1.0);

    public bool FilterObjectTree = false;

    public vcompo_t InitialMoveLimit = (vcompo_t)(6.0);

    public bool SnapToZero = true;

    public bool SnapToLastDownPoint = true;

    public bool SnapToSelfPoint = true;

    #region Draw settings
    public DrawModes DrawMode = DrawModes.DARK;

    public bool DrawMeshEdge = true;

    public bool DrawMeshBorder = true;

    public bool FillMesh = true;

    public bool DrawNormal = false;

    public bool DrawAxis = true;

    public bool DrawAxisLabel = false;

    public bool DrawCompass = true;
    #endregion

    public string LastDataDir = null;

    public string LastScriptDir = null;

    #region Print
    public bool PrintWithBitmap = true;
    public vcompo_t MagnificationBitmapPrinting = (vcompo_t)(0.962);
    public bool PrintLineSmooth = false;
    #endregion

    public PlotterSettings()
    {
        GridSize = new vector3_t(10, 10, 10);
    }

    private string FileName()
    {
        Assembly asm = Assembly.GetEntryAssembly();

        string exePath = asm.Location;

        string dir = Path.GetDirectoryName(exePath);

        string fileName = dir + @"\settings.json";

        return fileName;
    }

    public bool Save()
    {
        string fileName = FileName();

        JObj root = new JObj();

        JObj jo;

        root.Add("ContinueCreateFigure", ContinueCreateFigure);
        root.Add("LastDataDir", LastDataDir);
        root.Add("LastScriptDir", LastScriptDir);

        jo = new JObj();
        jo.Add("enable", SnapToPoint);
        jo.Add("range", PointSnapRange);
        root.Add("PointSnap", jo);

        jo = new JObj();
        jo.Add("enable", SnapToSegment);
        root.Add("SegmentSnap", jo);

        jo = new JObj();
        jo.Add("enable", SnapToLine);
        jo.Add("range", LineSnapRange);
        root.Add("LineSnap", jo);

        jo = new JObj();
        jo.Add("unit_x", MoveKeyUnitX);
        jo.Add("unit_y", MoveKeyUnitY);
        root.Add("MoveKey", jo);

        jo = new JObj();
        jo.Add("enable", SnapToZero);
        root.Add("ZeroSnap", jo);

        jo = new JObj();
        jo.Add("enable", SnapToLastDownPoint);
        root.Add("LastDownSnap", jo);

        jo = new JObj();
        jo.Add("enable", SnapToSelfPoint);
        root.Add("SelfPointSnap", jo);

        jo = new JObj();
        jo.Add("enable", SnapToGrid);
        jo.Add("size_x", GridSize.X);
        jo.Add("size_y", GridSize.Y);
        jo.Add("size_z", GridSize.Z);
        root.Add("GridInfo", jo);


        jo = new JObj();
        jo.Add("DrawModes", (int)DrawMode);
        jo.Add("DrawMeshEdge", DrawMeshEdge);
        jo.Add("DrawMeshBorder", DrawMeshBorder);
        jo.Add("FillMesh", FillMesh);
        jo.Add("DrawNormal", DrawNormal);
        jo.Add("DrawAxis", DrawAxis);
        jo.Add("DrawAxisLabel", DrawAxisLabel);
        jo.Add("DrawCompass", DrawCompass);
        root.Add("DrawSettings", jo);

        jo = new JObj();
        jo.Add("PrintWithBitmap", PrintWithBitmap);
        jo.Add("MagnificationBitmapPrinting", MagnificationBitmapPrinting);
        root.Add("PrintSettings", jo);

        StreamWriter writer = new StreamWriter(fileName);

        writer.Write(root.ToString());
        writer.Close();

        return true;
    }

    public bool Load()
    {
        string fileName = FileName();

        if (!File.Exists(fileName))
        {
            return true;
        }

        StreamReader reader = new StreamReader(fileName);

        var js = reader.ReadToEnd();

        reader.Close();

        JsonDocument jdoc = JsonDocument.Parse(js);

        JsonElement root = jdoc.RootElement;

        JsonElement jo;

        ContinueCreateFigure = root.GetBool("ContinueCreateFigure", ContinueCreateFigure);

        LastDataDir = root.GetString("LastDataDir", LastDataDir);
        LastScriptDir = root.GetString("LastScriptDir", LastScriptDir);

        if (root.TryGetProperty("PointSnap", out jo))
        {
            SnapToPoint = jo.GetBool("enable", SnapToPoint);
            PointSnapRange = jo.GetDouble("range", PointSnapRange);
        }

        if (root.TryGetProperty("SegmentSnap", out jo))
        {
            SnapToSegment = jo.GetBool("enable", SnapToSegment);
        }

        if (root.TryGetProperty("LineSnap", out jo))
        {
            SnapToLine = jo.GetBool("enable", SnapToLine);
            LineSnapRange = jo.GetDouble("range", LineSnapRange);
        }

        if (root.TryGetProperty("MoveKey", out jo))
        {
            MoveKeyUnitX = jo.GetDouble("unit_x", MoveKeyUnitX);
            MoveKeyUnitY = jo.GetDouble("unit_y", MoveKeyUnitY);
        }

        if (root.TryGetProperty("ZeroSnap", out jo))
        {
            SnapToZero = jo.GetBool("enable", SnapToZero);
        }

        if (root.TryGetProperty("LastDownSnap", out jo))
        {
            SnapToLastDownPoint = jo.GetBool("enable", SnapToLastDownPoint);
        }

        if (root.TryGetProperty("SelfPointSnap", out jo))
        {
            SnapToSelfPoint = jo.GetBool("enable", SnapToSelfPoint);
        }

        if (root.TryGetProperty("DrawSettings", out jo))
        {
            DrawMode = jo.GetEnum<DrawModes>("DrawModes", DrawMode);
            DrawMeshEdge = jo.GetBool("DrawMeshEdge", DrawMeshEdge);
            DrawMeshBorder = jo.GetBool("DrawMeshBorder", DrawMeshBorder);
            FillMesh = jo.GetBool("FillMesh", FillMesh);
            DrawNormal = jo.GetBool("DrawNormal", DrawNormal);
            DrawAxis = jo.GetBool("DrawAxis", DrawAxis);
            DrawAxisLabel = jo.GetBool("DrawAxisLabel", DrawAxisLabel);
            DrawCompass = jo.GetBool("DrawCompass", DrawCompass);
        }

        if (root.TryGetProperty("GridInfo", out jo))
        {
            SnapToGrid = jo.GetBool("enable", SnapToSelfPoint);
            GridSize.X = jo.GetDouble("size_x", 10);
            GridSize.Y = jo.GetDouble("size_y", 10);
            GridSize.Z = jo.GetDouble("size_z", 10);
        }

        if (root.TryGetProperty("PrintSettings", out jo))
        {
            PrintWithBitmap = jo.GetBool("PrintWithBitmap", PrintWithBitmap);
            MagnificationBitmapPrinting = jo.GetDouble("MagnificationBitmapPrinting", MagnificationBitmapPrinting);
        }

        return true;
    }
}
