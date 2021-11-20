using CadDataTypes;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Plotter.svg
{
    public static class FigureXmlExt
    {
        public static XElement ToPath(CadFigurePolyLines fig, DrawContext dc, double width, double height, double lineW)
        {
            StringBuilder sb = new StringBuilder("");

            int state = 0;

            foreach (CadVertex v in fig.PointList)
            {
                Vector3d dv = dc.WorldPointToDevPoint(v.vector);
                if (state == 0)
                {
                    sb.Append("M ");
                    state = 1;
                }
                else
                {
                    sb.Append(" L ");
                }
                sb.Append($"{dv.X:F3} {dv.Y:F3}");
            }

            if (fig.IsLoop)
            {
                sb.Append(" z");
            }

            XElement ele = new XElement("path",
                new XAttribute("d", sb.ToString()),
                new XAttribute("fill", "none"),
                new XAttribute("stroke", "black"),
                new XAttribute("stroke-width", lineW)
                );

            //DOut.pl(ele.ToString());

            return ele;
        }
    }

    public class SvgExporter
    {
        public double DefaultLineW = 0.2;


        public static XDocumentType DocType = new XDocumentType(
                @"svg", @" -//W3C//DTD SVG 1.1//EN",
                @"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd",
                null);


        public XDocument ToSvg(List<CadFigure> figList, DrawContext currentDC, double width, double height)
        {
            DrawContext dc = currentDC.Clone();

            dc.SetViewSize(width, height);
            dc.SetViewOrg(new Vector3d(width / 2, height / 2, 0));
            dc.UnitPerMilli = 1;

            XDocument doc = new XDocument();

            doc.Add(DocType);

            XElement root = CreateRoot(width, height);

            AddFiguresToElement(root, figList, dc, width, height);

            doc.Add(root);
            return doc;
        }

        public void AddFiguresToElement(
            XElement parent, List<CadFigure> figList, DrawContext dc, double width, double height)
        {
            foreach (CadFigure fig in figList)
            {
                if (fig is CadFigurePolyLines)
                {
                    XElement ele = FigureXmlExt.ToPath((CadFigurePolyLines)fig, dc, width, height, DefaultLineW);
                    parent.Add(ele);
                }
            }
        }

        public static XElement CreateRoot(double width, double height)
        {
            XElement root = new XElement("svg",
                new XAttribute("width", $"{width}mm"),
                new XAttribute("height", $"{height}mm"),
                new XAttribute("viewBox", $"0 0 {width} {height}"),
                new XAttribute(XNamespace.Xmlns + "svg", "http://www.w3.org/2000/svg"),
                new XAttribute("version", "1.1")
                );
            return root;
        }
    }
}
