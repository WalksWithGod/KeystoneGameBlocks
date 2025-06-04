/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 12/20/2014
 * Time: 11:24 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Keystone.Immediate_2D
{
	/// <summary>
	/// Description of ShadowedText.
	/// </summary>
	public class ShadowedText
	{

//[01:18.08] <Watermanz> Ok, it's a bit complex apparently, but here comes :) :
//[01:18.58] <Watermanz> - I create an original font:
//[01:18.59] <Watermanz> fontORIG?#.TV.Text2D.TextureFont_Create'FontORIG' 'Arial Narrow'(7+3?FHEIGHTS)0 0 0 1 0 ? Source font
//[01:19.16] <Watermanz> - Then i grab it's texture:
//[01:19.16] <Watermanz> texORIG?#.TV.Text2D.TextureFont_GetTexture fontORIG
//[01:20.01] <Watermanz> - and get the texture size (which by comments seems to be 512 512):
//[01:20.01] <Watermanz> sz?(#.TV.Tex.GetTextureInfo texORIG).(Width Height) 
//[01:21.29] <Watermanz> - Then i create an array (empty, apparently), but of some reason i must go with GetUninitializedObject. This was a tricky thing to find out. TV3D lacks the type, or something:
//[01:21.31] <Watermanz> array?#.ByRef.New?256?#.Runtime.Serialization.FormatterServices.GetUninitializedObject #.TV.TV_TEXTUREFONT_CHAR
//[01:22.02] <Watermanz> - what i want to do is get the font metrics into that array:
//[01:22.05] <Watermanz> #.TV.Text2D.TextureFont_GetFontData fontORIG(#.ByRef.New 256)array
//[01:23.22] <Watermanz> Good, so far. Now i want to use that font data in a shader, to create the new font of my own, that has a transparency around each character. (This is the major point, i guess):
//[01:23.26] <Watermanz> surfFONT?#.TV.Scene.CreateAlphaRenderSurface sz,0
//[01:23.42] <Watermanz> texFONT?surfFONT.GetTexture
//[01:23.50] <Watermanz> (create a shader, bla bla)
//[01:24.24] <Watermanz> - then draw onto the newly created surface, using the previously grabbed texture as parameter:
//[01:24.24] <Watermanz> #.TV.TV2D.Draw_FullscreenQuadWithShader shaderFONT 0 0 1 1 texORIG
//[01:25.09] <Watermanz> - then create a static texture out of the rs (since the next call demands that):
//[01:25.10] <Watermanz> texHUDFONT?surfFONT.CreateStaticTextureFromRenderSurface sz,#.CONST_TV_COLORKEY.TV_COLORKEY_USE_ALPHA_CHANNEL
//[01:25.36] <Watermanz> - finally, voilá - create a new font from the shader-passed texture:
//[01:25.38] <Watermanz> font13GUI?#.TV.Text2D.TextureFont_CreateCustom'font13GUI'texHUDFONT 256 array
//[01:26.01] <Watermanz> - clean up:
//[01:26.02] <Watermanz>      surfFONT.Destroy
//[01:26.02] <Watermanz>      shaderFONT.Destroy
//[01:26.02] <Watermanz>      #.TV.Text2D.Font_Delete fontORIG
//[01:26.02] <Hypnotron> ah, so its all just a pre-process step and done once
//[01:26.13] <Watermanz> Well most CERTAINLY !!!
//[01:26.18] <Watermanz> Always cache etc ! :)
//[01:26.36] <Watermanz> BUT ofc i can re-do it any time, if i want
//[01:27.13] <Watermanz> - and the final step is to create my own 1-liner function, that i use to draw text with elsewhere in code:
//[01:27.14] <Watermanz> Draw13_GUI?{#.TV.Text2D.TextureFont_DrawText ?[2],(??),colGUI,font13GUI}
//[01:27.45] <Watermanz> ... so i say eg.:   Draw_13_GUI 'Hello world' 400 800
//[01:28.11] <Watermanz> - and it comes with each char surrounded with a nice, smooth transparency 
//[01:28.26] <Watermanz> ---- The shader is basically:
//[01:28.28] <Hypnotron> transparency?
//[01:28.49] <Watermanz> float4 PS_Main(VS_OUTPUT input) : COLOR
//[01:28.50] <Watermanz>     {
//[01:28.50] <Watermanz>     float4 col;
//[01:28.50] <Watermanz>     
//[01:28.50] <Watermanz>     float2 alpha;
//[01:28.50] <Watermanz>     float2 coord = input.TexCoord;
//[01:28.52] <Watermanz>     alpha.x = tex2D(samHUD, coord).a;
//[01:28.54] <Watermanz>     alpha.y = tex2D(samHUD, coord + 0.001953125).a;
//[01:28.56] <Watermanz>     alpha.y = max(alpha.y, tex2D(samHUD, coord - 0.001953125).a);
//[01:28.58] <Watermanz>     alpha.y = max(alpha.y, tex2D(samHUD, coord + float2(-0.001953125, 0.001953125)).a);
//[01:29.00] <Watermanz>     alpha.y = max(alpha.y, tex2D(samHUD, coord - float2(-0.001953125, 0.001953125)).a);
//[01:29.02] <Watermanz>     col.rgb = min(alpha.y - alpha.x, 0.5);
//[01:29.04] <Watermanz>     col.a = alpha.y;
//[01:29.06] <Watermanz>     return col;
//[01:29.08] <Watermanz>     }
//[01:29.10] <Watermanz> ---
//[01:29.16] <Watermanz> The whole point is to alter the outer edge of each character
//[01:29.19] <Hypnotron> is it easy to read the characters no matter what color background text is floating over?
//[01:29.39] <Watermanz> Well yeah, since the foont is outlined now
//[01:29.48] <Watermanz> bith re. color and alpha
//[01:29.56] <Watermanz> *both
//[01:30.28] <Watermanz> ofc, it may require a re-do if the conditions change (i don't re-do), and also some test runs to get it right
//[01:37.33] <Hypnotron> ok.  and that pixel shader is what basically redraws the texture font adding alpha along the edge... but not sure where you're adding an outline
//[01:38.42] <Watermanz> I think it may be since the rs has another background color, or likewise
//[01:39.01] <Watermanz> ie. takes a bit of that too, maybe
//[01:39.34] <Watermanz> the increased alpha around each character floods something
//[01:40.34] <Watermanz> and the outline increases the contrast of each char. Ie. if there is dark outside, and the font is black, i can still read it if it has a weak white outline
//[01:40.51] <Watermanz> (heh- gotta look at how it actually looks :))
//[01:42.10] <Watermanz> Go through the new pics in stormwind.fi media section!
//[01:42.37] <Watermanz> It's a black font with white outline - very fit for all sky colors and gradients without adjusting font color all time
//[01:48.32] <Hypnotron> thanks.  im going to implement this tomorrow.  plus i need to start tracking the rectangles of floating text to try and avoid overlapping them
//[01:48.40] <Watermanz> - One more thing to notice: There are small rs's under the screen symbols and texts. Ie. i draw the text only when something changes, onto a small surface. Only the surface is drawn onto main screen each loop cycle.
//[01:49.59] <Watermanz> Well ya, u know windows uses a depth (or order) for each window. Focus brings to front, etc. Maybe use a sort order for each small screen element/rectangle?
//[01:50.27] <Watermanz> Then u can re-draw the entire screen and maintain the correctness :)
	}
}
