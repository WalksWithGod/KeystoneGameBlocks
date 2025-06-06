/*********************************************************************NVMH3****
$Revision: #3 $

Copyright NVIDIA Corporation 2007
TO THE MAXIMUM EXTENT PERMITTED BY APPLICABLE LAW, THIS SOFTWARE IS PROVIDED
*AS IS* AND NVIDIA AND ITS SUPPLIERS DISCLAIM ALL WARRANTIES, EITHER EXPRESS
OR IMPLIED, INCLUDING, BUT NOT LIMITED TO, IMPLIED WARRANTIES OF MERCHANTABILITY
AND FITNESS FOR A PARTICULAR PURPOSE.  IN NO EVENT SHALL NVIDIA OR ITS SUPPLIERS
BE LIABLE FOR ANY SPECIAL, INCIDENTAL, INDIRECT, OR CONSEQUENTIAL DAMAGES
WHATSOEVER (INCLUDING, WITHOUT LIMITATION, DAMAGES FOR LOSS OF BUSINESS PROFITS,
BUSINESS INTERRUPTION, LOSS OF BUSINESS INFORMATION, OR ANY OTHER PECUNIARY
LOSS) ARISING OUT OF THE USE OF OR INABILITY TO USE THIS SOFTWARE, EVEN IF
NVIDIA HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

    This file, to be #included in HLSL FX effects, will create
	a 3D (volume) noise texture using the DirectX Virtual machine.


To learn more about shading, shaders, and to bounce ideas off other shader
    authors and users, visit the NVIDIA Shader Library Forums at:

    http://developer.nvidia.com/forums/

******************************************************************************/

#ifndef _H_NOISE3D
#define _H_NOISE3D

#ifndef NOISE_SCALE
#define NOISE_SCALE 5
#endif /* NOISE_SCALE */

// predefine as 1 for "pure" noise
#ifndef NOISE3D_LIMIT
#define NOISE3D_LIMIT 256
#endif /* NOISE3D_LIMIT */

// function used to fill the volume noise texture
float4 noise_3d(float3 Pos : POSITION) : COLOR
{
    float4 Noise = (float4)0;
    for (int i = 1; i < NOISE3D_LIMIT; i += i) {
        Noise.r += (noise(Pos * NOISE_SCALE * i)) / i;
        Noise.g += (noise((Pos + 1)* NOISE_SCALE * i)) / i;
        Noise.b += (noise((Pos + 2) * NOISE_SCALE * i)) / i;
        Noise.a += (noise((Pos + 3) * NOISE_SCALE * i)) / i;
    }
    return (Noise+0.5);
}

#ifndef NOISE_VOLUME_SIZE
#define NOISE_VOLUME_SIZE 32
#endif /* NOISE_VOLUME_SIZE */

texture NoiseTex  <
    string TextureType = "VOLUME";
    string function = "noise_3d";
	string UIName = "3D Noise Texture";
    string UIWidget = "None";
    int width = NOISE_VOLUME_SIZE, height = NOISE_VOLUME_SIZE, depth = NOISE_VOLUME_SIZE;
>;

// samplers
sampler3D NoiseSamp = sampler_state 
{
    texture = <NoiseTex>;
    AddressU  = Wrap;        
    AddressV  = Wrap;
    AddressW  = Wrap;
    MinFilter = Linear;
    MipFilter = Linear;
    MagFilter = Linear;
};

#define NOISE3D(p) tex3D(NoiseSamp,(p))
#define SNOISE3D(p) (NOISE3D(p)-0.5)

#endif /* _H_NOISE3D */

//////////////////////////////////////////////////// eof ///
