using MessagePack;
using Plotter.Serializer.v1001;
using System.Collections.Generic;

namespace Plotter.Serializer
{
    public class MpUtil
    {
        public static byte[] FigListToBin(List<CadFigure> figList)
        {
            List<MpFigure_v1002> mpfigList = MpUtil_v1002.FigureListToMp_v1002(figList, true);

            byte[] bin = MessagePackSerializer.Serialize(mpfigList);

            return bin;
        }

        public static List<CadFigure> BinToFigList(byte[] bin)
        {
            List<MpFigure_v1002> mpfigList = MessagePackSerializer.Deserialize<List<MpFigure_v1002>>(bin);

            List<CadFigure> figList = MpUtil_v1002.FigureListFromMp_v1002(mpfigList);

            return figList;
        }

        public static byte[] FigToBin(CadFigure fig, bool withChild)
        {
            MpFigure_v1002 mpf = MpFigure_v1002.Create(fig, withChild);
            return MessagePackSerializer.Serialize(mpf);
        }

        public static CadFigure BinToFig(byte[] bin, CadObjectDB db = null)
        {
            MpFigure_v1002 mpfig = MessagePackSerializer.Deserialize<MpFigure_v1002>(bin);
            CadFigure fig = mpfig.Restore();

            if (db != null)
            {
                SetChildren(fig, mpfig.ChildIdList, db);
            }

            return fig;
        }

        public static void BinRestoreFig(byte[] bin, CadFigure fig, CadObjectDB db = null)
        {
            MpFigure_v1002 mpfig = MessagePackSerializer.Deserialize<MpFigure_v1002>(bin);
            mpfig.RestoreTo(fig);

            SetChildren(fig, mpfig.ChildIdList, db);
        }

        public static void BinRestoreFig(byte[] bin, CadObjectDB db = null)
        {
            if (db == null)
            {
                return;
            }

            MpFigure_v1002 mpfig = MessagePackSerializer.Deserialize<MpFigure_v1002>(bin);

            CadFigure fig = db.GetFigure(mpfig.ID);

            mpfig.RestoreTo(fig);

            SetChildren(fig, mpfig.ChildIdList, db);
        }


        #region LZ4
        public static byte[] FigToLz4Bin(CadFigure fig, bool withChild = false)
        {
            MpFigure_v1002 mpf = MpFigure_v1002.Create(fig, withChild);
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            return MessagePackSerializer.Serialize(mpf, lz4Options);
        }

        public static CadFigure Lz4BinToFig(byte[] bin, CadObjectDB db = null)
        {
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            MpFigure_v1002 mpfig = MessagePackSerializer.Deserialize<MpFigure_v1002>(bin, lz4Options);

            CadFigure fig = mpfig.Restore();

            if (db != null)
            {
                SetChildren(fig, mpfig.ChildIdList, db);
            }

            return fig;
        }

        public static void Lz4BinRestoreFig(byte[] bin, CadFigure fig, CadObjectDB db = null)
        {
            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            MpFigure_v1002 mpfig = MessagePackSerializer.Deserialize<MpFigure_v1002>(bin, lz4Options);
            mpfig.RestoreTo(fig);

            SetChildren(fig, mpfig.ChildIdList, db);
        }

        public static void Lz4BinRestoreFig(byte[] bin, CadObjectDB db = null)
        {
            if (db == null)
            {
                return;
            }

            var lz4Options = MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
            MpFigure_v1002 mpfig = MessagePackSerializer.Deserialize<MpFigure_v1002>(bin, lz4Options);

            CadFigure fig = db.GetFigure(mpfig.ID);

            mpfig.RestoreTo(fig);

            SetChildren(fig, mpfig.ChildIdList, db);
        }
        #endregion LZ4





        private static void SetChildren(CadFigure fig, List<uint> idList, CadObjectDB db)
        {
            for (int i = 0; i < idList.Count; i++)
            {
                fig.AddChild(db.GetFigure(idList[i]));
            }
        }
    }
}
