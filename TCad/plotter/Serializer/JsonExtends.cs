using System;
using System.Text.Json;


namespace Plotter.Serializer
{
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

        public static double GetDouble(this JsonElement jo, string key, double defaultValue)
        {
            JsonElement prop;

            if (!jo.TryGetProperty(key, out prop))
            {
                return defaultValue;
            }

            return prop.GetDouble();
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
}
