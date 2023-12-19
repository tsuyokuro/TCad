//#define DEFAULT_DATA_TYPE_DOUBLE
using MessagePack;
using Plotter.Serializer.v1002;
using Plotter.Serializer.v1003;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using JObj = System.Text.Json.Nodes.JsonObject;
using JNode = System.Text.Json.Nodes.JsonNode;
//using JObj = Newtonsoft.Json.Linq.JObject;
using System;
using System.Xml;
using System.Runtime.Serialization.Json;





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

public struct CadData
{
    public CadObjectDB DB;
    public vcompo_t WorldScale;
    public PaperPageSize PageSize;

    public CadData(CadObjectDB db, vcompo_t worldScale, PaperPageSize pageSize)
    {
        DB = db;
        WorldScale = worldScale;
        PageSize = pageSize;
    }
}

public class CadFileException : Exception
{
    public enum ReasonCode
    {
        OTHER,
        INCORRECT_TYPE,
        DESERIALIZE_FAILED,
    }

    public ReasonCode Reason;

    public CadFileException(ReasonCode reason)
    {
        Reason = reason;
    }

    public string getMessage()
    {
        switch (Reason)
        {
            case ReasonCode.OTHER:
                return "Unknown error";
            case ReasonCode.INCORRECT_TYPE:
                return "Incorrect type signature";
            case ReasonCode.DESERIALIZE_FAILED:
                return "Deserialize failed";
            default:
                return "Unknown error";
        }
    }
}

public class MpCadFile
{
    private static byte[] SignOld = Encoding.ASCII.GetBytes("KCAD_BIN");
    private static byte[] Sign = Encoding.ASCII.GetBytes("TCAD_BIN");
    private static string JsonSign = "TCAD_JSON";
    private static VersionCode CurrentVersion = new VersionCode(1, 0, 0, 3);

    static MpCadFile()
    {
    }

    public static CadData? Load(string fname)
    {
        FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read);

        byte[] sign = new byte[Sign.Length];

        fs.Read(sign, 0, Sign.Length);

        if (!Sign.SequenceEqual<byte>(sign) && !SignOld.SequenceEqual<byte>(sign))
        {
            fs.Close();
            throw new CadFileException(CadFileException.ReasonCode.INCORRECT_TYPE);
        }

        byte[] version = new byte[VersionCode.CodeLength];

        fs.Read(version, 0, VersionCode.CodeLength);

        byte[] data = new byte[fs.Length - Sign.Length - VersionCode.CodeLength];

        fs.Read(data, 0, data.Length);

        fs.Close();

        DOut.pl($"MpCadFile.Load {fname} {VersionStr(version)}");

        try
        {
            if (VersionCode_v1002.Version.Equals(version))
            {
                MpCadData_v1002 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1002>(data);
                return MpUtil_v1002.CreateCadData_v1002(mpdata);
            }
            else if (VersionCode_v1003.Version.Equals(version))
            {
                MpCadData_v1003 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1003>(data);
                return mpdata.Restore();
            }
        }
        catch
        {
            throw new CadFileException(CadFileException.ReasonCode.DESERIALIZE_FAILED);
        }

        return null;
    }


    private static string VersionStr(byte[] v)
    {
        return $"{v[0]}.{v[1]}.{v[2]}.{v[3]}";
    }

    public static CadData? LoadJson(string fname)
    {
        FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read);

        byte[] data = new byte[fs.Length];
        fs.Read(data, 0, data.Length);
        fs.Close();


        Utf8JsonReader jsonReader = new Utf8JsonReader(data);

        string header = GetJsonObject(data, ref jsonReader, "header");
        if (header == null) return null;

        JsonDocument jheader = JsonDocument.Parse(header);

        JsonElement je;

        if (!jheader.RootElement.TryGetProperty("type", out je)) return null;
        string type = je.GetString();

        if (type != JsonSign)
        {
            throw new CadFileException(CadFileException.ReasonCode.INCORRECT_TYPE);
        }

        if (!jheader.RootElement.TryGetProperty("version", out je)) return null;
        string version = jheader.RootElement.GetProperty("version").GetString();
        
        string body = GetJsonObject(data, ref jsonReader, "body");
        if (body == null) return null;

        byte[] bin = MessagePackSerializer.ConvertFromJson(body);

        try
        {
            if (version == VersionCode_v1002.Version.Str)
            {
                MpCadData_v1002 mpcd = MessagePackSerializer.Deserialize<MpCadData_v1002>(bin);

                CadData cd = new CadData(
                    mpcd.GetDB(),
                    mpcd.ViewInfo.WorldScale,
                    mpcd.ViewInfo.PaperSettings.GetPaperPageSize()
                    );

                return cd;
            }
            else if (version == VersionCode_v1003.Version.Str)
            {
                MpCadData_v1003 mpcd = MessagePackSerializer.Deserialize<MpCadData_v1003>(bin);

                CadData cd = new CadData(
                    mpcd.GetDB(),
                    mpcd.ViewInfo.WorldScale,
                    mpcd.ViewInfo.PaperSettings.GetPaperPageSize()
                    );

                return cd;
            }
        }
        catch
        {
            throw new CadFileException(CadFileException.ReasonCode.DESERIALIZE_FAILED);
        }

        return null;
    }

    public static string GetJsonObject(byte[] data, ref Utf8JsonReader jsonReader, string pname)
    {
        int state = 0;
        int startIdx = 0;
        int len = 0;

        while (true)
        {
            if (!jsonReader.Read())
            {
                return null;
            }

            if (state == 0)
            {
                if (jsonReader.TokenType == JsonTokenType.PropertyName)
                {
                    if (jsonReader.GetString() == pname)
                    {
                        state = 1;
                    }

                }
            }
            else if (state == 1)
            {
                if (jsonReader.TokenType == JsonTokenType.StartObject)
                {
                    startIdx = (int)jsonReader.TokenStartIndex;
                    int scount = (int)jsonReader.BytesConsumed;
                    
                    jsonReader.Skip(); // Skip all members
                    
                    len = (int)jsonReader.BytesConsumed - scount + 1;

                    break;
                }
            }
        }

        if (len == 0)
        {
            return null;
        }

        string str = Encoding.UTF8.GetString(data, startIdx, len);

        return str;
    }


    public static void Save(string fname, CadData cd)
    {
        var mpcd = MpCadData_v1003.Create(cd);

        mpcd.MpDB.GarbageCollect();

        byte[] data = MessagePackSerializer.Serialize(mpcd);

        FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);

        fs.Write(Sign, 0, Sign.Length);
        fs.Write(CurrentVersion.Bytes, 0, VersionCode.CodeLength);
        fs.Write(data, 0, data.Length);

        fs.Close();
    }

    public static void SaveAsJson(string fname, CadData cd)
    {
        JObj root = new();
        JObj header = new();

        header.Add("type", MpCadFile.JsonSign);
        header.Add("version", MpCadFile.CurrentVersion.Str);
        root.Add("header", header);


        var data = MpCadData_v1003.Create(cd);

        string dbJs = MessagePackSerializer.SerializeToJson(data);


        JObj body = (JObj)JObj.Parse(dbJs);
        root.Add("body", body);

        string ss = root.ToIndentedString();

        StreamWriter writer = new StreamWriter(fname);

        writer.Write(ss);

        writer.Close();
    }
}
