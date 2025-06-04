using System;
using System.Drawing;
using System.IO;
using Microsoft.DirectX.Direct3D;
using MTV3D65;

/// <summary>
/// A near direct c# translation of JViper's vb.net LOD generation code.  I just modified the interface a bit and added a few overloads to 
/// simplify the useage of the class.   -Hypnotron Nov.15.2007
/// </summary>
public class MeshTool
{
    private TVMaterialFactory _matfac;
    private TVTextureFactory _texfac;
    private TVScene _scene;

    public MeshTool(TVScene scene, TVMaterialFactory matfac, TVTextureFactory texfac)
    {
        if (matfac == null || texfac == null || scene == null) throw new ArgumentNullException();
        _matfac = matfac;
        _texfac = texfac;
        _scene = scene;
    }

    public void SaveToX (string inTVMFilepath, bool loadTextures, bool loadMaterials, string outXFilepath, XFileFormat XFormat)
    {
        SaveToX(LoadTVMeshFromFile(inTVMFilepath, loadTextures , loadMaterials), outXFilepath, XFormat);
    }

    public void SaveToX(TVMesh TVMesh, string outXFilepath, XFileFormat XFormat)
    {
        TVInternalObjects TVIO = new TVInternalObjects();
        Mesh TmpD3DMsh = new Mesh(TVIO.GetD3DMesh(TVMesh.GetIndex()));

        int[] TmpAdj = new int[TVMesh.GetTriangleCount()*6];
        int[] Adj;

        TmpD3DMsh.GenerateAdjacency(0, TmpAdj);
        Mesh D3DMsh = Mesh.Clean(CleanType.Optimization, TmpD3DMsh, TmpAdj, out Adj);

        ExtendedMaterial[] MatLst = new ExtendedMaterial[TVMesh.GetGroupsNumber()];
        Material TmpMat = new Material();
        for (int i = 0; i < MatLst.Length; i++)
        {
            TmpMat.Ambient = TVColorToSystemColor(_matfac.GetAmbient(TVMesh.GetMaterial(i)));
            TmpMat.Diffuse = TVColorToSystemColor(_matfac.GetDiffuse(TVMesh.GetMaterial(i)));
            TmpMat.Emissive = TVColorToSystemColor(_matfac.GetEmissive(TVMesh.GetMaterial(i)));
            TmpMat.Specular = TVColorToSystemColor(_matfac.GetSpecular(TVMesh.GetMaterial(i)));
            TmpMat.SpecularSharpness = _matfac.GetPower(TVMesh.GetMaterial(i));
            MatLst[i].TextureFilename = _texfac.GetTextureInfo(TVMesh.GetTexture(i)).Filename;
            MatLst[i].Material3D = TmpMat;
        }

        D3DMsh.Save(outXFilepath, Adj, MatLst, XFormat);
        TmpD3DMsh.Dispose();
        D3DMsh.Dispose();
    }

    private Color TVColorToSystemColor(TV_COLOR color)
    {
        return Color.FromArgb((int) (color.a*255), (int) (color.r*255), (int) (color.g*255), (int) (color.b*255));
    }

    public TVMesh CreateLOD(string filepath, bool loadTextures, bool loadMaterials, float percentLOD, string name)
    {
        return CreateLOD(LoadTVMeshFromFile(filepath, loadTextures, loadMaterials), percentLOD, name);
    }

    private TVMesh LoadTVMeshFromFile(string filepath,bool loadTextures, bool loadMaterials)
    {
        TVMesh mesh = _scene.CreateMeshBuilder("");
        string ext = Path.GetExtension(filepath);

        if (ext.ToUpper() == ".X")
            mesh.LoadXFile(filepath, loadTextures, loadMaterials);
        else if (ext.ToUpper() == ".TVM")
            mesh.LoadTVM(filepath, loadTextures, loadMaterials);

        return mesh;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tvmesh"></param>
    /// <param name="percentLOD">Valid values must be BETWEEN 1.0 (full quality) and 0.0 (no quality)</param>
    /// <returns></returns>
    public TVMesh CreateLOD(TVMesh tvmesh, float percentLOD, string name)
    {
        int VrtNum;
        int FaceNum;

        if (percentLOD >= 1.0 || percentLOD <= 0.0) throw new ArgumentOutOfRangeException();
        
        
        TVInternalObjects TVIO = new TVInternalObjects();
        Device Dev = new Device(TVIO.GetDevice3D());
        Mesh TmpD3DMsh = new Mesh(TVIO.GetD3DMesh(tvmesh.GetIndex()));
        int[] Adj = new int[TmpD3DMsh.NumberFaces * 6];
        TmpD3DMsh.GenerateAdjacency(0, Adj);
        Mesh D3DMsh = Mesh.Clean(CleanType.Optimization | CleanType.Simplification, TmpD3DMsh, Adj, out Adj);
        ProgressiveMesh TmpPMsh = new ProgressiveMesh(D3DMsh, Adj, 1, MeshFlags.SimplifyFace);
        ProgressiveMesh PMsh =
            TmpPMsh.Clone(MeshFlags.SimplifyFace, VertexFormats.Position | VertexFormats.Normal | VertexFormats.Texture1,
                          Dev);

        // compute the desired numer of faces based on the percentage and set it to the pmesh which will immediately update the detail
        int desiredFaces = (int)(percentLOD * D3DMsh.NumberFaces);
        PMsh.NumberFaces = desiredFaces;
        FaceNum = GetPrgMeshFaceCount(PMsh);
        VrtNum = GetPrgMeshVrtCount(PMsh);

        if (FaceNum == D3DMsh.NumberFaces) throw new Exception("Mesh detail cannot be progressively scaled down for this model.");
        //{
        //    // I thought the following might serve as a workd around, but attempting to set the numberVertices instead of NumberFaces has no effect either.
        //    PMsh.NumberVertices = (int) percentLOD*D3DMsh.NumberVertices;
        //    FaceNum = GetPrgMeshFaceCount(PMsh);
        //    VrtNum = GetPrgMeshVrtCount(PMsh);
        //}


        AttributeRange[] Attr = PMsh.GetAttributeTable();

        CustomVertex.PositionNormalTextured[] VrtAry =
            (CustomVertex.PositionNormalTextured[])
            PMsh.LockVertexBuffer(typeof (CustomVertex.PositionNormalTextured), LockFlags.ReadOnly, PMsh.MaxVertices); // D3DMsh.NumberVertices); // PMsh.MaxVertices fixes the index out of bounds error for some meshes.  For some reason the maxVertices of the PMesh is higher than that of the original D3DMesh
        PMsh.UnlockVertexBuffer();

        ushort[] IndAry = (ushort[]) PMsh.LockIndexBuffer(typeof (ushort), LockFlags.ReadOnly, D3DMsh.NumberFaces*3);
        PMsh.UnlockIndexBuffer();

        TV_SVERTEX[] Vrt = new TV_SVERTEX[VrtNum];
        int[] VrtReMap = new int[IndAry.Length];
        int[] AttrId = new int[FaceNum];
        int[] Tex = new int[tvmesh.GetGroupCount()];
        int[] Mat = new int[tvmesh.GetGroupCount()];

        int TmpN = 0;
        for (int n = 0; n < Attr.Length; n++)
        {
            Tex[n] = tvmesh.GetTexture(n);
            Mat[n] = tvmesh.GetMaterial(n);
            for (int i = Attr[n].VertexStart; i < Attr[n].VertexStart + Attr[n].VertexCount; i++)
            {
                VrtReMap[i] = TmpN++;
            }
        }


        TmpN = 0;
        for (int n = 0; n < Attr.Length; n++)
            TmpN += Attr[n].FaceCount;

        int[] Ind = new int[TmpN*3];

        TmpN = 0;
        for (int n = 0; n < Attr.Length; n++)
        {
            for (int i = Attr[n].FaceStart; i < Attr[n].FaceStart + Attr[n].FaceCount; i++)
            {
                Ind[TmpN*3] = VrtReMap[IndAry[i*3]];
                Ind[TmpN * 3 + 1] = VrtReMap[IndAry[i * 3 + 1]];
                Ind[TmpN * 3 + 2] = VrtReMap[IndAry[i * 3 + 2]];
                AttrId[TmpN] = Attr[n].AttributeId;
                TmpN ++;
            }

            // note: having switched to PMsh.MaxVertices() in our LockIndexBuffer fixes the index out of range issue here.
            for (int i = Attr[n].VertexStart; i < Attr[n].VertexStart + Attr[n].VertexCount; i++)
            {
                Vrt[VrtReMap[i]].x = VrtAry[i].X;
                Vrt[VrtReMap[i]].y = VrtAry[i].Y;
                Vrt[VrtReMap[i]].z = VrtAry[i].Z;
                Vrt[VrtReMap[i]].nx = VrtAry[i].Nx;
                Vrt[VrtReMap[i]].ny = VrtAry[i].Ny;
                Vrt[VrtReMap[i]].nz = VrtAry[i].Nz;
                Vrt[VrtReMap[i]].tu = VrtAry[i].Tu;
                Vrt[VrtReMap[i]].tv = VrtAry[i].Tv;
            }
        }

        TVMesh lodresult = _scene.CreateMeshBuilder(name);
        lodresult.SetGeometry(Vrt, VrtNum, Ind, FaceNum, Attr.Length + 1, AttrId, true);
            
        for (int i = 0; i < Tex.Length; i++)
        {
            lodresult.SetMaterial(Mat[i], i);
            lodresult.SetTexture(Tex[i], i);
        }

        // TmpD3DMsh.Dispose();
        // D3DMsh.Dispose();
        TmpPMsh.Dispose();
        PMsh.Dispose();
        return lodresult;
    }

    private int GetPrgMeshFaceCount(ProgressiveMesh PMsh)
    {
        int n = 0;
        AttributeRange[] attr;

        attr = PMsh.GetAttributeTable();
        for (int i = 0; i < attr.Length; i++)
            n += attr[i].FaceCount;

        return n;
    }


    private int GetPrgMeshVrtCount(ProgressiveMesh PMsh)
    {
        int n = 0;
        AttributeRange[] attr;

        attr = PMsh.GetAttributeTable();
        for (int i = 0; i < attr.Length; i++)
            n += attr[i].VertexCount;

        return n;
    }
}