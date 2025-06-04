using System;
using Keystone.Shaders;
using Microsoft.DirectX.Direct3D;
using MTV3D65;

namespace Keystone
{
    // translated from jviper's code posted here
    // http://www.truevision3d.com/forums/tv3d_sdk_65/tv3d_and_cal3d-t18579.0.html;msg127627
    class BatchList
    {

        VertexBuffer D3DVrtBuff;
        IndexBuffer D3DIndBuff;
        private Shader refShader;

        //public void Test()
        //{
        //    Device D3DDev = new Device(Core._CoreClient.Internals.GetDevice3D());  //Pass DirectX Device Pointer to DirectX Device contructor to retrieve the DirectX Device TV3D is currently using.
        //    D3DDev.TestCooperativeLevel();  //Check whether the DirectX Device has not been lost. Usually if a DirectX Device is lost because the window has bee re-sized. If this is the case, we skip the rendering 
            
            
        //    try
        //    {
        //    //Now  we create our DirectX instances of our Vertex and Index buffers
        //        D3DVrtBuff = new VertexBuffer(GetType(Typ), VertexArray.Length, D3DDev, Usage.Dynamic, Vertex.D3DFormat, Pool.Default);
        //        D3DIndBuff = new IndexBuffer(GetType(int), IndexArray.Length, D3DDev, Usage.Dynamic, Pool.Default);
        //        D3DVrtBuff.SetData(VertexArray, 0, LockFlags.Discard);
        //        D3DIndBuff.SetData(IndexArray, 0, LockFlags.Discard);

        //    if (D3DIndBuff != null && D3DVrtBuff != null) 
        //    {
        //        D3DDev.Indices = D3DIndBuff;
        //        D3DDev.VertexFormat = VrtFormats ;
        //        D3DDev.SetStreamSource(0, D3DVrtBuff, 0, SizeOf(GetType(Vrt)));
        //        //Here we passed the size of your Vertex Type in bytes
        //        //The SizeOf function comes from the System.Runtime.InteropServices.Marshal namespace.

        //        //Now we are ready to render 
        //        //Here we can render with DirectX fixed pipeline, or with a shader
        //        //If you pass a valid shader to this function, it will render with the shader
        //        if (refShader != null)
        //        {
        //            //!!!!!!IMPORTANT!!!!!!
        //            //MAKE SURE YOU UPDATE THE SEMANTICS FOR YOUR SHADER!
        //            //You'll be using alot of TVShader.SetEffectParam..... functions.
        //            for (int pn = 0; pn < refShader.TVShader.GetPassCount(); pn++)
        //            {
        //                Core._CoreClient.Internals.Shader_Begin(refShader.TVShader, pn);
        //                D3DDev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexArray.Length, 0, (int)(IndexArray.Length/3));
        //                Core._CoreClient.Internals.Shader_End(refShader.TVShader);
        //            }
        //        }
        //        else
        //        {
        //            // Otherwise we'll just render using fixed pipeline
        //            D3DDev.RenderState.Lighting = true;
        //            D3DDev.RenderState.SpecularEnable = true;
        //            D3DDev.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexArray.Length, 0, (int)(IndexArray.Length / 3));
        //        }
        //        D3DDev.Indices = null;
        //        D3DDev.SetStreamSource(0, null, 0);
        //    }
        //    }
        //    catch
        //    {
        //        if (D3DVrtBuff != null) D3DVrtBuff.Dispose();
        //        D3DVrtBuff = null;
        //        if (D3DIndBuff != null)  D3DIndBuff.Dispose() ;
        //        D3DIndBuff = null;
        //        if (D3DDev != null) D3DDev = null; 
        //    }
        //}
    }
}
