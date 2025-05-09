#pragma unmanaged

#include "IglFuncs.h"

#include <iostream>
#include <igl/triangle/triangulate.h>
#include <igl/readOFF.h>

/*
#include <Eigen/Dense>
#include <Eigen/Sparse>
#include <Eigen/Core>
#include <Eigen/Geometry>
*/

#pragma managed 

#include <msclr/marshal_cppstd.h>

#include "IglW.h"

using namespace System::Runtime::InteropServices;
using namespace CadDataTypes;
using namespace MyCollections;
using namespace msclr::interop;

namespace LibiglWrapper
{
	void IglW::Test1(array<double>^ plist, int rows, int cols)
	{
		pin_ptr<double> p = &plist[0];
		Eigen::MatrixXd m = ArrayToMatrixXd(p, rows, cols);


		std::cout << m;

	}

	void IglW::Test2(VertexList^ vl)
	{
		Eigen::MatrixXd V = ToMatrixXd2D(vl);
		Eigen::MatrixXi E;
		Eigen::MatrixXd H;

		int n = vl->Count;

		E.resize(n, 2);

		int i;
		for (i = 0; i < n; i++)
		{
			E(i, 0) = i;
			E(i, 1) = ((i + 1) % n);
		}

		Eigen::MatrixXd V2;
		Eigen::MatrixXi F2;

		printf("Start triangulate\n");

		// 4th argument as string are quality option.
		// Value of after "a" is max area size for triangle
		// Value of after "q" is min angle for triangle. If write "q" and not set value,
		// min angle is 20 degree
		// Other options see
		// https://www.cs.cmu.edu/~quake/triangle.switch.html
		igl::triangle::triangulate(V, E, H, "a20q", V2, F2);
	
		printf("End triangulate\n");
	}

	void IglW::Test()
	{
	}

	CadMesh^ IglW::Triangulate(VertexList^ vl, String^ option)
	{
		Eigen::MatrixXd V = ToMatrixXd2D(vl);
		Eigen::MatrixXi E;
		Eigen::MatrixXd H;

		int n = vl->Count;

		E.resize(n, 2);

		int i;
		for (i = 0; i < n; i++)
		{
			E(i, 0) = i;
			E(i, 1) = ((i + 1) % n);
		}

		Eigen::MatrixXd V2;
		Eigen::MatrixXi F2;

		std::string s = marshal_as<std::string>(option);

		// 4th argument as string are quality option.
		// Value of after "a" is max area size for triangle
		// Value of after "q" is min angle for triangle. If write "q" and not set value,
		// min angle is 20 degree
		// e.g. a100q
		// Other options see
		// https://www.cs.cmu.edu/~quake/triangle.switch.html

		igl::triangle::triangulate(V, E, H, s, V2, F2);

		CadMesh^ ret = gcnew CadMesh();

		ret->VertexStore = IglW::ToVectorListWithRowMaijor2D(V2);
		ret->FaceStore = IglW::ToFaceListWithRowMaijor(F2);

		return ret;
	}

	Eigen::MatrixXd IglW::ToMatrixXd2D(VertexList^ vl)
	{
		Eigen::MatrixXd m;

		int rows = vl->Count;

		m.resize(rows, 2);

		for (int r = 0; r < rows; r++)
		{
			m(r, 0) = vl->Data[r].X;
			m(r, 1) = vl->Data[r].Y;
		}

		return m;
	}

	Eigen::MatrixXd IglW::ToMatrixXd(VertexList^ vl)
	{
		Eigen::MatrixXd m;

		int rows = vl->Count;

		m.resize(rows, 3);

		for (int r = 0; r < rows; r++)
		{
			m(r, 0) = vl->Data[r].X;
			m(r, 1) = vl->Data[r].Y;
			m(r, 2) = vl->Data[r].Z;
		}

		return m;
	}

	Eigen::MatrixXd IglW::ArrayToMatrixXd(double * data, int rows, int cols)
	{
		Eigen::MatrixXd m(rows, cols);

		int p = 0;

		for (int r = 0; r < rows; r++)
		{
			for (int c = 0; c < cols; c++)
			{
				m(r, c) = data[p++];
			}
		}

		return m;
	}

	CadMesh^ IglW::ReadOFF(String^ fname)
	{
		char* pfname = (char*)Marshal::StringToHGlobalAnsi(fname).ToPointer();

		Eigen::MatrixXd V;
		Eigen::MatrixXi F;

		igl::readOFF(pfname, V, F);

		CadMesh^ ret = gcnew CadMesh();

		ret->VertexStore = IglW::ToVectorListWithRowMaijor(V);
		ret->FaceStore = IglW::ToFaceListWithRowMaijor(F);

		return ret;
	}

	// RowMaijor
	// 
	VertexList^ IglW::ToVectorListWithRowMaijor(Eigen::MatrixXd m)
	{
		int rows = m.rows();
		int cols = m.cols();

		VertexList^ data = gcnew VertexList(cols * rows);

		CadVertex v;

		for (int r = 0; r < m.rows(); r++)
		{
			v.X = m(r, 0);
			v.Y = m(r, 1);
			v.Z = m(r, 2);

			data->Add(v);
		}
		return data;
	}

	VertexList^ IglW::ToVectorListWithRowMaijor2D(Eigen::MatrixXd m)
	{
		int rows = m.rows();
		int cols = m.cols();

		VertexList^ data = gcnew VertexList(cols * rows);

		CadVertex v;

		for (int r = 0; r < m.rows(); r++)
		{
			v.X = m(r, 0);
			v.Y = m(r, 1);
			v.Z = 0;
			data->Add(v);
		}
		return data;
	}

	FlexArray<CadFace^>^ IglW::ToFaceListWithRowMaijor(Eigen::MatrixXi m)
	{
		int rows = m.rows();
		int cols = m.cols();

		FlexArray<CadFace^>^ data = gcnew FlexArray<CadFace^>(rows);

		CadFace^ f;

		for (int r = 0; r < rows; r++)
		{
			f = gcnew CadFace();

			for (int c = 0; c < cols; c++)
			{
				f->VList->Add(m(r, c));
			}

			data->Add(f);
		}
		return data;
	}

	FlexArray<int>^ IglW::ToIntListWithRowMaijor(Eigen::MatrixXi m)
	{
		int rows = m.rows();
		int cols = m.cols();

		FlexArray<int>^ data = gcnew FlexArray<int>(cols * rows);

		for (int r = 0; r < m.rows(); r++)
		{
			for (int c = 0; c < cols; c++)
			{
				data->Add(m(r, c));
			}
		}
		return data;
	}
}
