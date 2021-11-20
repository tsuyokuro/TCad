using Newtonsoft.Json.Linq;
using System;

namespace Plotter.Serializer
{
    public static class JsonExtends
    {
        public static double GetDouble(this JObject jo, string key, double defaultValue)
        {
            JToken jt = jo[key];

            if (jt == null)
            {
                return defaultValue;
            }

            return (double)jt;
        }

        public static bool GetBool(this JObject jo, string key, bool defaultValue)
        {
            JToken jt = jo[key];

            if (jt == null)
            {
                return defaultValue;
            }

            return (bool)jt;
        }

        public static string GetString(this JObject jo, string key, string defaultValue)
        {
            JToken jt = jo[key];

            if (jt == null)
            {
                return defaultValue;
            }

            return (string)jt;
        }

        public static T GetEnum<T>(this JObject jo, string key, T defaultValue)
        {
            JToken jt = jo[key];

            if (jt == null)
            {
                return defaultValue;
            }

            int num = (int)jt;

            try
            {
                return (T)Enum.ToObject(typeof(T), num);
            }
            catch (ArgumentException e)
            {
                return defaultValue;
            }
        }
    }
}
