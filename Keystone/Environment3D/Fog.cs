/*
 * Created by SharpDevelop.
 * User: Hypnotron
 * Date: 1/15/2015
 * Time: 5:03 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Keystone.Environment3D
{
	/// <summary>
	/// When to use a custom node and when to just use a scripted generic entity node?
	/// I think fog is generic enough to warrant it's own node.  When rendering
	/// this node when on the stack has to modify fog settings... the process for managing the stack
	/// and the rendering RegionPVS is not decided.
	/// </summary>
	public class Fog
	{
		public Fog()
		{
			// TODO: for atmospheric scattering shader, we want to apply fog gradient based on pixel height from horizon
			//       this way, we still get sky coloring higher up
			
//            _atmosphere.Fog_Enable (true);
//            _atmosphere.Fog_SetColor (1.0f, 1.0f, 1.0f);
//            // NOTE: shader semantic fogDensity is not set if LINEAR since simple LERP is expected at ratio between start and end distance
//            _atmosphere.Fog_SetType (CONST_TV_FOG.TV_FOG_LINEAR, CONST_TV_FOGTYPE.TV_FOGTYPE_PIXEL);
//            float fogstart = 160;
//            float fogEnd = fogstart * 4;
//            _atmosphere.Fog_SetParameters (fogstart, fogEnd);

//            // SetBackGroundColor(0.75f, 0.75f, 0.75f, 1.0f); <-- should set to same as fog color? or should never be necessary so long as no empty space in scene

//            // normally something like an "appearance" attribute gets applied to the mesh as we render... could an environmental node
//            // like fog behave similarly?
//           // during Render() if the environmental node at top of stack has changed, then we need to apply that Fog node...
		}
	}
}

// http://download.autodesk.com/us/maya/2010help/Nodes/envFog.html
// Env Fog is a volume object that fills the whole scene, from the near to the far clipping planes. 
// Env Fog is normally semi-transparent, and tends to hide other objects. You can adjust its
// attributes to change fog saturation, color, and how much of the scene it fills.
//
// There are two different fog simulations supported by this node. Simple Fog is the default, and 
// uses a small number of attributes to make a simple representation of fog.
//
//If you turn on the Physical Fog attribute, then EnvFog switches to using a more sophisticated, 
// physically-based model to simulate fog. This model is more complex to use, but the results can 
// be much better. You can use Physical Fog to simulate complete atmospheric environments, and you 
// can even use it for underwater scenes.
//
//Physical Fog works by simulating three things: the air (which has its attributes for its 
// light-scattering, density, transparency, etc), the water vapour (fog) in the air (which has its 
// own attributes for the same things), and optionally, a body of water which has its own light 
// scattering and other attributes. The combination of these effects creates a realistic fog.
//
//Aside from the attributes described here, the Env Fog node inherits some of the attributes of 
// its parent Light Fog node.
//
//In the table below, important attribtues have their names indicated in bold in the description column.
//
//This node is MP safe
//Node name	Parents	Classification	MFn type	Compatible function sets
//envFog	lightFog	shader/volume/fog	kEnvFogMaterial	kBase
//kNamedObject
//kDependencyNode
//kLightFogMaterial
//kEnvFogMaterial
//Related nodes
//
//lightFog, lightLinker, lightList
//Attributes (76)
//
//The following quick index only shows top-level attributes (too many attributes to show them all): 
// airColor (3), airDecay, airDensity, airLightScatter, airMaxHeight, airMinHeight, airOpacity (3), 
// blendRange, distanceClipPlanes, endDistance, fogAxis, fogColor (3), fogDecay, fogDensity, 
// fogFarDistance, fogLightScatter, fogMaxHeight, fogMinHeight, fogNearDistance, fogOpacity (3), 
// fogType, layer, matrixEyeToWorld, maxHeight, minHeight, physicalFog, planetRadius, pointCamera (3), 
// pointWorld (3), rayDirection (3), saturationDistance, startDistance, sunAzimuth, sunColor (3), 
// sunElevation, sunIntensity, useDistance, useHeight, useLayer, waterColor (3), waterDensity, 
// waterDepth, waterLevel, waterLightDecay, waterLightScatter, waterOpacity (3)

//Long name (short name)	Type	Default	Flags
//matrixEyeToWorld (e2w) 	fltMatrix	identity	outputinputconnectablestorablehidden
//	The transform to go from eye to world space
//pointCamera (p) 	float3	0.0, 0.0, 0.0	outputinputconnectablehidden
//	The current sample point that has to be shaded
//pointCameraX (px) 	float	0.0	outputinputconnectablehidden
//	The x component of the current sample position
//pointCameraY (py) 	float	0.0	outputinputconnectablehidden
//	The y component of the current sample position
//pointCameraZ (pz) 	float	0.0	outputinputconnectablehidden
//	The z component of the current sample position
//pointWorld (pw) 	float3	0.0, 0.0, 0.0	outputinputconnectablehidden
//	The current sample point that has to be shaded
//pointWorldX (pwx) 	float	0.0	outputinputconnectablehidden
//	The x component of the current sample position
//pointWorldY (pwy) 	float	0.0	outputinputconnectablehidden
//	The y component of the current sample position
//pointWorldZ (pwz) 	float	0.0	outputinputconnectablehidden
//	The z component of the current sample position
//rayDirection (r) 	float3	0.0, 0.0, 1.0	outputinputconnectablehidden
//	The current incident ray
//rayDirectionX (rx) 	float	0.0	outputinputconnectablehidden
//	The x component of the ray
//rayDirectionY (ry) 	float	0.0	outputinputconnectablehidden
//	The y component of the ray
//rayDirectionZ (rz) 	float	0.0	outputinputconnectablehidden
//	The z component of the ray
//distanceClipPlanes (dcp) 	enum	1	inputconnectablestorable
//	Distance Clip Planes specifies which set of clipping planes define where there is fog. envFog 
// can either be limited by the camera near and far clipping plane, or by the Fog Near and Far Distance.
//useLayer (ul) 	bool	false	outputinputconnectablestorable
//	When Use Layer is turned on, the density of the fog will be multiplied by the value of the Layer 
// parameter. Layer can be use to alter the degree of saturation of the fog. By connecting the output 
// of a 3d Texture to the Layer parameter, you can create texture in the fog. If Use Layer is turned 
// off, the Layer parameter will be ignored.
//
//This attribute only applies when Physical Fog is turned off.
//useHeight (uh) 	bool	false	outputinputconnectablestorable
//	When Use Height is turned on, the fog will not fill the whole scene from top to bottom. Instead, it will use the Min Height and Max Height attributes to determine where the fog begins and ends. These heights are measured in world space. When Use Height is turned off, the fog will fill the entire scene vertically.
//
//This attribute only applies when Physical Fog is turned off.
//blendRange (br) 	float	1.0	outputinputconnectablestorable
//	The distance/height at which blending starts
//
//This attribute only applies when Physical Fog is turned off.
//saturationDistance (sdt) 	float	100.0	outputinputconnectablestorable
//	Saturation Distance specifies the distance in camera space where the fog saturates (that is where the fog becames completely opaque).
//
//This attribute only applies when Physical Fog is turned off.
//fogNearDistance (fnd) 	float	0.0	outputinputconnectablestorable
//	Fog Near Distance is the distance in front of the camera where the fog should begin. This distance is defined in camera space.
//
//This attribute only applies when Distance Clip Planes is set to Fog Near/Far.
//fogFarDistance (ffd) 	float	200.0	outputinputconnectablestorable
//	Fog Far Distance is the distance in front of the camera where the fog should end. If you set this value to be less than Fog Near Distance, then the fog will start at Fog Near Distance, but extend out to the camera far clipping plane. This distance is defined in camera space.
//
//This attribute only applies when Distance Clip Planes is set to Fog Near/Far.
//layer (l) 	float	1.0	outputinputconnectablestorable
//	The Layer attribute is used as a multiplier to the fog's density. This attribute is ignored unless Use Layer is turned on.
//
//Connect a 3d texture to this attribute to give some texture to your fog or use it to decrease the amount of fog saturation.
//
//This attribute only applies when Physical Fog is turned off.
//minHeight (mnh) 	float	-1.0	outputinputconnectablestorable
//	Min Height is the height above which the fog starts. This attribute is ignored unless Use Height is turned on. Otherwise, fog fills the entire scene vertically. The height is defined in world space.
//
//This attribute only applies when Physical Fog is turned off and Use Height is turned on.
//maxHeight (mxh) 	float	1.0	outputinputconnectablestorable
//	Max Height is the height where the fog ends; above this height there is no fog.. This attribute is ignored unless Use Height is turned on. Otherwise, fog fills the entire scene vertically. The height is defined in world space.
//
//This attribute only applies when Physical Fog is turned off and Use Height is turned on.
//useDistance (ud) 	bool	false	outputinputconnectablestorable
//	Obsolete attribute.
//startDistance (sd) 	float	0.0	outputinputconnectablestorable
//	Obsolete attribute.
//endDistance (ed) 	float	-1.0	outputinputconnectablestorable
//	Obsolete attribute.
//physicalFog (sff) 	bool	false	outputinputconnectablestorablekeyable
//	Physical Fog. If this is turned off, Env Fog uses a simple fog model with just a few attributes specified. When Physical Fog is turned on, a more sophisticated physically-based model is used to simulate fog. A different set of attributes is used for this model.
//fogType (ftp) 	enum	0	outputinputconnectablestorablekeyable
//	Fog Type controls how the fog will be simulated in your scene. It has a number of different settings, depending on what kind of fog effect you want. This attribute is only available when Physical Fog is turned on. The settings are:
//
//    Uniform Fog: The fog has uniform density in all directions.
//
//    Atmospheric: The fog is thickest near the ground, and thins out (decays) as you move upwards.
//
//    Sky: This simulates the effect of a whole sky, with fog blending properly at the horizon, etc. Use this to provide a complete background to a foggy scene. It is especially useful for scenes where you can see a Tint32 distance, e.g. to the horizon.
//
//    Water: This simulates the effect of water, scattering light that is shining in from above it. This is useful for underwater shots, or for shots where objects are visible in a pool of water from above.
//
//    Water/Fog: Allows you to have water in your scene, with Uniform Fog above the water.
//
//    Water/Atmos: Allows you to have water in your scene, with Atmospheric Fog above the water.
//
//    Water/Sky: Allows you to have water in your scene, with the full Sky Fog simulation above it. 
//
//fogDensity (fdn) 	double	0.4	outputinputconnectablestorablekeyable
//	Fog Density controls the optical density of vapour in the fog layer. Increase this value to make the fog seem "thicker".
//
//This attribute is only available when Physical Fog is turned on.
//fogColor (fcl) 	float3		outputinputconnectablestorable
//	Fog Color controls the color of light that is scattered by the fog. When light passes through a medium such as air or fog, some frequencies (colors) pass directly through, while other frequencies are scattered. The frequencies that are scattered give the fog its distinctive color.
//
//This attribute is only available when Physical Fog is turned on.
//fogColorR (fcr) 	float	1.0	outputinputconnectablestorablekeyable
//	Fog Color Red value
//fogColorG (fcg) 	float	1.0	outputinputconnectablestorablekeyable
//	Fog Color Green value
//fogColorB (fcb) 	float	1.0	outputinputconnectablestorablekeyable
//	Fog Color Blue value
//fogOpacity (fop) 	float3		outputinputconnectablestorable
//	Fog Opacity controls what frequencies (colors) of light are absorbed as they pass through the fog. This will affect the way that objects in the background appear to be "tinted" by the fog.
//
//This attribute is only available when Physical Fog is turned on.
//fogOpacityR (for) 	float	0.5	outputinputconnectablestorablekeyable
//	Fog Opacity Red value
//fogOpacityG (fog) 	float	0.5	outputinputconnectablestorablekeyable
//	Fog Opacity Green value
//fogOpacityB (fob) 	float	0.5	outputinputconnectablestorablekeyable
//	Fog Opacity Blue value
//fogMinHeight (fmh) 	double	0.0	outputinputconnectablestorablekeyable
//	Fog Min Height controls the height of the bottom of the fog layer.
//
//This attribute is only available when Physical Fog is turned on, and Fog Type is not Uniform.
//fogMaxHeight (fxh) 	double	1.0	outputinputconnectablestorablekeyable
//	Fog Max Height controls the height of the fog layer.
//
//If the Fog Type is Atmospheric (or Water/Atmos) then the density of the fog decreases exponentially from Fog Min Height upwards. In this case, Fog Max Height is the height at which density has decreased to half the value it has at the fog base. (You might think of this as the half-life height)
//
//If the Fog Type is Sky (or Water/Sky) then Fog Max Height is the height of the top of the fog layer.
//
//This attribute is only available when Physical Fog is turned on, and Fog Type is not Uniform.
//fogDecay (fdc) 	double	0.2	outputinputconnectablestorablekeyable
//	Fog Decay controls the rate that the fog 'thins out' at higher altitudes. A value of 0.5 will make the fog decay linearly. Values between 0.5 and 1 will make the fog fairly even near the ground, then drop off suddenly at the top. Values between 0 and 0.5 will cause the fog to thin out very rapidly near its base.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is not Uniform.
//fogLightScatter (flc) 	double	1.0	outputinputconnectablestorablekeyable
//	Fog Light Scatter: This controls how evenly the light is scattered in the fog. A value of 1 means that the light is uniformly scattered, and will appear to be spread throughout the fog. Lower values will cause the fog to be brighter near the position of the sun.
//
//This attribute is only available when Physical Fog is turned on.
//airDensity (adn) 	double	0.0	outputinputconnectablestorablekeyable
//	Air Density controls the optical density of air our in the fog simulation. Increase this value to make the air seem more dense.
//
//This attribute is only available when Physical Fog is turned on.
//airColor (acl) 	float3		outputinputconnectablestorable
//	Air Color controls the color of light that is scattered by the air. When light passes through a medium such as air or fog, some frequencies (colors) pass directly through, while other frequencies are scattered. The frequencies that are scattered are what give the sky, for example, its distinctive color.
//
//This attribute is only available when Physical Fog is turned on.
//airColorR (acr) 	float	0.6	outputinputconnectablestorablekeyable
//	Air Color Red value
//airColorG (acg) 	float	0.8	outputinputconnectablestorablekeyable
//	Air Color Green value
//airColorB (acb) 	float	1.0	outputinputconnectablestorablekeyable
//	Air Color Blue value
//airOpacity (aop) 	float3		outputinputconnectablestorable
//	Air Opacity controls what frequencies (colors) of light are absorbed as they pass through the air. This will affect the way that objects in the background appear to be "tinted" by the air. (for example, objects on the horizon often appear bluish in real life).
//
//This attribute is only available when Physical Fog is turned on.
//airOpacityR (aor) 	float	0.37	outputinputconnectablestorablekeyable
//	Air Opacity Red value
//airOpacityG (aog) 	float	0.47	outputinputconnectablestorablekeyable
//	Air Opacity Green value
//airOpacityB (aob) 	float	0.9	outputinputconnectablestorablekeyable
//	Air Opacity Blue value
//airMinHeight (amh) 	double	0.0	outputinputconnectablestorablekeyable
//	Air Min Height controls the height of the bottom of the air layer.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is not Uniform.
//airMaxHeight (axh) 	double	50.0	outputinputconnectablestorablekeyable
//	Air Max Height controls the height of the air layer.
//
//If the Fog Type is Atmospheric (or Water/Atmos) then the density of the air decreases exponentially from Air Min Height upwards. In this case, Air Max Height is the height at which air density has decreased to half the value it has at Air Min Height. (You might think of this as the half-life height)
//
//If the Fog Type is Sky (or Water/Sky) then Air Max Height is the height of the top of the air layer.
//
//This attribute is only available when Physical Fog is turned on, and Fog Type is not Uniform.
//airDecay (adc) 	double	0.1	outputinputconnectablestorablekeyable
//	Air Decay controls the rate that the air 'thins out' at higher altitudes. A value of 0.5 will make the air decay linearly. Values between 0.5 and 1 will make the air fairly even near the ground, then drop off suddenly at the top. Values between 0 and 0.5 will cause the air to thin out very rapidly near the ground.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is not Uniform.
//airLightScatter (alc) 	double	1.0	outputinputconnectablestorablekeyable
//	Air Light Scatter: This controls how evenly the light is scattered in the air. A value of 1 means that the light is uniformly scattered, and will appear to be spread throughout the air. Lower values will cause the atmosphere to appear brighter around the sun position.
//
//This attribute is only available when Physical Fog is turned on.
//waterDensity (wdn) 	double	0.0	outputinputconnectablestorablekeyable
//	Water Density controls the optical density of water in the fog simulation. Increase this value to make the 'underwater fog' seem more dense.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is set to one of the Water types.
//waterColor (wcl) 	float3		outputinputconnectablestorable
//	Water Color controls the color of light that is scattered by the water. When light passes through a medium such as air or water, some frequencies (colors) pass directly through, while other frequencies are scattered. The frequencies that are scattered are what give the water its distinctive color.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is set to one of the Water types.
//waterColorR (wcr) 	float	0.6	outputinputconnectablestorablekeyable
//	Water Color Red value
//waterColorG (wcg) 	float	0.8	outputinputconnectablestorablekeyable
//	Water Color Green value
//waterColorB (wcb) 	float	1.0	outputinputconnectablestorablekeyable
//	Water Color Blue value
//waterOpacity (wop) 	float3		outputinputconnectablestorable
//	Water Opacity controls what frequencies (colors) of light are absorbed as they pass through the water. This will affect the way that objects in the background appear to be "tinted" by the water.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is set to one of the Water types.
//waterOpacityR (wor) 	float	0.37	outputinputconnectablestorablekeyable
//	Water Opacity Red value
//waterOpacityG (wog) 	float	0.47	outputinputconnectablestorablekeyable
//	Water Opacity Green value
//waterOpacityB (wob) 	float	0.9	outputinputconnectablestorablekeyable
//	Water Opacity Blue value
//waterLevel (wlv) 	double	0.0	outputinputconnectablestorablekeyable
//	Water Level is the height of the surface of the water.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is set to one of the Water types.
//waterDepth (wdp) 	double	50.0	outputinputconnectablestorablekeyable
//	Water Depth is the depth of the water in the scene. Increase this to make the water deeper.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is set to one of the Water types.
//waterLightDecay (wdc) 	double	2.0	outputinputconnectablestorablekeyable
//	Water Light Decay controls the rate that the illumination drops off with depth.
//
//Set this to 0 if you don't want the light to decay (that is, you don't want it to get darker as you go deeper). A setting of 1.0 will give a linear decay, and higher values will make it get darker faster.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is set to one of the Water types.
//waterLightScatter (wlc) 	double	1.0	outputinputconnectablestorablekeyable
//	Water Light Scatter: This controls how evenly the light is scattered in the water. A value of 1 means that the light is uniformly scattered, and will appear to be spread throughout the water. Lower values will cause the water to appear brighter near the surface, where the light is coming from.
//
//This attribute is only available when Physical Fog is turned on and Fog Type is set to one of the Water types.
//planetRadius (prd) 	double	1000.0	outputinputconnectablestorablekeyable
//	Planet Radius is a number used when calculating atmospheric effects that depend on the curvature of the atmosphere. Increasing this value makes the planet "larger", effectively making the atmosphere flatter, and pushing the horizon further away.
//
//This attribute is only available when Physical Fog is turned on, and the Fog Type is not Uniform.
//fogAxis (fax) 	enum	0	outputinputconnectablestorablekeyable
//	Fog Axis controls the direction that the fog simulation will use for "up". You can choose any of the X, Y, or Z axes in the positive or negative direction.
//
//This attribute is only available when Physical Fog is turned on, and the Fog Type is not Uniform.
//sunIntensity (sin) 	double	1.0	outputinputconnectablestorablekeyable
//	Sun Intensity controls the overall brightness of the fog.
//
//This attribute is only available when Physical Fog is turned on, and the Fog Type is not Uniform.
//sunAzimuth (saz) 	double	0.0	outputinputconnectablestorablekeyable
//	Sun Azimuth controls the position of the sun along an imaginary circle that goes around the Fog Axis.
//
//This attribute is only available when Physical Fog is turned on, and the Fog Type is not Uniform.
//sunElevation (sel) 	double	45.0	outputinputconnectablestorablekeyable
//	Sun Elevation controls the position of the sun along a line from the horizon to directly overhead. A value of 90 puts the sun directly above (actually, directly in the position pointed to by the Fog Axis) and a value of 0 puts the sun at the horizon.
//
//This attribute is only available when Physical Fog is turned on, and the Fog Type is not Uniform.
//sunColor (snc) 	float3		outputinputconnectablestorable
//	Sun Color controls the color of the sunlight illuminating the fog.
//
//This attribute is only available when Physical Fog is turned on, and the Fog Type is not Uniform.
//sunColorR (snr) 	float	1.0	outputinputconnectablestorablekeyable
//	Sun Color Red value
//sunColorG (sng) 	float	1.0	outputinputconnectablestorablekeyable
//	Sun Color Green value
//sunColorB (snb) 	float	1.0	outputinputconnectablestorablekeyable
//	Sun Color Blue value 