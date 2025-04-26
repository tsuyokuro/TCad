using System.Collections.Generic;

namespace TCad.Plotter;

public class CadRegion2D
{
    public vcompo_t X;
    public vcompo_t Y;
    public List<List<vcompo_t>> Data = new List<List<vcompo_t>>();
}
