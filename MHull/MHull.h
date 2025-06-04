#pragma once

using namespace System;
#include "..\\StanHull\\hull.h"

namespace ManagedStanHull 
{
	// example of how to: http://msdn.microsoft.com/en-us/library/ms235281.aspx
	//Managed Class Wrappers
	//  - Hypnotron March.2007

	 public ref class MHullResult
	{
	public:
		
		MHullResult(){}

		MHullResult(HullResult &hr)
		{
			Triangles = !hr.mPolygons;
			FaceCount = hr.mNumFaces;
			IndexCount = hr.mNumIndices;
			Count = hr.mNumOutputVertices ;

			Vertices = gcnew array<float>(Count * 3);
			for (int i = 0 ; i < Count; i++)
			{
				Vertices[i * 3] = hr.mOutputVertices [i * 3];
				Vertices[i * 3+1] = hr.mOutputVertices [i * 3+1];
				Vertices[i * 3+2] = hr.mOutputVertices [i * 3+2];
			}
			Indices = gcnew array<unsigned int>(IndexCount);
			for (int i = 0 ; i < IndexCount; i++)
			{
				Indices[i] = hr.mIndices [i];
			}
		}

		bool Triangles;
		int FaceCount;
		int IndexCount;
		int Count;
		
		array<float>^ GetVertices(){return Vertices;};
		array<unsigned int>^ GetIndices(){return Indices;};
	private :
		array<float>^Vertices;
		array <unsigned int>^Indices;

	};

	public ref class MHull 
		{
		public:
			MHull()
			{
				m_hl = new HullLibrary();
			}

			~MHull(){}

			//MHullResult ^MHull::CreateConvexHull(bool makeTriangles, bool reverseOrder, bool extrudeSkinWidth, 
			//									int vertexCount,  float vertices[], int stride, float normalEpsilon,
			//									int maxHullVertices, int maxHullFaces, float skinWidth)

			MHullResult ^MHull::CreateConvexHull(bool makeTriangles, bool reverseOrder, bool extrudeSkinWidth, 
												int vertexCount,  array<float>^vertices, int stride, float 
												normalEpsilon, int maxHullVertices, int maxHullFaces, float skinWidth)
			{
				HullResult hr;
				HullDesc desc;
				
				unsigned int flags;
				if (makeTriangles) flags = flags | QF_TRIANGLES;
				if (reverseOrder) flags = flags | QF_REVERSE_ORDER;
				if (extrudeSkinWidth) flags = flags | QF_SKIN_WIDTH;

				pin_ptr<float>  tmpFloatArray = &vertices[0];

				desc.mFlags = flags;
				desc.mMaxFaces = maxHullFaces;
				desc.mMaxVertices = maxHullVertices;
				desc.mNormalEpsilon = normalEpsilon;
				desc.mSkinWidth = skinWidth;
				desc.mVcount = vertexCount;
				desc.mVertexStride = stride;

				desc.mVertices = tmpFloatArray ; //vertices;
				
				m_hl->CreateConvexHull(desc , hr);
				mHR = gcnew MHullResult(hr);
				
				m_hl->ReleaseResult(hr);
				return mHR;
			}

		private:
			HullLibrary  *m_hl;
			MHullResult  ^mHR;
	};
}
