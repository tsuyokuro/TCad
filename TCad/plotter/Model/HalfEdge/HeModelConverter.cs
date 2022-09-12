
using MyCollections;
using CadDataTypes;
using System.Collections.Generic;

namespace HalfEdgeNS
{
    public class HeModelConverter
    {
        public static HeModel ToHeModel(CadMesh src)
        {
            HeModel m = new HeModel();

            m.VertexStore = src.VertexStore;

            Dictionary<uint, HalfEdge> map = new Dictionary<uint, HalfEdge>();

            for (int fi = 0; fi < src.FaceStore.Count; fi++)
            {
                CadFace f = src.FaceStore[fi];

                int vi = f.VList[0];
                HalfEdge head = m.CreateHalfEdge(vi);
                HalfEdge current_he = head;

                HeFace face = m.CreateFace(head);
                int faceIndex = m.FaceStore.Add(face);

                current_he.Face = faceIndex;

                HalfEdge next_he;

                for (int pi = 1; pi < f.VList.Count; pi++)
                {
                    vi = f.VList[pi];
                    next_he = m.CreateHalfEdge(vi);

                    current_he.Next = next_he;
                    next_he.Prev = current_he;

                    next_he.Face = faceIndex;

                    current_he = next_he;
                }

                head.Prev = current_he;
                current_he.Next = head;


                HalfEdge c = head;

                for (; ; )
                {
                    HeConnector.SetHalfEdgePair(c, map);

                    map[HeConnector.GetHeKey(c)] = c;

                    c = c.Next;
                    if (c == head) break;
                }
            }

            m.RecreateNormals();

            return m;
        }

        public static CadMesh ToCadMesh(HeModel hem)
        {
            CadMesh cm = new CadMesh();

            cm.VertexStore = new VertexList(hem.VertexStore);
            cm.FaceStore = new FlexArray<CadFace>();

            for (int i=0; i < hem.FaceStore.Count;i++)
            {
                CadFace cf = ToCadFace(hem.FaceStore[i]);
                if (cf != null)
                {
                    cm.FaceStore.Add(cf);
                }
            }

            return cm;
        }

        public static CadFace ToCadFace(HeFace hef)
        {
            CadFace ret = new CadFace();

            HalfEdge head = hef.Head;
            HalfEdge c = head;

            while (c!=null)
            {
                ret.VList.Add(c.Vertex);

                c = c.Next;

                if (c == head)
                {
                    break;
                }
            }

            return ret;
        }
    }
}
