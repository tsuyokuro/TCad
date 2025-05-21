using System;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;

namespace TCad.Util;

public static class JsonElementExtends
{
    public static Byte GetByte(this JsonElement jo, byte defaultValue)
    {
        if (jo.TryGetByte(out Byte val))
        {
            return val;
        }

        return defaultValue;
    }

    public static Byte GetByte(this JsonElement jo, string key, byte defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return GetByte(prop, defaultValue);
    }


    public static Int32 GetInt32(this JsonElement jo, Int32 defaultValue)
    {
        if (jo.TryGetInt32(out Int32 val))
        {
            return val;
        }

        return defaultValue;
    }

    public static Int32 GetInt32(this JsonElement jo, string key, int defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return GetInt32(prop, defaultValue);
    }


    public static float GetSingle(this JsonElement jo, float defaultValue)
    {
        if (jo.TryGetSingle(out float val))
        {
            return val;
        }

        return defaultValue;
    }

    public static float GetSingle(this JsonElement jo, string key, float defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return GetSingle(prop, defaultValue);
    }


    public static double GetDouble(this JsonElement jo, double defaultValue)
    {
        if (jo.TryGetDouble(out double val))
        {
            return val;
        }

        return defaultValue;
    }

    public static double GetDouble(this JsonElement jo, string key, double defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return prop.GetDouble(defaultValue);
    }


    public static vcompo_t GetVcompo(this JsonElement jo, vcompo_t defaultValue)
    {
        if (jo.TryGetDouble(out double val))
        {
            return (vcompo_t)val;
        }

        return defaultValue;
    }

    public static vcompo_t GetVcompo(this JsonElement jo, string key, vcompo_t defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return prop.GetVcompo(defaultValue);
    }


    public static bool GetBool(this JsonElement jo, bool defaultValue)
    {
        if (jo.ValueKind == JsonValueKind.True || jo.ValueKind == JsonValueKind.False)
        {
            return jo.GetBoolean();
        }

        return defaultValue;
    }

    public static bool GetBool(this JsonElement jo, string key, bool defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return prop.GetBool(defaultValue);
    }


    public static string GetString(this JsonElement jo, string defaultValue)
    {
        if (jo.ValueKind == JsonValueKind.String)
        {
            return jo.GetString();
        }

        return defaultValue;
    }


    public static string GetString(this JsonElement jo, string key, string defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return prop.GetString(defaultValue);
    }

    public static T GetEnum<T>(this JsonElement jo, T defaultValue)
    {
        if (jo.TryGetInt32(out int num))
        {
            try
            {
                return (T)Enum.ToObject(typeof(T), num);
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }

        return defaultValue;
    }

    public static T GetEnum<T>(this JsonElement jo, string key, T defaultValue)
    {
        JsonElement prop;

        if (!jo.TryGetProperty(key, out prop))
        {
            return defaultValue;
        }

        return prop.GetEnum<T>(defaultValue);
    }
}

public static class JsonObjExtends
{
    public static string ToIndentedString(this JsonObject jo)
    {
        return convertToIndentedJson(jo.ToJsonString());
    }


    public static string convertToIndentedJson(string json)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        using (MemoryStream stream = new MemoryStream())
        using (XmlDictionaryWriter writer = JsonReaderWriterFactory.CreateJsonWriter(stream, Encoding.UTF8, true, true))
        using (XmlDictionaryReader reader = JsonReaderWriterFactory.CreateJsonReader(buffer, XmlDictionaryReaderQuotas.Max))
        {
            writer.WriteNode(reader, true);
            writer.Flush();
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }
}
