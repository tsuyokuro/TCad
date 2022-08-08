using HalfEdgeNS;
using MessagePack;
using System;
using System.Collections.Generic;
using CadDataTypes;
using SplineCurve;
using System.Drawing.Printing;
using OpenTK;
using OpenTK.Mathematics;

namespace Plotter.Serializer.v1001
{
    public class VersionCode_v1001
    {
        private static VersionCode Version_ = new VersionCode(1, 0, 0, 1);

        public static VersionCode Version => Version_;
    }

    // Drop Version 1001
}
