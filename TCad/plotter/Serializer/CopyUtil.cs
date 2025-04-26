using MessagePack;
using Plotter;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using System.Threading;
using TCad.Plotter.Model.Figure;
using TCad.Plotter.Serializer.v1004;

namespace TCad.Plotter.Serializer;


using MpCadObjectDB = MpCadObjectDB_v1004;
using MpFig = MpFigure_v1004;

public class CopyUtil
{
    private delegate T Deserialize_<T>(ReadOnlyMemory<byte> buffer, MessagePackSerializerOptions options = null, CancellationToken cancellationToken = default);

    private static Deserialize_<List<MpFig>> Deserialize = MessagePackSerializer.Deserialize<List<MpFig>>;

    private static Deserialize_<MpFig> DeserializeFig = MessagePackSerializer.Deserialize<MpFig>;

    private static SerializeContext SC = new(MpCadFile.CurrentVersion, SerializeType.MP_BIN);
    private static DeserializeContext DSC = new(MpCadFile.CurrentVersion, SerializeType.MP_BIN);


    private static MessagePackSerializerOptions lz4Options
    {
        get => MessagePackSerializerOptions.Standard.WithCompression(MessagePackCompression.Lz4BlockArray);
    }

    public static byte[] FigListToBin(List<CadFigure> figList)
    {
        var mpfigList = MpUtil.FigureListToMp<MpFig>(SC, figList, true);

        byte[] bin = MessagePackSerializer.Serialize(mpfigList);

        return bin;
    }

    public static List<CadFigure> BinToFigList(byte[] bin)
    {
        var mpfigList = Deserialize(bin);

        var figList = MpUtil.FigureListFromMp(DSC, mpfigList);

        return figList;
    }

    public static byte[] FigToBin(CadFigure fig, bool withChild)
    {
        var mpf = new MpFig();
        mpf.Store(SC, fig, withChild);
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
        var mpf = new MpFig();
        mpf.Store(SC, fig, withChild);
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
        MpCadObjectDB mpdb = new MpCadObjectDB();
        mpdb.Store(SC, db);
        byte[] bin = MessagePackSerializer.Serialize(mpdb, lz4Options);

        return bin;
    }

    public static CadObjectDB Lz4BinRestoreDB(byte[] bin)
    {
        MpCadObjectDB mpdb = MessagePackSerializer.Deserialize<MpCadObjectDB>(bin, lz4Options);
        CadObjectDB db = mpdb.Restore(DSC);

        return db;
    }
}
