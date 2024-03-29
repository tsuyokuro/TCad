#include "CarveW.h"
#include "geometry.hpp"

using namespace CadDataTypes;
using namespace MyCollections;

namespace CarveWapper
{
	CadMesh^ CarveW::AMinusB(CadMesh^ a, CadMesh^ b)
	{
		carve::poly::Polyhedron* pa = ToPolyhedron(a);
		carve::poly::Polyhedron* pb = ToPolyhedron(b);
	
		carve::poly::Polyhedron* pc =
			carve::csg::CSG().compute(pa, pb, carve::csg::CSG::A_MINUS_B);
	
		return ToCadMesh(pc);
	}

	CadMesh^ CarveW::Union(CadMesh^ a, CadMesh^ b)
	{
		carve::poly::Polyhedron* pa = ToPolyhedron(a);
		carve::poly::Polyhedron* pb = ToPolyhedron(b);

		carve::poly::Polyhedron* pc =
			carve::csg::CSG().compute(pa, pb, carve::csg::CSG::UNION);

		return ToCadMesh(pc);
	}

	CadMesh^ CarveW::Intersection(CadMesh^ a, CadMesh^ b)
	{
		carve::poly::Polyhedron* pa = ToPolyhedron(a);
		carve::poly::Polyhedron* pb = ToPolyhedron(b);

		carve::poly::Polyhedron* pc =
			carve::csg::CSG().compute(pa, pb, carve::csg::CSG::INTERSECTION);

		return ToCadMesh(pc);
	}

	CadMesh^ CarveW::ToCadMesh(carve::poly::Polyhedron* pmesh)
	{
		CadMesh^ ret = gcnew CadMesh();

		ret->VertexStore = gcnew VertexList();

		int vnum = pmesh->vertices.size();

		int i = 0;

		for (; i < vnum; i++)
		{
			carve::poly::Vertex<3> vertex = pmesh->vertices[i];
			ret->VertexStore->Add(CadVertex::Create3(vertex.v.x, vertex.v.y, vertex.v.z));
		}

		int fnum = pmesh->faces.size();

        ret->FaceStore = gcnew FlexArray<CadFace^>(fnum);

		i = 0;
		for (; i < fnum; i++)
		{
			carve::poly::Face<3U> face = pmesh->faces[i];
			CadFace^ cadFace = gcnew CadFace();

			int fvnum = face.nVertices();

			for (int j = 0; j < fvnum; j++) {
				const carve::poly::Vertex<3U>* pv = face.vertex(j);

				int idx = pmesh->vertexToIndex(pv);
				cadFace->VList->Add(idx);
			}

			ret->FaceStore->Add(cadFace);
		}

		return ret;
	}

	carve::poly::Polyhedron* CarveW::ToPolyhedron(CadMesh^ cadMesh)
	{
		carve::input::PolyhedronData data;

		int i;
		int vnum = cadMesh->VertexStore->Count;

		for (i = 0; i < vnum; i++)
		{
			CadVertex v = cadMesh->VertexStore->Data[i];
			data.addVertex(carve::geom::VECTOR(v.X, v.Y, v.Z));
		}

		int fnum = cadMesh->FaceStore->Count;

		for (i = 0; i < fnum; i++)
		{
			CadFace^ face = cadMesh->FaceStore->Data[i];

			int pnum = face->VList->Count;

			if (pnum == 3)
			{
				data.addFace(face->VList->Data[0], face->VList->Data[1], face->VList->Data[2]);
			}
			else if (pnum == 4)
			{
				data.addFace(face->VList->Data[0], face->VList->Data[1], face->VList->Data[2], face->VList->Data[3]);
			}
			else if (pnum > 4)
			{
				std::vector<int> vl;

				for (int j = 0; j < pnum; j++)
				{
					vl.push_back((int)(face->VList->Data[j]));
				}

				data.addFace(vl.begin(), vl.end());
			}
		}

		return data.create();
	}
}


