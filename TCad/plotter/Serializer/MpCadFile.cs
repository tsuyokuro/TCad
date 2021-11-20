using MessagePack;
using Plotter.Serializer.v1001;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Plotter.Serializer
{
    public struct CadData
    {
        public CadObjectDB DB;
        public double WorldScale;
        public PaperPageSize PageSize;

        public CadData(CadObjectDB db, double worldScale, PaperPageSize pageSize)
        {
            DB = db;
            WorldScale = worldScale;
            PageSize = pageSize;
        }
    }

    public class MpCadFile
    {
        private static byte[] Sign;
        private static byte[] Version = { 1, 0, 0, 2 };
        private static string JsonSign = "KCAD_JSON";
        private static string JsonVersion = "1002";

        static MpCadFile()
        {
            Sign = Encoding.ASCII.GetBytes("KCAD_BIN");
        }

        public static CadData? Load(string fname)
        {
            FileStream fs = new FileStream(fname, FileMode.Open, FileAccess.Read);

            byte[] sign = new byte[Sign.Length];

            fs.Read(sign, 0, Sign.Length);

            if (!Sign.SequenceEqual<byte>(sign))
            {
                fs.Close();
                return null;
            }

            byte[] version = new byte[Version.Length];

            fs.Read(version, 0, Version.Length);

            byte[] data = new byte[fs.Length - Sign.Length - Version.Length];

            fs.Read(data, 0, data.Length);

            fs.Close();

            DOut.pl($"MpCadFile.Load {fname} {VersionStr(version)}");

            if (IsVersion(version, 1, 0, 0, 0))
            {
                return null;
            }
            else if (IsVersion(version, 1, 0, 0, 1))
            {
                MpCadData_v1001 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1001>(data);
                return MpUtil_v1001.CreateCadData_v1001(mpdata);
            }
            else if (IsVersion(version, 1, 0, 0, 2))
            {
                MpCadData_v1002 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1002>(data);
                return MpUtil_v1002.CreateCadData_v1002(mpdata);
            }

            return null;
        }

        private static bool IsVersion(byte[] v, int v0, int v1, int v2, int v3)
        {
            return v[0] == v0 && v[1] == v1 && v[2] == v2 && v[3] == v3;
        }

        private static string VersionStr(byte[] v)
        {
            return $"MpCadFile.Load {v[0]}.{v[1]}.{v[2]}.{v[3]}";
        }

        public static CadData? LoadJson(string fname)
        {
            StreamReader reader = new StreamReader(fname);

            reader.ReadLine(); // skip "{\n"
            string header = reader.ReadLine();
            Regex headerPtn = new Regex(@"version=([0-9a-fA-F]+)");

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

            if (version == "1001")
            {
                MpCadData_v1001 mpcd = MessagePackSerializer.Deserialize<MpCadData_v1001>(bin);

                CadData cd = new CadData(
                    mpcd.GetDB(),
                    mpcd.ViewInfo.WorldScale,
                    mpcd.ViewInfo.PaperSettings.GetPaperPageSize()
                    );

                return cd;
            }
            else if (version == "1002")
            {
                MpCadData_v1002 mpcd = MessagePackSerializer.Deserialize<MpCadData_v1002>(bin);

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
            MpCadData_v1002 mpcd = MpUtil_v1002.CreateMpCadData_v1002(cd);

            mpcd.MpDB.GarbageCollect();

            byte[] data = MessagePackSerializer.Serialize(mpcd);

            FileStream fs = new FileStream(fname, FileMode.Create, FileAccess.Write);

            fs.Write(Sign, 0, Sign.Length);
            fs.Write(Version, 0, Version.Length);
            fs.Write(data, 0, data.Length);

            fs.Close();
        }

        public static void SaveAsJson(string fname, CadData cd)
        {
            MpCadData_v1002 data = MpUtil_v1002.CreateMpCadData_v1002(cd);
            string s = MessagePackSerializer.SerializeToJson(data);

            s = s.Trim();

            s = s.Substring(1, s.Length - 2);

            string ss = @"{" + "\n" +
                        @"""header"":""" + "type=" + JsonSign + "," + "version=" + JsonVersion + @"""," + "\n" +
                        s + "\n" +
                        @"}";

            StreamWriter writer = new StreamWriter(fname);

            writer.Write(ss);

            writer.Close();
        }
    }
}
