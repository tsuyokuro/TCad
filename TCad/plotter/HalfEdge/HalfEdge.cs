
using MyCollections;
using CadDataTypes;
using Newtonsoft.Json.Linq;
using Plotter;
using Plotter.Serializer;
using System;
using System.Collections.Generic;
using OpenTK;

namespace HalfEdgeNS
{
    public class HeFace
    {
        public uint ID;

        public HalfEdge Head; // HalfEdge link listの先頭

        public int Normal = HeModel.INVALID_INDEX;

        public HeFace(HalfEdge he)
        {
            Head = he;
        }
    }

    public class HalfEdge
    {
        public uint ID;

        public HalfEdge Pair;

        public HalfEdge Next;

        public HalfEdge Prev;

        public int Vertex = HeModel.INVALID_INDEX;

        // FaceのIndex(IDではない)
        public int Face = HeModel.INVALID_INDEX;

        public int Normal = HeModel.INVALID_INDEX;

        public HalfEdge(int vertex)
        {
            Vertex = vertex;
        }

        public HalfEdge()
        {
        }
    }

    public class HeModel
    {
        public const int INVALID_INDEX = -1;

        public IdProvider HeIdProvider = new IdProvider();

        public IdProvider FaceIdProvider = new IdProvider();

        public VertexList VertexStore;
        public FlexArray<HeFace> FaceStore;
        public Vector3dList NormalStore;

        public HeModel()
        {
            VertexStore = new VertexList(8);
            FaceStore = new FlexArray<HeFace>(6);
            NormalStore = new Vector3dList(8);
        }

        public void Clear()
        {
            VertexStore.Clear();
            FaceStore.Clear();
            NormalStore.Clear();
        }

        // 単純に頂点を追加
        public int AddVertex(CadVertex v)
        {
            return VertexStore.Add(v);
        }

        public HalfEdge CreateHalfEdge(int vindex)
        {
            HalfEdge he = new HalfEdge(vindex);
            he.ID = HeIdProvider.getNew();
            return he;
        }

        public HeFace CreateFace(HalfEdge head)
        {
            HeFace face = new HeFace(head);
            face.ID = FaceIdProvider.getNew();
            return face;
        }

        public List<HalfEdge> GetHalfEdgeList()
        {
            List<HalfEdge> list = new List<HalfEdge>();

            // すべてのFaceを巡回する
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                // Faceに含まれるHalfEdgeを巡回する
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    list.Add(c);

                    c = c.Next;

                    if (c == head) break;
                }
            }

            return list;
        }

        public void RecreateNormals()
        {
            Vector3dList newNormalStore = new Vector3dList(VertexStore.Count);

            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HeFace face = FaceStore[i];

                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                Vector3d n = CadMath.Normal(
                    VertexStore[c.Vertex].vector,
                    VertexStore[c.Next.Vertex].vector,
                    VertexStore[c.Next.Next.Vertex].vector
                    );

                face.Normal = newNormalStore.Add(n);
                
                for (; ; )
                {
                    c.Normal = newNormalStore.Add(n);

                    c = c.Next;

                    if (c == head) break;
                }
            }

            NormalStore = newNormalStore;
        }

        public void InvertAllFace()
        {
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HeFace face = FaceStore[i];

                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    HalfEdge next = c.Next;

                    c.Next = c.Prev;
                    c.Prev = next;

                    c = next;
                    if (c == head) break;
                }
            }

            for (i = 0; i<NormalStore.Count; i++)
            {
                NormalStore[i] = -NormalStore[i];
            }
        }

        // 頂点番号に関連づいたFaceを削除
        public void RemoveVertexRelationFace(int vindex)
        {
            int[] indexMap = new int[FaceStore.Count];

            var rmFaceList = FindFaceAll(vindex);

            for (int i=0; i<rmFaceList.Count; i++)
            {
                int rmFace = rmFaceList[i];

                RemoveFaceLink(rmFace);
                indexMap[rmFace] = -1;
            }

            int p = 0;

            for (int i=0; i<FaceStore.Count; i++)
            {
                if (indexMap[i] == -1)
                {

                }
                else
                {
                    indexMap[i] = p;
                    p++;
                }
            }

            for (int i = 0; i < FaceStore.Count; i++)
            {
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    c.Face = indexMap[c.Face];

                    c = c.Next;
                    if (c == head) break;
                }
            }

            for (int i = indexMap.Length-1; i >= 0; i--)
            {
                if (indexMap[i] == -1)
                {
                    FaceStore.RemoveAt(i);
                }
            }
        }

        public void RemoveVertexs(List<int> idxList)
        {
            int[] indexMap = new int[VertexStore.Count];

            for (int i=0; i<idxList.Count; i++)
            {
                indexMap[idxList[i]] = -1;
            }

            int r = 0;

            for (int i = 0; i < VertexStore.Count; i++)
            {
                if (indexMap[i] != -1)
                {
                    indexMap[i] = r;
                    r++;
                }
            }

            ForEachHalfEdge(he =>
            {
                he.Vertex = indexMap[he.Vertex];
                if (he.Vertex == -1)
                {
                    DOut.pl("HeModel.RemoveVertexs error. he.Vertex == -1");
                }
            });

            for (int i=VertexStore.Count-1; i>=0; i--)
            {
                if (indexMap[i] == -1)
                {
                    VertexStore.RemoveAt(i);
                }
            }
        }

        private void RemoveFaceLink(int idx)
        {
            HeFace face = FaceStore[idx];

            HalfEdge head = face.Head;
            HalfEdge c = head;

            for (; ; )
            {
                if (c.Pair != null)
                {
                    c.Pair.Pair = null;
                    c.Pair = null;
                }

                c = c.Next;
                if (c == head) break;
            }
        }

        public List<int> FindFaceAll(int vertexIndex)
        {
            var faceList = new List<int>();

            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    if (c.Vertex == vertexIndex)
                    {
                        faceList.Add(i);
                        break;
                    }

                    c = c.Next;
                    if (c == head) break;
                }
            }

            return faceList;
        }

        public FlexArray<int> GetOuterEdge()
        {
            // Pairを持たないHalfEdgeのリストを作成
            List<HalfEdge> heList = new List<HalfEdge>();
            
            ForEachHalfEdge(he => {
                if (he.Pair == null)
                {
                    heList.Add(he);
                }
            });

            FlexArray<int> ret = new FlexArray<int>();

            if (heList.Count <= 1)
            {
                return ret;
            }

            int s = FindMaxDistantHalfEdge(CadVertex.Zero, heList);

            if (s == -1)
            {
                DOut.pl("HeModel.GetOuterEdge not found start HalfEdge");
                return ret;
            }


            int t = s;
            HalfEdge whe = heList[t];

            int vi = whe.Vertex;

            heList.RemoveAt(t);

            while (true)
            {
                ret.Add(vi);
                vi = whe.Next.Vertex;

                t = FindHalfEdge(vi, heList);

                if (t == -1)
                {
                    break;
                }

                whe = heList[t];
                heList.RemoveAt(t);
            }

            return ret;
        }

        public int FindHalfEdge(int idx, List<HalfEdge> list)
        {
            for (int i=0; i<list.Count; i++)
            {
                if (list[i].Vertex == idx)
                {
                    return i;
                }
            }

            return -1;
        }

        // 指定された座標から最も遠いHalfEdgeを求める
        public int FindMaxDistantHalfEdge(CadVertex p0, List<HalfEdge> heList)
        {
            CadVertex t;

            double maxd = 0;

            int ret = -1;

            for (int i=0; i<heList.Count; i++)
            {
                int vi = heList[i].Vertex;

                if (vi == -1)
                {
                    continue;
                }

                CadVertex fp = VertexStore[vi];

                t = fp - p0;
                double d = t.Norm();

                if (d > maxd)
                {
                    maxd = d;
                    ret = i;
                }
            }

            return ret;
        }


        public void ForEachHalfEdge(Func<HalfEdge, bool> func)
        {
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    if (!func(c))
                    {
                        return;
                    }

                    c = c.Next;
                    if (c == head) break;
                }
            }
        }

        public void ForEachHalfEdge(Action<HalfEdge> action)
        {
            int i;
            for (i = 0; i < FaceStore.Count; i++)
            {
                HalfEdge head = FaceStore[i].Head;
                HalfEdge c = head;

                for (; ; )
                {
                    action(c);

                    c = c.Next;
                    if (c == head) break;
                }
            }
        }

        public List<int> GetEdgePointList()
        {
            List<int> idxList = new List<int>();

            HeFace f = FaceStore[0];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            for (; ; )
            {
                if (c.Pair == null)
                {
                    idxList.Add(c.Vertex);
                    c = c.Next;
                }
                else
                {
                    c = c.Pair;
                    c = c.Next;
                }

                if (c.ID == head.ID)
                {
                    break;
                }
            }

            return idxList;
        }

        public void ForReachEdgePoint(Func<CadVertex, bool> func)
        {
            HeFace f = FaceStore[0];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            for (; ; )
            {
                if (c.Pair == null)
                {
                    if (!func(VertexStore[c.Vertex]))
                    {
                        break;
                    }
                    c = c.Next;
                }
                else
                {
                    c = c.Pair;
                    c = c.Next;
                }

                if (c.ID == head.ID)
                {
                    break;
                }
            }
        }

        public void ForReachEdgePoint(Action<CadVertex> action)
        {
            HeFace f = FaceStore[0];

            HalfEdge head = f.Head;

            HalfEdge c = head;

            for (; ; )
            {
                if (c.Pair == null)
                {
                    action(VertexStore[c.Vertex]);
                    c = c.Next;
                }
                else
                {
                    c = c.Pair;
                    c = c.Next;
                }

                if (c.ID == head.ID)
                {
                    break;
                }
            }
        }

    }

    public class HeConnector
    {
        public static uint GetHeKey(HalfEdge he)
        {
            return ((uint)he.Next.Vertex) << 16 | (uint)he.Vertex;
        }

        public static uint GetPairHeKey(HalfEdge he)
        {
            return ((uint)he.Vertex) << 16 | (uint)he.Next.Vertex;
        }

        public static uint GetHeKey(int next_v, int v)
        {
            return ((uint)next_v) << 16 | (uint)v;
        }

        public static void SetHalfEdgePair(HalfEdge he, Dictionary<uint, HalfEdge> map)
        {
            uint pair_key = GetPairHeKey(he);

            HalfEdge pair;

            if (!map.TryGetValue(pair_key, out pair))
            {
                return;
            }

            he.Pair = pair;
            pair.Pair = he;
        }
    }
}
