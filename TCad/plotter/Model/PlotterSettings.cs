using Plotter.Serializer;
using System;
using System.IO;
using System.Reflection;
using OpenTK.Mathematics;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Plotter.Settings
{
    public static class SettingsHolder
    {
        public static PlotterSettings Settings = new PlotterSettings();
    }

    public class PlotterSettings
    {
        public bool ContinueCreateFigure = true;

        public bool SnapToPoint = true;

        public bool SnapToSegment = true;

        public bool SnapToLine = true;

        public bool SnapToGrid = false;

        public Vector3d GridSize;

        public double PointSnapRange = 6;

        public double LineSnapRange = 8;

        public double KeyMoveUnit = 1.0;

        public bool FilterObjectTree = false;

        public double InitialMoveLimit = 6.0;

        public bool SnapToZero = true;

        public bool SnapToLastDownPoint = true;

        public bool SnapToSelfPoint = true;

        #region Draw settings
        public DrawTools.DrawMode DrawMode = DrawTools.DrawMode.DARK;

        public bool DrawMeshEdge = true;

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
        public double MagnificationBitmapPrinting = 0.96;
        #endregion

        public PlotterSettings()
        {
            GridSize = new Vector3d(10, 10, 10);
        }

        private String FileName()
        {
            Assembly asm = Assembly.GetEntryAssembly();

            string exePath = asm.Location;

            String dir = Path.GetDirectoryName(exePath);

            string fileName = dir + @"\settings.json";

            return fileName;
        }

        public bool Save()
        {
            string fileName = FileName();

            JsonObject root = new JsonObject();

            JsonObject jo;

            root.Add("ContinueCreateFigure", ContinueCreateFigure);
            root.Add("LastDataDir", LastDataDir);
            root.Add("LastScriptDir", LastScriptDir);

            jo = new JsonObject();
            jo.Add("enable", SnapToPoint);
            jo.Add("range", PointSnapRange);
            root.Add("PointSnap", jo);

            jo = new JsonObject();
            jo.Add("enable", SnapToSegment);
            root.Add("SegmentSnap", jo);

            jo = new JsonObject();
            jo.Add("enable", SnapToLine);
            jo.Add("range", LineSnapRange);
            root.Add("LineSnap", jo);

            jo = new JsonObject();
            jo.Add("unit", KeyMoveUnit);
            root.Add("KeyMove", jo);

            jo = new JsonObject();
            jo.Add("enable", SnapToZero);
            root.Add("ZeroSnap", jo);

            jo = new JsonObject();
            jo.Add("enable", SnapToLastDownPoint);
            root.Add("LastDownSnap", jo);

            jo = new JsonObject();
            jo.Add("enable", SnapToSelfPoint);
            root.Add("SelfPointSnap", jo);

            jo = new JsonObject();
            jo.Add("enable", SnapToGrid);
            jo.Add("size_x", GridSize.X);
            jo.Add("size_y", GridSize.Y);
            jo.Add("size_z", GridSize.Z);
            root.Add("GridInfo", jo);


            jo = new JsonObject();
            jo.Add("DrawMode", (int)DrawMode);
            jo.Add("DrawFaceOutline", DrawMeshEdge);
            jo.Add("FillFace", FillMesh);
            jo.Add("DrawNormal", DrawNormal);
            jo.Add("DrawAxis", DrawAxis);
            jo.Add("DrawAxisLabel", DrawAxisLabel);
            jo.Add("DrawCompass", DrawCompass);
            root.Add("DrawSettings", jo);

            jo = new JsonObject();
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

            if (root.TryGetProperty("KeyMove", out jo))
            {
                KeyMoveUnit = jo.GetDouble("unit", KeyMoveUnit);
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
                DrawMode = jo.GetEnum<DrawTools.DrawMode>("DrawMode", DrawMode);
                DrawMeshEdge = jo.GetBool("DrawFaceOutline", DrawMeshEdge);
                FillMesh = jo.GetBool("FillFace", FillMesh);
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
}