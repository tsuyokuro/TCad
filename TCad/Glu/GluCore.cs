using OpenTK.Graphics.OpenGL;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace OpenGL.GLU;

public static partial class Glu
{
    internal static partial class Imports
    {
        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBeginCurve")]
        internal static partial void BeginCurve(IntPtr nurb);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBeginPolygon")]
        internal static partial void BeginPolygon(IntPtr tess);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBeginSurface")]
        internal static partial void BeginSurface(IntPtr nurb);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBeginTrim")]
        internal static partial void BeginTrim(IntPtr nurb);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBuild1DMipmapLevels")]
        internal static partial Int32 Build1DMipmapLevels(TextureTarget target, Int32 internalFormat, Int32 width, PixelFormat format, PixelType type, Int32 level, Int32 @base, Int32 max, IntPtr data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBuild1DMipmaps")]
        internal static partial Int32 Build1DMipmaps(TextureTarget target, Int32 internalFormat, Int32 width, PixelFormat format, PixelType type, IntPtr data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBuild2DMipmapLevels")]
        internal static partial Int32 Build2DMipmapLevels(TextureTarget target, Int32 internalFormat, Int32 width, Int32 height, PixelFormat format, PixelType type, Int32 level, Int32 @base, Int32 max, IntPtr data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBuild2DMipmaps")]
        internal static partial Int32 Build2DMipmaps(TextureTarget target, Int32 internalFormat, Int32 width, Int32 height, PixelFormat format, PixelType type, IntPtr data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBuild3DMipmapLevels")]
        internal static partial Int32 Build3DMipmapLevels(TextureTarget target, Int32 internalFormat, Int32 width, Int32 height, Int32 depth, PixelFormat format, PixelType type, Int32 level, Int32 @base, Int32 max, IntPtr data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluBuild3DMipmaps")]
        internal static partial Int32 Build3DMipmaps(TextureTarget target, Int32 internalFormat, Int32 width, Int32 height, Int32 depth, PixelFormat format, PixelType type, IntPtr data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluCheckExtension")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static unsafe partial bool CheckExtension(Byte* extName, Byte* extString);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluCylinder")]
        internal static partial void Cylinder(IntPtr quad, double @base, double top, double height, Int32 slices, Int32 stacks);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluDeleteNurbsRenderer")]
        internal static partial void DeleteNurbsRenderer(IntPtr nurb);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluDeleteQuadric")]
        internal static partial void DeleteQuadric(IntPtr quad);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluDeleteTess")]
        internal static partial void DeleteTess(IntPtr tess);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluDisk")]
        internal static partial void Disk(IntPtr quad, double inner, double outer, Int32 slices, Int32 loops);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluEndCurve")]
        internal static partial void EndCurve(IntPtr nurb);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluEndPolygon")]
        internal static partial void EndPolygon(IntPtr tess);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluEndSurface")]
        internal static partial void EndSurface(IntPtr nurb);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluEndTrim")]
        internal static partial void EndTrim(IntPtr nurb);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluErrorString")]
        internal static partial IntPtr ErrorString(GluErrorCode error);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluGetString")]
        internal static partial IntPtr GetString(GluStringName name);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluGetNurbsProperty")]
        internal static unsafe partial void GetNurbsProperty(IntPtr nurb, GluNurbsProperty property, float* data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluGetTessProperty")]
        internal static unsafe partial void GetTessProperty(IntPtr tess, GluTessParameter which, double* data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluLoadSamplingMatrices")]
        internal static unsafe partial void LoadSamplingMatrices(IntPtr nurb, float* model, float* perspective, Int32* view);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluLookAt")]
        internal static partial void LookAt(double eyeX, double eyeY, double eyeZ, double centerX, double centerY, double centerZ, double upX, double upY, double upZ);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNewNurbsRenderer")]
        internal static partial IntPtr NewNurbsRenderer();

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNewQuadric")]
        internal static partial IntPtr NewQuadric();

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNewTess")]
        internal static partial IntPtr NewTess();

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNextContour")]
        internal static partial void NextContour(IntPtr tess, GluTessContour type);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNurbsCallback")]
        internal static partial void NurbsCallback(IntPtr nurb, GluNurbsCallback which, IntPtr CallBackFunc);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNurbsCallbackData")]
        internal static partial void NurbsCallbackData(IntPtr nurb, IntPtr userData);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNurbsCurve")]
        internal static unsafe partial void NurbsCurve(IntPtr nurb, Int32 knotCount, float* knots, Int32 stride, float* control, Int32 order, MapTarget type);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNurbsProperty")]
        internal static partial void NurbsProperty(IntPtr nurb, GluNurbsProperty property, float value);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluNurbsSurface")]
        internal static unsafe partial void NurbsSurface(IntPtr nurb, Int32 sKnotCount, float* sKnots, Int32 tKnotCount, float* tKnots, Int32 sStride, Int32 tStride, float* control, Int32 sOrder, Int32 tOrder, MapTarget type);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluOrtho2D")]
        internal static partial void Ortho2D(double left, double right, double bottom, double top);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluPartialDisk")]
        internal static partial void PartialDisk(IntPtr quad, double inner, double outer, Int32 slices, Int32 loops, double start, double sweep);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluPerspective")]
        internal static partial void Perspective(double fovy, double aspect, double zNear, double zFar);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluPickMatrix")]
        internal static unsafe partial void PickMatrix(double x, double y, double delX, double delY, Int32* viewport);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluProject")]
        internal static unsafe partial Int32 Project(double objX, double objY, double objZ, double* model, double* proj, Int32* view, double* winX, double* winY, double* winZ);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluPwlCurve")]
        internal static unsafe partial void PwlCurve(IntPtr nurb, Int32 count, float* data, Int32 stride, GluNurbsTrim type);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluQuadricCallback")]
        internal static partial void QuadricCallback(IntPtr quad, GluQuadricCallback which, IntPtr CallBackFunc);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluQuadricDrawStyle")]
        internal static partial void QuadricDrawStyle(IntPtr quad, GluQuadricDrawStyle draw);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluQuadricNormals")]
        internal static partial void QuadricNormals(IntPtr quad, GluQuadricNormal normal);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluQuadricOrientation")]
        internal static partial void QuadricOrientation(IntPtr quad, GluQuadricOrientation orientation);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluQuadricTexture")]
        internal static partial void QuadricTexture(IntPtr quad, [MarshalAs(UnmanagedType.Bool)] bool texture);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluScaleImage")]
        internal static partial Int32 ScaleImage(PixelFormat format, Int32 wIn, Int32 hIn, PixelType typeIn, IntPtr dataIn, Int32 wOut, Int32 hOut, PixelType typeOut, IntPtr dataOut);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluSphere")]
        internal static partial void Sphere(IntPtr quad, double radius, Int32 slices, Int32 stacks);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluTessBeginContour")]
        internal static partial void TessBeginContour(IntPtr tess);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluTessBeginPolygon")]
        internal static partial void TessBeginPolygon(IntPtr tess, IntPtr data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluTessCallback")]
        internal static partial void TessCallback(IntPtr tess, GluTessCallback which, IntPtr CallBackFunc);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluTessEndContour")]
        internal static partial void TessEndContour(IntPtr tess);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluTessEndPolygon")]
        internal static partial void TessEndPolygon(IntPtr tess);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluTessNormal")]
        internal static partial void TessNormal(IntPtr tess, double valueX, double valueY, double valueZ);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluTessProperty")]
        internal static partial void TessProperty(IntPtr tess, GluTessParameter which, double data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluTessVertex")]
        internal static unsafe partial void TessVertex(IntPtr tess, double* location, IntPtr data);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluUnProject")]
        internal static unsafe partial Int32 UnProject(double winX, double winY, double winZ, double* model, double* proj, Int32* view, double* objX, double* objY, double* objZ);

        [SuppressUnmanagedCodeSecurity()]
        [LibraryImport(DllName, EntryPoint = "gluUnProject4")]
        internal static unsafe partial Int32 UnProject4(double winX, double winY, double winZ, double clipW, double* model, double* proj, Int32* view, double near, double far, double* objX, double* objY, double* objZ, double* objW);
    }
}
