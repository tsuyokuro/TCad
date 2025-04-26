using MessagePack;
//using JObj = Newtonsoft.Json.Linq.JObject;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using TCad.Plotter;
using TCad.Plotter.DrawContexts;
using TCad.Plotter.Serializer;
using TCad.Plotter.Serializer.v1003;
using TCad.Plotter.Serializer.v1004;
using TCad.Logger;

using JObj = System.Text.Json.Nodes.JsonObject;

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

public enum SerializeType
{
    MP_BIN,
    JSON,
}

public class SerializeContext
{
    public VersionCode Version { get; set; } = MpCadFile.CurrentVersion;

    public SerializeType SerializeType { get; set; } = SerializeType.MP_BIN;

    public SerializeContext(VersionCode version, SerializeType serializeType)
    {
        Version = version;
        SerializeType = serializeType;
    }
}

public class DeserializeContext
{
    public VersionCode Version { get; set; } = MpCadFile.CurrentVersion;
    public SerializeType SerializeType { get; set; } = SerializeType.MP_BIN;

    public DeserializeContext(VersionCode version, SerializeType serializeType)
    {
        Version = version;
        SerializeType = serializeType;
    }
}

public class MpCadFile
{
    public static VersionCode CurrentVersion = new VersionCode(1, 0, 0, 4);

    private static byte[] SignOld = Encoding.ASCII.GetBytes("KCAD_BIN");
    private static byte[] Sign = Encoding.ASCII.GetBytes("TCAD_BIN");
    private static string JsonSign = "TCAD_JSON";

    static MpCadFile()
    {
    }

    public static CadData? Load(string fname)
    {
        FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read);

        byte[] sign = new byte[Sign.Length];

        fs.ReadExactly(sign, 0, Sign.Length);

        if (!Sign.SequenceEqual<byte>(sign) && !SignOld.SequenceEqual<byte>(sign))
        {
            fs.Close();
            throw new CadFileException(CadFileException.ReasonCode.INCORRECT_TYPE);
        }

        byte[] version = new byte[VersionCode.CodeLength];

        fs.ReadExactly(version, 0, VersionCode.CodeLength);

        byte[] data = new byte[fs.Length - Sign.Length - VersionCode.CodeLength];

        fs.ReadExactly(data);

        fs.Close();

        Log.pl($"MpCadFile.Load {fname} {VersionStr(version)}");

        VersionCode fileVersion = new VersionCode(version);
        DeserializeContext dc = new DeserializeContext(fileVersion, SerializeType.MP_BIN);

        try
        {
            if (VersionCode_v1003.Version.Equals(version))
            {
                MpCadData_v1003 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1003>(data);
                return mpdata.Restore(dc);
            }
            else if (VersionCode_v1004.Version.Equals(version))
            {
                MpCadData_v1004 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1004>(data);
                return mpdata.Restore(dc);
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
        fs.ReadExactly(data);
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

        VersionCode fileVersion = new VersionCode(version);
        DeserializeContext dc = new DeserializeContext(fileVersion, SerializeType.JSON);

        try
        {
            if (version == VersionCode_v1003.Version.Str)
            {
                MpCadData_v1003 mpcd = MessagePackSerializer.Deserialize<MpCadData_v1003>(bin);
                return mpcd.Restore(dc);
            }
            else if (version == VersionCode_v1004.Version.Str)
            {
                MpCadData_v1004 mpcd = MessagePackSerializer.Deserialize<MpCadData_v1004>(bin);
                return mpcd.Restore(dc);
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
        SerializeContext sc = new SerializeContext(CurrentVersion, SerializeType.MP_BIN);

        var mpcd = new MpCadData_v1004();
        mpcd.Store(sc, cd);

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

        SerializeContext sc = new SerializeContext(CurrentVersion, SerializeType.JSON);

        var data = new MpCadData_v1004();
        data.Store(sc, cd);

        string dbJs = MessagePackSerializer.SerializeToJson(data);


        JObj body = (JObj)JObj.Parse(dbJs);
        root.Add("body", body);

        string ss = root.ToIndentedString();

        StreamWriter writer = new StreamWriter(fname);

        writer.Write(ss);

        writer.Close();
    }
}
