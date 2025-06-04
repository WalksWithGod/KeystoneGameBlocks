///*
//-----------------------------------------------------------------------------
//This source file is part of OGRE
//    (Object-oriented Graphics Rendering Engine)
//For the latest info, see http://www.ogre3d.org/

//Copyright (c) 2000-2005 The OGRE Team
//Also see acknowledgements in Readme.html

//This program is free software; you can redistribute it and/or modify it under
//the terms of the GNU Lesser General Public License as published by the Free Software
//Foundation; either version 2 of the License, or (at your option) any later
//version.

//This program is distributed in the hope that it will be useful, but WITHOUT
//ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

//You should have received a copy of the GNU Lesser General Public License along with
//this program; if not, write to the Free Software Foundation, Inc., 59 Temple
//Place - Suite 330, Boston, MA 02111-1307, USA, or go to
//http://www.gnu.org/copyleft/lesser.txt.
//-----------------------------------------------------------------------------
//*/

//// Written by me :-)
////
//// -- Vincent Cantin (karmaGfa)

//#ifndef _BillboardChain_H__
//#define _BillboardChain_H__

//#include "OgrePrerequisites.h"

//#include "OgreSimpleRenderable.h"

//namespace Ogre {


//   /** Contains the data of an element of the BillboardChain.
//   */
//   class /*_OgreExport*/ BillboardChainElement
//   {

//   public:

//      BillboardChainElement();

//      BillboardChainElement(Vector3 position,
//                       Real width,
//                       Real uTexCoord,
//                       ColourValue colour);

//      Vector3 position;
//      Real width;
//      Real uTexCoord;
//      ColourValue colour;

//   };

//    /** Allows the rendering of a chain of connected billboards.
//    */
//   class /*_OgreExport*/ BillboardChain : public SimpleRenderable
//   {

//   public:

//      typedef std::vector<BillboardChainElement> BillboardChainElementList;

//      BillboardChain(int maxNbChainElements = 10);
//      virtual ~BillboardChain();

//        virtual void _notifyCurrentCamera(Camera* cam);
//      virtual Real getSquaredViewDepth(const Camera* cam) const;
//        virtual Real getBoundingRadius(void) const;

//      void setNbChainElements(unsigned int nbChainElements);
//      void setChainElement(unsigned int elementIndex, const BillboardChainElement& billboardChainElement);
//      void updateBoundingBox();

//   protected:

//      Real mRadius;

//      int mCurrentNbChainElements;
//      int mMaxNbChainElements;

//      BillboardChainElementList mChainElementList;

//      void updateHardwareBuffers();
//   };

//} // namespace

//#endif 

///*
//-----------------------------------------------------------------------------
//This source file is part of OGRE
//    (Object-oriented Graphics Rendering Engine)
//For the latest info, see http://www.ogre3d.org/

//Copyright (c) 2000-2005 The OGRE Team
//Also see acknowledgements in Readme.html

//This program is free software; you can redistribute it and/or modify it under
//the terms of the GNU Lesser General Public License as published by the Free Software
//Foundation; either version 2 of the License, or (at your option) any later
//version.

//This program is distributed in the hope that it will be useful, but WITHOUT
//ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
//FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

//You should have received a copy of the GNU Lesser General Public License along with
//this program; if not, write to the Free Software Foundation, Inc., 59 Temple
//Place - Suite 330, Boston, MA 02111-1307, USA, or go to
//http://www.gnu.org/copyleft/lesser.txt.
//-----------------------------------------------------------------------------
//*/

//// Written by me :-)
////
//// -- Vincent Cantin (karmaGfa)

////#include "OgreStableHeaders.h"
//#include "OgreBillboardChain.h"

//#include "OgreSimpleRenderable.h"
//#include "OgreHardwareBufferManager.h"
//#include "OgreNode.h"
//#include "OgreCamera.h"

//namespace Ogre {

//#define POSITION_BINDING      0
//#define DIFFUSE_COLOR_BINDING 1
//#define TEXCOORD_BINDING      2

//   BillboardChainElement::BillboardChainElement()
//   {
//   }

//   BillboardChainElement::BillboardChainElement(Vector3 _position,
//                                     Real _width,
//                                     Real _uTexCoord,
//                                     ColourValue _colour) :
//      position(_position),
//      width(_width),
//      uTexCoord(_uTexCoord),
//      colour(_colour)
//   {
//   }

//   BillboardChain::BillboardChain(int maxNbChainElements)
//    {
//      mRadius = 0.0f;
//      mCurrentNbChainElements = 0;
//      mMaxNbChainElements = maxNbChainElements;

//        mRenderOp.vertexData = new VertexData();
//        mRenderOp.indexData = NULL;
//        mRenderOp.vertexData->vertexCount = mCurrentNbChainElements * 2;
//        mRenderOp.vertexData->vertexStart = 0;
//        mRenderOp.operationType = RenderOperation::OT_TRIANGLE_STRIP;
//        mRenderOp.useIndexes = false;

//        VertexDeclaration* decl = mRenderOp.vertexData->vertexDeclaration;
//        VertexBufferBinding* bind = mRenderOp.vertexData->vertexBufferBinding;

//      // Add a description for the buffer of the positions of the vertices
//        decl->addElement(POSITION_BINDING, 0, VET_FLOAT3, VES_POSITION);

//      // Create the buffer
//        HardwareVertexBufferSharedPtr pVertexBuffer =
//            HardwareBufferManager::getSingleton().createVertexBuffer(
//            decl->getVertexSize(POSITION_BINDING),
//            mMaxNbChainElements * 2,
//            HardwareBuffer::HBU_STATIC_WRITE_ONLY);

//        // Bind the buffer
//        bind->setBinding(POSITION_BINDING, pVertexBuffer);

//      // Add a description for the buffer of the diffuse color of the vertices
//      decl->addElement(DIFFUSE_COLOR_BINDING, 0, VET_FLOAT4, VES_DIFFUSE);

//      // Create the buffer
//        HardwareVertexBufferSharedPtr pVertexColorBuffer =
//            HardwareBufferManager::getSingleton().createVertexBuffer(
//            decl->getVertexSize(DIFFUSE_COLOR_BINDING),
//            mMaxNbChainElements * 2,
//            HardwareBuffer::HBU_STATIC_WRITE_ONLY);

//        // Bind the buffer
//        bind->setBinding(DIFFUSE_COLOR_BINDING, pVertexColorBuffer);

//      // Add a description for the buffer of the texture coordinates of the vertices
//        decl->addElement(TEXCOORD_BINDING, 0, VET_FLOAT2, VES_TEXTURE_COORDINATES);

//      // Create the buffer
//        HardwareVertexBufferSharedPtr pTexCoordBuffer =
//            HardwareBufferManager::getSingleton().createVertexBuffer(
//            decl->getVertexSize(TEXCOORD_BINDING),
//            mMaxNbChainElements * 2,
//            HardwareBuffer::HBU_STATIC_WRITE_ONLY);

//        // Bind the buffer
//        bind->setBinding(TEXCOORD_BINDING, pTexCoordBuffer);

//        // set basic white material
//        this->setMaterial("BaseWhiteNoLighting");
//   }

//   BillboardChain::~BillboardChain()
//    {
//      delete mRenderOp.vertexData;
//   }

//    void BillboardChain::_notifyCurrentCamera(Camera* cam)
//    {
//      SimpleRenderable::_notifyCurrentCamera(cam);

//      updateHardwareBuffers();
//    }

//   Real BillboardChain::getSquaredViewDepth(const Camera* cam) const
//   {
//      Vector3 min, max, mid, dist;
//      min = mBox.getMinimum();
//      max = mBox.getMaximum();
//      mid = ((max - min) * 0.5) + min;
//      dist = cam->getDerivedPosition() - mid;

//      return dist.squaredLength();
//   }

//   Real BillboardChain::getBoundingRadius(void) const
//   {
//      return mRadius;
//   }

//   void BillboardChain::setNbChainElements(unsigned int nbChainElements)
//   {
//      mCurrentNbChainElements = nbChainElements;
//        mRenderOp.vertexData->vertexCount = mCurrentNbChainElements * 2;
//      mChainElementList.resize(mCurrentNbChainElements);
//   }

//   void BillboardChain::setChainElement(unsigned int elementIndex, const BillboardChainElement& billboardChainElement)
//   {
//      mChainElementList[elementIndex] = billboardChainElement;
//   }

//   void BillboardChain::updateBoundingBox()
//   {
//      if (mChainElementList.size() < 2)
//         return;

//      Real width = mChainElementList[0].width;
//      Vector3 widthVector = Vector3(width, width, width);
//      const Vector3& position = mChainElementList[0].position;
//      Vector3 minimum = position - widthVector;
//      Vector3 maximum = position + widthVector;

//      for (unsigned int i = 1; i < mChainElementList.size(); i++)
//      {
//         // Update the bounds of the bounding box
//         Real width = mChainElementList[i].width;
//         Vector3 widthVector = Vector3(width, width, width);
//         const Vector3& position = mChainElementList[i].position;
//         minimum.makeFloor(position - widthVector);
//         maximum.makeCeil(position + widthVector);
//      }

//      // Set the current bounding box
//      setBoundingBox(AxisAlignedBox(minimum, maximum));

//      // Set the current radius
//        mRadius = Math::Sqrt(std::max(minimum.squaredLength(), maximum.squaredLength()));
//   }

//   void BillboardChain::updateHardwareBuffers()
//    {
//      if (mChainElementList.size() < 2)
//         return;

//        // Setup the vertex coordinates

//      HardwareVertexBufferSharedPtr pPosBuffer =
//         mRenderOp.vertexData->vertexBufferBinding->getBuffer(POSITION_BINDING);

//        float* pPos = static_cast<float*>(pPosBuffer->lock(HardwareBuffer::HBL_DISCARD));

//      // Here. we need to compute the position of the camera in the coordinate system of the billboard chain.
//      Vector3 eyePos = mParentNode->_getDerivedOrientation().Inverse() *
//                   (m_pCamera->getDerivedPosition() - mParentNode->_getDerivedPosition());

//      // Compute the position of the vertices in the chain
//      unsigned int chainSize = mChainElementList.size();
//      for (unsigned int i = 0; i < chainSize; i++)
//      {
//         Vector3 chainTangent;
//         if (i == 0) chainTangent = mChainElementList[1].position - mChainElementList[0].position;
//         else if (i == chainSize - 1) chainTangent = mChainElementList[chainSize - 1].position - mChainElementList[chainSize - 2].position;
//         else chainTangent = mChainElementList[i + 1].position - mChainElementList[i - 1].position;

//         const Vector3& p1 = mChainElementList[i].position;

//         Vector3 vP1ToEye = eyePos - p1;
//         Vector3 vPerpendicular = chainTangent.crossProduct(vP1ToEye);
//         vPerpendicular.normalise();
//         vPerpendicular *= mChainElementList[i].width;

//         Vector3 pos0 = p1 - vPerpendicular;
//         Vector3 pos1 = p1 + vPerpendicular;

//         // Update the buffer with the 2 vertex positions
//         *pPos++ = pos0.x;
//         *pPos++ = pos0.y;
//         *pPos++ = pos0.z;
//         *pPos++ = pos1.x;
//         *pPos++ = pos1.y;
//         *pPos++ = pos1.z;
//      }

//        pPosBuffer->unlock();

//        // Setup the diffuse color of the vertex

//      HardwareVertexBufferSharedPtr pVertexColorBuffer =
//         mRenderOp.vertexData->vertexBufferBinding->getBuffer(DIFFUSE_COLOR_BINDING);

//        float* pColour = static_cast<float*>(pVertexColorBuffer->lock(HardwareBuffer::HBL_DISCARD));
//      for (unsigned int i = 0; i < mChainElementList.size(); i++)
//      {
//         ColourValue& col = mChainElementList[i].colour;
//         *pColour++ = col.r;
//         *pColour++ = col.g;
//         *pColour++ = col.b;
//         *pColour++ = col.a;
//         *pColour++ = col.r;
//         *pColour++ = col.g;
//         *pColour++ = col.b;
//         *pColour++ = col.a;
//      }
//        pVertexColorBuffer->unlock();

//        // Setup the texture coordinates

//      HardwareVertexBufferSharedPtr pTexCoordBuffer =
//         mRenderOp.vertexData->vertexBufferBinding->getBuffer(TEXCOORD_BINDING);

//        float* pTex = static_cast<float*>(pTexCoordBuffer->lock(HardwareBuffer::HBL_DISCARD));
//      for (unsigned int i = 0; i < mChainElementList.size(); i++)
//      {
//         *pTex++ = mChainElementList[i].uTexCoord;
//         *pTex++ = 0;
//         *pTex++ = mChainElementList[i].uTexCoord;
//         *pTex++ = 1;
//      }
//        pTexCoordBuffer->unlock();
//   }

//} 


//// Create the billboard chain
//      m_pBillboardChain = new BillboardChain(1000);
//      m_pBillboardChain->setMaterial("BillboardChainMaterial01");
//      m_pBillboardChain->setNbChainElements(1000);
//      for (int i = 0; i < 1000; i++)
//         m_pBillboardChain->setChainElement(i, BillboardChainElement(Vector3(Math::Sin(i / 100.0f * 2 * Math::PI),
//                                                            Math::Cos(i / 100.0f * 2 * Math::PI) ,
//                                                            i / 100.0f),
//                                                      0.1f,
//                                                      i / 10.0f,
//                                                      ColourValue(1.0f, 1.0, 1.0)));
//      m_pBillboardChain->updateBoundingBox();

//      // Add it to the scene
//      mSceneMgr->getRootSceneNode()->attachObject(m_pBillboardChain);
