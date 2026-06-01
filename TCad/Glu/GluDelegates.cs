using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;
using System.Security;

#pragma warning disable 0649

namespace OpenGL.GLU;

public static partial class Glu
{
    private static class Delegates
    {
        [SuppressUnmanagedCodeSecurity()]
        internal delegate void BeginCurve(IntPtr nurb);
        internal static BeginCurve gluBeginCurve;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void BeginPolygon(IntPtr tess);
        internal static BeginPolygon gluBeginPolygon;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void BeginSurface(IntPtr nurb);
        internal static BeginSurface gluBeginSurface;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void BeginTrim(IntPtr nurb);
        internal static BeginTrim gluBeginTrim;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate Int32 Build1DMipmapLevels(TextureTarget target, Int32 internalFormat, Int32 width, PixelFormat format, PixelType type, Int32 level, Int32 @base, Int32 max, IntPtr data);
        internal static Build1DMipmapLevels gluBuild1DMipmapLevels;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate Int32 Build1DMipmaps(TextureTarget target, Int32 internalFormat, Int32 width, PixelFormat format, PixelType type, IntPtr data);
        internal static Build1DMipmaps gluBuild1DMipmaps;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate Int32 Build2DMipmapLevels(TextureTarget target, Int32 internalFormat, Int32 width, Int32 height, PixelFormat format, PixelType type, Int32 level, Int32 @base, Int32 max, IntPtr data);
        internal static Build2DMipmapLevels gluBuild2DMipmapLevels;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate Int32 Build2DMipmaps(TextureTarget target, Int32 internalFormat, Int32 width, Int32 height, PixelFormat format, PixelType type, IntPtr data);
        internal static Build2DMipmaps gluBuild2DMipmaps;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate Int32 Build3DMipmapLevels(TextureTarget target, Int32 internalFormat, Int32 width, Int32 height, Int32 depth, PixelFormat format, PixelType type, Int32 level, Int32 @base, Int32 max, IntPtr data);
        internal static Build3DMipmapLevels gluBuild3DMipmapLevels;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate Int32 Build3DMipmaps(TextureTarget target, Int32 internalFormat, Int32 width, Int32 height, Int32 depth, PixelFormat format, PixelType type, IntPtr data);
        internal static Build3DMipmaps gluBuild3DMipmaps;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate bool CheckExtension(Byte* extName, Byte* extString);
        internal unsafe static CheckExtension gluCheckExtension;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void Cylinder(IntPtr quad, double @base, double top, double height, Int32 slices, Int32 stacks);
        internal static Cylinder gluCylinder;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void DeleteNurbsRenderer(IntPtr nurb);
        internal static DeleteNurbsRenderer gluDeleteNurbsRenderer;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void DeleteQuadric(IntPtr quad);
        internal static DeleteQuadric gluDeleteQuadric;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void DeleteTess(IntPtr tess);
        internal static DeleteTess gluDeleteTess;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void Disk(IntPtr quad, double inner, double outer, Int32 slices, Int32 loops);
        internal static Disk gluDisk;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void EndCurve(IntPtr nurb);
        internal static EndCurve gluEndCurve;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void EndPolygon(IntPtr tess);
        internal static EndPolygon gluEndPolygon;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void EndSurface(IntPtr nurb);
        internal static EndSurface gluEndSurface;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void EndTrim(IntPtr nurb);
        internal static EndTrim gluEndTrim;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate IntPtr ErrorString(GluErrorCode error);
        internal static ErrorString gluErrorString;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate IntPtr GetString(GluStringName name);
        internal static GetString gluGetString;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void GetNurbsProperty(IntPtr nurb, GluNurbsProperty property, float* data);
        internal unsafe static GetNurbsProperty gluGetNurbsProperty;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void GetTessProperty(IntPtr tess, GluTessParameter which, double* data);
        internal unsafe static GetTessProperty gluGetTessProperty;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void LoadSamplingMatrices(IntPtr nurb, float* model, float* perspective, Int32* view);
        internal unsafe static LoadSamplingMatrices gluLoadSamplingMatrices;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void LookAt(double eyeX, double eyeY, double eyeZ, double centerX, double centerY, double centerZ, double upX, double upY, double upZ);
        internal static LookAt gluLookAt;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate IntPtr NewNurbsRenderer();
        internal static NewNurbsRenderer gluNewNurbsRenderer;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate IntPtr NewQuadric();
        internal static NewQuadric gluNewQuadric;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate IntPtr NewTess();
        internal static NewTess gluNewTess;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void NextContour(IntPtr tess, GluTessContour type);
        internal static NextContour gluNextContour;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void NurbsCallback(IntPtr nurb, GluNurbsCallback which, IntPtr CallBackFunc);
        internal static NurbsCallback gluNurbsCallback;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void NurbsCallbackData(IntPtr nurb, IntPtr userData);
        internal static NurbsCallbackData gluNurbsCallbackData;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void NurbsCallbackDataEXT(IntPtr nurb, IntPtr userData);
        internal static NurbsCallbackDataEXT gluNurbsCallbackDataEXT;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void NurbsCurve(IntPtr nurb, Int32 knotCount, float* knots, Int32 stride, float* control, Int32 order, MapTarget type);
        internal unsafe static NurbsCurve gluNurbsCurve;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void NurbsProperty(IntPtr nurb, GluNurbsProperty property, float value);
        internal static NurbsProperty gluNurbsProperty;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void NurbsSurface(IntPtr nurb, Int32 sKnotCount, float* sKnots, Int32 tKnotCount, float* tKnots, Int32 sStride, Int32 tStride, float* control, Int32 sOrder, Int32 tOrder, MapTarget type);
        internal unsafe static NurbsSurface gluNurbsSurface;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void Ortho2D(double left, double right, double bottom, double top);
        internal static Ortho2D gluOrtho2D;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void PartialDisk(IntPtr quad, double inner, double outer, Int32 slices, Int32 loops, double start, double sweep);
        internal static PartialDisk gluPartialDisk;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void Perspective(double fovy, double aspect, double zNear, double zFar);
        internal static Perspective gluPerspective;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void PickMatrix(double x, double y, double delX, double delY, Int32* viewport);
        internal unsafe static PickMatrix gluPickMatrix;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate Int32 Project(double objX, double objY, double objZ, double* model, double* proj, Int32* view, double* winX, double* winY, double* winZ);
        internal unsafe static Project gluProject;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void PwlCurve(IntPtr nurb, Int32 count, float* data, Int32 stride, GluNurbsTrim type);
        internal unsafe static PwlCurve gluPwlCurve;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void QuadricCallback(IntPtr quad, GluQuadricCallback which, IntPtr CallBackFunc);
        internal static QuadricCallback gluQuadricCallback;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void QuadricDrawStyle(IntPtr quad, GluQuadricDrawStyle draw);
        internal static QuadricDrawStyle gluQuadricDrawStyle;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void QuadricNormals(IntPtr quad, GluQuadricNormal normal);
        internal static QuadricNormals gluQuadricNormals;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void QuadricOrientation(IntPtr quad, GluQuadricOrientation orientation);
        internal static QuadricOrientation gluQuadricOrientation;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void QuadricTexture(IntPtr quad, bool texture);
        internal static QuadricTexture gluQuadricTexture;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate Int32 ScaleImage(PixelFormat format, Int32 wIn, Int32 hIn, PixelType typeIn, IntPtr dataIn, Int32 wOut, Int32 hOut, PixelType typeOut, IntPtr dataOut);
        internal static ScaleImage gluScaleImage;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void Sphere(IntPtr quad, double radius, Int32 slices, Int32 stacks);
        internal static Sphere gluSphere;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void TessBeginContour(IntPtr tess);
        internal static TessBeginContour gluTessBeginContour;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void TessBeginPolygon(IntPtr tess, IntPtr data);
        internal static TessBeginPolygon gluTessBeginPolygon;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void TessCallback(IntPtr tess, GluTessCallback which, IntPtr CallBackFunc);
        internal static TessCallback gluTessCallback;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void TessEndContour(IntPtr tess);
        internal static TessEndContour gluTessEndContour;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void TessEndPolygon(IntPtr tess);
        internal static TessEndPolygon gluTessEndPolygon;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void TessNormal(IntPtr tess, double valueX, double valueY, double valueZ);
        internal static TessNormal gluTessNormal;

        [SuppressUnmanagedCodeSecurity()]
        internal delegate void TessProperty(IntPtr tess, GluTessParameter which, double data);
        internal static TessProperty gluTessProperty;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate void TessVertex(IntPtr tess, double* location, IntPtr data);
        internal unsafe static TessVertex gluTessVertex;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate Int32 TexFilterFuncSGI(TextureTarget target, SgisTextureFilter4 filtertype, float* parms, Int32 n, [Out] float* weights);
        internal unsafe static TexFilterFuncSGI gluTexFilterFuncSGI;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate Int32 UnProject(double winX, double winY, double winZ, double* model, double* proj, Int32* view, double* objX, double* objY, double* objZ);
        internal unsafe static UnProject gluUnProject;

        [SuppressUnmanagedCodeSecurity()]
        internal unsafe delegate Int32 UnProject4(double winX, double winY, double winZ, double clipW, double* model, double* proj, Int32* view, double near, double far, double* objX, double* objY, double* objZ, double* objW);
        internal unsafe static UnProject4 gluUnProject4;
    }
}
