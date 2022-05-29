using MessagePack;
using Plotter.Serializer.v1001;
using Plotter.Serializer.v1002;
using Plotter.Serializer.v1003;
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
                return null;
            }

            byte[] version = new byte[VersionCode.CodeLength];

            fs.Read(version, 0, VersionCode.CodeLength);

            byte[] data = new byte[fs.Length - Sign.Length - VersionCode.CodeLength];

            fs.Read(data, 0, data.Length);

            fs.Close();

            DOut.pl($"MpCadFile.Load {fname} {VersionStr(version)}");

            if (VersionIs(version, VersionCode_v1001.Version.Code))
            {
            }
            else if (VersionIs(version, VersionCode_v1002.Version.Code))
            {
                MpCadData_v1002 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1002>(data);
                return MpUtil_v1002.CreateCadData_v1002(mpdata);
            }
            else
            {
                MpCadData_v1003 mpdata = MessagePackSerializer.Deserialize<MpCadData_v1003>(data);
                return MpUtil_v1003.CreateCadData_v1003(mpdata);
            }

            return null;
        }

        private static bool VersionIs(byte[] l, byte[] r)
        {
            return l[0] == r[0] && l[1] == r[1] && l[2] == r[2] && l[3] == r[3];
        }

        private static string VersionStr(byte[] v)
        {
            return $"{v[0]}.{v[1]}.{v[2]}.{v[3]}";
        }

        public static CadData? LoadJson(string fname)
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

            if (version == VersionCode_v1001.Version.Str)
            {
                return null;
            }
            else if (version == VersionCode_v1002.Version.Str)
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
            fs.Write(CurrentVersion.Code, 0, VersionCode.CodeLength);
            fs.Write(data, 0, data.Length);

            fs.Close();
        }

        public static void SaveAsJson(string fname, CadData cd)
        {
            var data = MpUtil_v1003.CreateMpCadData_v1003(cd);
            string s = MessagePackSerializer.SerializeToJson(data);

            s = s.Trim();

            s = s.Substring(1, s.Length - 2);

            string ss = @"{" + "\n" +
                        @"""header"":""" + "type=" + JsonSign + "," + "version=" + VersionCode_v1003.Version.Str + @"""," + "\n" +
                        s + "\n" +
                        @"}";

            StreamWriter writer = new StreamWriter(fname);

            writer.Write(ss);

            writer.Close();
        }
    }
}
