using TCad.Plotter.DrawContexts;

namespace TCad.Plotter;

public class CadData
{
    public CadObjectDB DB;
    public vcompo_t WorldScale;
    public PaperPageSize PageSize;

    public CadData()
    {
        DB = new CadObjectDB();
        WorldScale = (vcompo_t)1.0;
        PageSize = PaperPageSize.A4Portrate;
    }

    public CadData(CadObjectDB db, vcompo_t worldScale, PaperPageSize pageSize)
    {
        DB = db;
        WorldScale = worldScale;
        PageSize = pageSize;
    }
}
