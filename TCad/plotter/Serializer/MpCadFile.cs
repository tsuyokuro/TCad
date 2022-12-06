using MessagePack;
using Plotter.Serializer.v1002;
using Plotter.Serializer.v1003;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using JObj = System.Text.Json.Nodes.JsonObject;
//using JObj = Newtonsoft.Json.Linq.JObject;
using System;


using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;

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
                return MpUtil_v1003.CreateCadData_v1003(mpdata);
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

    public static CadData? LoadJson_OLD(string fname)
    {
        StreamReader reader = new StreamReader(fname);

        reader.ReadLine(); // skip "{\n"
        string header = reader.ReadLine();
        Regex headerPtn = new Regex(@"version=([0-9a-fA-F\.]+)");

        Match m = headerPtn.Match(header);

        string version = "";

        if (m.Groups.Count >= 1)
        {
            version = m.Groups[1].Value;
        }

        string js = reader.ReadToEnd();
        reader.Close();

        js = js.Trim();
        js = js.Substring(0, js.Length - 1);
        js = "{" + js + "}";

        byte[] bin = MessagePackSerializer.ConvertFromJson(js);

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

        return null;
    }

    public static void Save(string fname, CadData cd)
    {
        var mpcd = MpUtil_v1003.CreateMpCadData_v1003(cd);

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
        JObj n = new JObj();
        JObj header = new JObj();

        header.Add("type", MpCadFile.JsonSign);
        header.Add("version", MpCadFile.CurrentVersion.Str);
        n.Add("header", header);

        string headerJs = n.ToString();
        headerJs = headerJs.Substring(1, headerJs.Length - 2);


        var data = MpUtil_v1003.CreateMpCadData_v1003(cd);
        string dbJs = MessagePackSerializer.SerializeToJson(data);
        dbJs = dbJs.Trim();
        dbJs = dbJs.Substring(1, dbJs.Length - 2);

        string ss = @"{" +
                    headerJs + "," +
                    @"""body"":" + "{" + dbJs + "}" +
                    "}";

        StreamWriter writer = new StreamWriter(fname);

        writer.Write(ss);

        writer.Close();
    }
}
