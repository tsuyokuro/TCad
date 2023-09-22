//#define DEFAULT_DATA_TYPE_DOUBLE
using System;
using System.Text.Json;




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


namespace Plotter.Serializer;

public static class JsonElementExtends
{
    public static bool GetBool(this JsonElement jo, string key, bool defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return prop.GetBoolean();
    }

    public static vcompo_t GetDouble(this JsonElement jo, string key, vcompo_t defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return (vcompo_t)prop.GetDouble();
    }
    public static string GetString(this JsonElement jo, string key, string defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return prop.GetString();
    }

    public static T GetEnum<T>(this JsonElement jo, string key, T defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        int num = prop.GetInt32();

        try
        {
            return (T)Enum.ToObject(typeof(T), num);
        }
        catch (ArgumentException)
        {
            return defaultValue;
        }
    }
}
