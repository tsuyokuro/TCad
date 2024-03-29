//#define DEFAULT_DATA_TYPE_DOUBLE
using MessagePack;
using Plotter.Serializer.v1003;
using System;
using System.Collections.Generic;
using System.Threading;



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

public class CopyUtil
{
    private delegate T Deserialize_<T>(ReadOnlyMemory<byte> buffer, MessagePackSerializerOptions options = null, CancellationToken cancellationToken = default);

    private static Deserialize_<List<MpFigure_v1003>> Deserialize = MessagePackSerializer.Deserialize<List<MpFigure_v1003>>;
    
    private static Deserialize_<MpFigure_v1003> DeserializeFig = MessagePackSerializer.Deserialize<MpFigure_v1003>;

    private static MpFigCreator<MpFigure_v1003> CreateMpFig = MpFigure_v1003.Create;

    private static SerializeContext SC = SerializeContext.MpBin;
    private static DeserializeContext DSC = DeserializeContext.MpBin;


    private static MessagePackSerializerOptions lz4Options
    {
        get => MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
    }

    public static byte[] FigListToBin(List<CadFigure> figList)
    {
        var mpfigList = MpUtil.FigureListToMp<MpFigure_v1003>(SC, figList, CreateMpFig, true);

        byte[] bin = MessagePackSerializer.Serialize(mpfigList);

        return bin;
    }

    public static List<CadFigure> BinToFigList(byte[] bin)
    {
        var mpfigList = Deserialize(bin);

        var figList = MpUtil.FigureListFromMp<MpFigure_v1003>(DSC, mpfigList);

        return figList;
    }

    public static byte[] FigToBin(CadFigure fig, bool withChild)
    {
        var mpf = CreateMpFig(SC, fig, withChild);
        return MessagePackSerializer.Serialize(mpf);
    }

    public static CadFigure BinToFig(byte[] bin, CadObjectDB db = null)
    {
        var mpfig = DeserializeFig(bin);
        CadFigure fig = mpfig.Restore(DSC);

        if (db != null)
        {
            SetChildren(fig, mpfig.ChildIdList, db);
        }

        return fig;
    }


    #region LZ4
    public static byte[] FigToLz4Bin(CadFigure fig, bool withChild = false)
    {
        var mpf = CreateMpFig(SC, fig, withChild);
        return MessagePackSerializer.Serialize(mpf, lz4Options);
    }

    public static CadFigure Lz4BinToFig(byte[] bin, CadObjectDB db = null)
    {
        var mpfig = DeserializeFig(bin, lz4Options);

        CadFigure fig = mpfig.Restore(DSC);

        if (db != null)
        {
            SetChildren(fig, mpfig.ChildIdList, db);
        }

        return fig;
    }

    public static void Lz4BinRestoreFig(byte[] bin, CadFigure fig, CadObjectDB db = null)
    {
        var mpfig = DeserializeFig(bin, lz4Options);
        mpfig.RestoreTo(DSC, fig);

        SetChildren(fig, mpfig.ChildIdList, db);
    }

    public static void Lz4BinRestoreFig(byte[] bin, CadObjectDB db = null)
    {
        if (db == null)
        {
            return;
        }

        var mpfig = DeserializeFig(bin, lz4Options);

        CadFigure fig = db.GetFigure(mpfig.ID);

        mpfig.RestoreTo(DSC, fig);

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

    public static byte[] DBToLz4(CadObjectDB db)
    {
        MpCadObjectDB_v1003 mpdb = MpCadObjectDB_v1003.Create(SC, db);
        byte[] bin = MessagePackSerializer.Serialize(mpdb, lz4Options);

        return bin;
    }

    public static CadObjectDB Lz4BinRestoreDB(byte[] bin)
    {
        MpCadObjectDB_v1003 mpdb = MessagePackSerializer.Deserialize<MpCadObjectDB_v1003>(bin, lz4Options);
        CadObjectDB db = mpdb.Restore(DSC);

        return db;
    }
}
