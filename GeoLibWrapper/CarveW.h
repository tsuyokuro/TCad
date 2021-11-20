#pragma once

#include <carve/csg.hpp>
#include <carve/input.hpp>


#include "cad_data_types.h"

using namespace CadDataTypes;

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
		static carve::poly::Polyhedron* ToPolyhedron(CadMesh^ cadMesh);

		// –¢Žg—p
		static CadMesh^ CrateCylinder(int slices, double rad, double height);
		static CadMesh^ CrateRectangular(double sizeX, double sizeY, double sizeZ);
	};
}


