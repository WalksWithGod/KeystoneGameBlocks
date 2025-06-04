using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.Celestial
{
    class Nebula
    {
        // Geonordo's volumetric fog using billboard particles
        // http://www.youtube.com/watch?v=C7zs8GTuyMs
        // TODO: a class such as this should make use of or inherit from a more generic
        // VolumetricCloud class.

        // if there is no startcatalog file in the archive, it can be generated using the existing systems xml
        // note also that this catalog may be extended to include basic data for planets so we can just use the catalog
        // for rendering our galaxy view and our solar system views as well.
        // For now actually lets just keep it to stars and get pickable stars working.
        // So as interim step, lets create some random stars and project them onto a sphere that is centered at the origin.
        // then we just need to translate all the stars each loop.
        //  

        private void Test()
        {     
            

            //MTV3D65.TVMiniMesh mini;
            //mini = new MTV3D65.TVMiniMesh ();
            ////mini.CreateBillboard(

            //MTV3D65.TVParticleSystem particles;
            //particles.CreateEmitter (MTV3D65.CONST_TV_EMITTERTYPE.TV_EMITTER_POINTSPRITE, starCount)

            //particles.SetEmitterShape(emitterId, CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME);

            //int texture = TV.TVTextureFactory.LoadTexture(Config.BasePath + "media\\demo\\particles\\psmoke.dds", "Particle", 640, 480, CONST_TV_COLORKEY.TV_COLORKEY_NO, true);
            //particles.SetPointSprite(emitterId, texture);

        }
        //Entities.ModeledEntity _catalog = new Entities.StaticEntity("starcatalog");

        //// below code is from ImportMinimesh which perhaps we should be using also

        //string name = Resource.Repository.GetNewName(typeof(Minimesh));
        //_resource = Minimesh.Create(name, FilePath, LoadTextures, LoadMaterials, out _app);
        //_resource.MaxInstancesCount = _maxInstancesCount;
        //((FX.FXInstanceRenderer)Core._Core.SceneManager.FXProviders[(int)FX.FX_SEMANTICS.FX_INSTANCER]).AddMinimesh(_resource);

        //for (int i = 0; i < _resource.MaxInstancesCount; i++ )
        //{
        //    // place a seperate new entity into the world
        //    StaticEntity newEntity = new StaticEntity(Resource.Repository.GetNewName(typeof(StaticEntity)), _resource.Model);
        //    newEntity.Enable = true;
        //    newEntity.Translation = _positions[i];
        //    newEntity.Scale = _scales[i]; // TODO: our scales are broken i think when computing bounding box so scaling anything will screw up culling
        //    newEntity.Rotation = _rotations[i];
        //    _parent.AddChild(newEntity);
        //}

        // add as child entities every single 

// READ IRC LOG FOR ZIRE from #GAMEDEV who makes Gods & Idolts
        // C:\Documents and Settings\Hypnotron\My Documents\logs\irc\Zire.EFNet.log
        // note the bit about rotating two textures would work great for things like "photon torpedo" fx as well.
        // AND
        // Ziggyware's Dyanmic sun shader
        //http://www.mathematik.uni-marburg.de/~menzel/index.php?seite=tutorials&id=1
        // http://www.youtube.com/watch?v=yiLeATHdMxk

// TODO: For rendering close up stars
//        The first thing I did in 3D was the solar system. I tried hard to make the sun as good looking as possible 
        // without heavy processing and found a very simple way. The NASA website has textures for most planets 
        // and bumpmaps for some of them, so you can make a pretty awesome solar system.

//1. Make yourself a texture from an image of the sun, like this one.
//http://cumbriansky.files.wordpress.com/2008/12/sun_021203.jpg
//Just cut a big square in it, as long as there's no black on it.

//2. Apply the texture as fully opaque on your big sphere and make the sphere rotate clock-wise.

//3. Create a second sphere, barely bigger than the previous one (it must not be noticeable to the eye)

//4. Apply the same texture, this time 50% opaque and make the sphere rotate counter clock-wise.

//You get yourself a pretty realistic sun. Okay, the north and south pole can look a little odd, depending on the 
        // camera that you use for you game. That's due to the cheap texture. You might need to make a better one if
        // your camera angle makes the flaw too obvious.

        

        ///////////////////////////////////////
        // Rendering A Large Volume Of Billboards

        //Hi all, bit of a long winded one but I am really screwed...
        //I am currently writing a game which uses a large number of billboards to simulate weapons fire, gas clouds, asteroids etc... 
        // (see screen1) in a 3D environment where standard rendering of models would just be pointless or too processor intensive for the final gain.

        //That said, I have set myself a relatively crazy goal of getting this thing to work on my 300Mhz, 160MB RAM, 32MB GeForce2 
        // stone age machine...

        //The general method that I am using to render the billboards is as follows...

        //1) Camera vFrom calculated from angles and zoom values. only occurs during rotation of the camera so speed not an issue.
        //2) during camera rotation an inverse camera rotation matrix is created. A vertex matrix is created:
        //Vertex1:X,Y,Z,S 'lower left
        //Vertex2:X,Y,Z,S 'etc...
        //Vertex3:X,Y,Z,S
        //Vertex4:X,Y,Z,S
        //this matrix is multiplied by the inverse camera rotation matrix to create a matrix of billboard vertices which is then used for 
        // all billboarding operations
        //3) all billboards of the same general type are batched into a single array of vertices using no world transformation and 
        // rendered in a single operation
        //4) repeat ad nausem...

        //Now this method worked fine for my screensaver (screen2) which was rendering about 3000-4000 billboards with no texture 
        // switching at just under 5 FPS whereas a method rendering each billboard individually was about 5-10x slower.
        //Unfortunately the expected speed increases for my game have been less than spectacular showing about a 5-7 frame rate drop
        // for each new object class that I add with massive framerate drops when I try to render 200 of them (Yet 1000 was no problem 
        // in the screensaver...)

        //This is basically the code I am using for each object class... can anyone see a way to increase the speed? I suspect the problem 
        // is not the billboarding but the continual adding/removing textures from the 3D card which I must do at least once per object class. 
        // In order for this method to work I need to use at least size 128x128 or 256x256 textures.

        //Is there a way to preset textures onto the 3D device so you dont have to continually texture switch and can just ask the device 
        // to render from preset texture 1 etc... using Dev.SetTexture 0, texture?


            //Code:
            //'*****************************************************
            //' Purpose: display an animated nebula field
            //'*****************************************************

            //Public Sub RenderNebulae()

            //Dim i As Integer
            //Dim n As Integer
            //Dim q As Integer

            //Dim X As Integer
            //Dim Z As Integer
            //Dim Xn As Integer
            //Dim Zn As Integer
            //Dim MaxX As Integer
            //Dim MaxZ As Integer

            //Dim tu As Single
            //Dim tv As Single
            //Dim nCol As Long
            //Dim Sx As Single

            //Dim Pos As D3DVECTOR
            //Dim Gas() As D3DLVERTEX

            //'get initial data
            //ReDim Gas(0)
            //MaxX = UBound(Map(), 1)
            //MaxZ = UBound(Map(), 2)

            //'cycle through adjacent squares
            //For Xn = -2 To 2
            //For Zn = -2 To 2

            //'check if square in visible range
            //If MapPos.MapX + Xn > 0 And MapPos.MapX + Xn < MaxX And _
            //MapPos.MapZ + Zn > 0 And MapPos.MapZ + Zn < MaxZ Then

            //'get co-ordinates
            //X = MapPos.MapX + Xn
            //Z = MapPos.MapZ + Zn

            //'create nebulae at location
            //If UBound(Map(X, Z).Gas()) > 0 Then GoSub SetupGasClouds
            //End If

            //Next Zn
            //Next Xn

            //'check if need to display
            //If UBound(Gas()) = 0 Then Exit Sub

            //'render the nebulae
            //GoSub SetupNebulaRenderer
            //GoSub DisplayNebulae

            //'clean up 3D settings
            //Dev.SetRenderState D3DRENDERSTATE_ALPHABLENDENABLE, False
            //Dev.SetRenderState D3DRENDERSTATE_ZWRITEENABLE, D3DZB_TRUE
            //Exit Sub

            //'initialize data lists to recieve nebulae
            //SetupGasClouds:

            //'check how much to display
            //q = UBound(Gas())
            //If Enable3DFX Then

            //'display inner and outer nebula
            //ReDim Preserve Gas((UBound(Map(X, Z).Gas()) * 12) + q - 1)

            //'cycle through nebulae
            //For i = 1 To UBound(Map(X, Z).Gas())
            //n = ((i - 1) * 12) + q
            //GoSub CreateNebulaVertices
            //Next i

            //Else
            //'display only outer portion
            //ReDim Preserve Gas((UBound(Map(X, Z).Gas()) * 6) + q - 1)

            //'cycle through nebulae
            //For i = 1 To UBound(Map(X, Z).Gas())
            //n = ((i - 1) * 6) + q
            //GoSub CreateNebulaVertices
            //Next i

            //End If
            //Return

            //'create the list of vertices
            //CreateNebulaVertices:

            //'1) animate the nebula
            //Map(X, Z).Gas(i).Anim = Map(X, Z).Gas(i).Anim + _
            //(Elapsed * Map(X, Z).Gas(i).AnimRate * Map(X, Z).Gas(i).AnimDir)

            //If Map(X, Z).Gas(i).Anim > 0.5 Then
            //Map(X, Z).Gas(i).AnimDir = -1
            //Map(X, Z).Gas(i).Anim = 0.5 - (Map(X, Z).Gas(i).Anim - 0.5)
            //ElseIf Map(X, Z).Gas(i).Anim < 0 Then
            //Map(X, Z).Gas(i).AnimDir = 1
            //Map(X, Z).Gas(i).Anim = 0 - Map(X, Z).Gas(i).Anim
            //End If

            //'2) get nebula world position relative to flagship
            //Pos.X = (X - MapPos.MapX) * 64
            //Pos.Z = (Z - MapPos.MapZ) * 64
            //dx3d.VectorAdd Pos, Pos, Map(X, Z).Gas(i).Pos 'get world position in voxel
            //dx3d.VectorSubtract Pos, Pos, MapPos.Pos 'subtract flagship offset

            //'get texture co-ordinates
            //Select Case Map(X, Z).Gas(i).Type
            //Case Is = 0 'bifid nebula
            //tu = 0
            //tv = 0
            //Case Is = 1 'trifid nebula
            //tu = 0.5
            //tv = 0
            //Case Is = 2 'emmission nebula
            //tu = 0
            //tv = 0.5
            //Case Is = 3 'reflection nebula
            //tu = 0.5
            //tv = 0.5
            //End Select

            //'setup 1st nebula face
            //Sx = Map(X, Z).Gas(i).Size * 1.5
            //nCol = dx3d.CreateColorRGBA(Colour(Map(X, Z).Gas(i).OCol).R * (Map(X, Z).Gas(i).Anim + 0.25), _
            //Colour(Map(X, Z).Gas(i).OCol).G * (Map(X, Z).Gas(i).Anim + 0.25), _
            //Colour(Map(X, Z).Gas(i).OCol).B * (Map(X, Z).Gas(i).Anim + 0.25), 1)

            //'create triangle 1
            //dx3d.CreateD3DLVertex (matBB.rc11 * Sx) + Pos.X, (matBB.rc12 * Sx) + Pos.Y, (matBB.rc13 * Sx) + Pos.Z, nCol, -1, 0 + tu, 0.5 + tv, Gas(0 + n)
            //dx3d.CreateD3DLVertex (matBB.rc21 * Sx) + Pos.X, (matBB.rc22 * Sx) + Pos.Y, (matBB.rc23 * Sx) + Pos.Z, nCol, -1, 0 + tu, 0 + tv, Gas(1 + n)
            //dx3d.CreateD3DLVertex (matBB.rc31 * Sx) + Pos.X, (matBB.rc32 * Sx) + Pos.Y, (matBB.rc33 * Sx) + Pos.Z, nCol, -1, 0.5 + tu, 0.5 + tv, Gas(2 + n)

            //'create triangle 2
            //dx3d.CreateD3DLVertex (matBB.rc21 * Sx) + Pos.X, (matBB.rc22 * Sx) + Pos.Y, (matBB.rc23 * Sx) + Pos.Z, nCol, -1, 0 + tu, 0 + tv, Gas(3 + n)
            //dx3d.CreateD3DLVertex (matBB.rc41 * Sx) + Pos.X, (matBB.rc42 * Sx) + Pos.Y, (matBB.rc43 * Sx) + Pos.Z, nCol, -1, 0.5 + tu, 0 + tv, Gas(4 + n)
            //dx3d.CreateD3DLVertex (matBB.rc31 * Sx) + Pos.X, (matBB.rc32 * Sx) + Pos.Y, (matBB.rc33 * Sx) + Pos.Z, nCol, -1, 0.5 + tu, 0.5 + tv, Gas(5 + n)

            //'check if drawing inner face
            //If Enable3DFX = False Then Return

            //'setup 2nd nebula face
            //Sx = Map(X, Z).Gas(i).Size
            //nCol = dx3d.CreateColorRGBA(Colour(Map(X, Z).Gas(i).ICol).R * (Map(X, Z).Gas(i).Anim + 0.25), _
            //Colour(Map(X, Z).Gas(i).ICol).G * (Map(X, Z).Gas(i).Anim + 0.25), _
            //Colour(Map(X, Z).Gas(i).ICol).B * (Map(X, Z).Gas(i).Anim + 0.25), 1)
            //'create triangle 1
            //dx3d.CreateD3DLVertex (matBB.rc11 * Sx) + Pos.X, (matBB.rc12 * Sx) + Pos.Y, (matBB.rc13 * Sx) + Pos.Z, nCol, -1, 0 + tu, 0.5 + tv, Gas(6 + n)
            //dx3d.CreateD3DLVertex (matBB.rc21 * Sx) + Pos.X, (matBB.rc22 * Sx) + Pos.Y, (matBB.rc23 * Sx) + Pos.Z, nCol, -1, 0 + tu, 0 + tv, Gas(7 + n)
            //dx3d.CreateD3DLVertex (matBB.rc31 * Sx) + Pos.X, (matBB.rc32 * Sx) + Pos.Y, (matBB.rc33 * Sx) + Pos.Z, nCol, -1, 0.5 + tu, 0.5 + tv, Gas(8 + n)

            //'create triangle 2
            //dx3d.CreateD3DLVertex (matBB.rc21 * Sx) + Pos.X, (matBB.rc22 * Sx) + Pos.Y, (matBB.rc23 * Sx) + Pos.Z, nCol, -1, 0 + tu, 0 + tv, Gas(9 + n)
            //dx3d.CreateD3DLVertex (matBB.rc41 * Sx) + Pos.X, (matBB.rc42 * Sx) + Pos.Y, (matBB.rc43 * Sx) + Pos.Z, nCol, -1, 0.5 + tu, 0 + tv, Gas(10 + n)
            //dx3d.CreateD3DLVertex (matBB.rc31 * Sx) + Pos.X, (matBB.rc32 * Sx) + Pos.Y, (matBB.rc33 * Sx) + Pos.Z, nCol, -1, 0.5 + tu, 0.5 + tv, Gas(11 + n)
            //Return

            //'set initial renderer properties
            //SetupNebulaRenderer:

            //'create world matrix
            //dx3d.IdentityMatrix matWorld
            //Dev.SetTransform D3DTRANSFORMSTATE_WORLD, matWorld

            //'select texture filtering mode
            //Dev.SetTextureStageState 0, D3DTSS_MAGFILTER, FilterMode
            //Dev.SetTextureStageState 0, D3DTSS_MINFILTER, FilterMode

            //'disable ambient lighting
            //Dev.SetRenderState D3DRENDERSTATE_LIGHTING, False
            //Dev.SetRenderState D3DRENDERSTATE_SPECULARENABLE, False

            //'turn off lights
            //For i = 1 To UBound(Lights())
            //Dev.LightEnable i, False
            //Next i

            //'enable alpha-blending
            //Dev.SetRenderState D3DRENDERSTATE_ALPHABLENDENABLE, True
            //Dev.SetRenderState D3DRENDERSTATE_SRCBLEND, D3DBLEND_ONE
            //Dev.SetRenderState D3DRENDERSTATE_DESTBLEND, D3DBLEND_ONE

            //'enable device color-keying (ColorKey is drawn transparent)
            //Dev.SetRenderState D3DRENDERSTATE_COLORKEYENABLE, True
            //Dev.SetRenderState D3DRENDERSTATE_COLORKEYBLENDENABLE, True

            //'set camera perspective and enable Z-buffer
            //Dev.SetTransform D3DTRANSFORMSTATE_VIEW, matView
            //Dev.SetRenderState D3DRENDERSTATE_ZENABLE, D3DZB_TRUE
            //Dev.SetRenderState D3DRENDERSTATE_ZWRITEENABLE, D3DZB_FALSE

            //'draw with filled triangles
            //Dev.SetRenderState D3DRENDERSTATE_FILLMODE, D3DFILL_SOLID

            //'Set the texture onto the D3D device
            //Dev.SetTexture 0, Tex(TEX_GAS)
            //Return

            //'render the nebula to the screen
            //DisplayNebulae:

            //'Begin rendering
            //Dev.BeginScene

            //'render nebulae onto screen
            //Dev.DrawPrimitive D3DPT_TRIANGLELIST, D3DFVF_LVERTEX, Gas(0), _
            //UBound(Gas()) + 1, D3DDP_DEFAULT

            //'Finished using D3D routines
            //Dev.EndScene
            //Return

            //End Sub
    }
}
