//#define DEFAULT_DATA_TYPE_DOUBLE


#if DEFAULT_DATA_TYPE_DOUBLE
global using vcompo_t = System.Double;
global using vector3_t = OpenTK.Mathematics.Vector3d;
global using vector4_t = OpenTK.Mathematics.Vector4d;
global using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
global using vcompo_t = System.Single;
global using vector3_t = OpenTK.Mathematics.Vector3;
global using vector4_t = OpenTK.Mathematics.Vector4;
global using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


