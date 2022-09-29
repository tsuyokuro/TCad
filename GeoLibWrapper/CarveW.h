#pragma once

#include <carve/csg.hpp>
#include <carve/input.hpp>


#include "cad_data_types.h"

using namespace CadDataTypes;
using namespace carve::poly;

namespace CarveWapper
{
	public ref class CarveW
	{
	public:
		CarveW();
		static CadMesh^ AMinusB(CadMesh^ a, CadMesh^ b);
		static CadMesh^ Union(CadMesh^ a, CadMesh^ b);
		static CadMesh^ Intersection(CadMesh^ a, CadMesh^ b);

		static CadMesh^ ToCadMesh(carve::poly::Polyhedron * pmesh);
		static Polyhedron* ToPolyhedron(CadMesh^ cadMesh);
    };
}


