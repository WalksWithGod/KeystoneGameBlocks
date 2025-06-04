using System;
using System.Collections.Generic;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Types;
using MTV3D65;

namespace Keystone.Elements
{
	/// <summary>
	/// Model used to represent many different Entity records within an EntitySystem
	/// </summary>
	public class EntitySystemModel : Model 
	{
		public EntitySystemModel (string id): base (id)
		{
		}

		// Render() - is a PointSprite Mesh3d Renderer
		
		// if this Model contains individual particle positions array, then
		// - but then doesnt this mean that the Model has to  persist the particles?  I don't really want that...
		// id prefer then the Entity do it and that perhaps the Entity inherited ParticleSystem and so had .Particles method
		// that can be retreived by the Model here?
		// - but i dont know how a particle system maps to emitters..  Well, they cant map well to TV's because
		//   TV's emitters manage their own particles.  They're not individually accessible things.
		//  I think really 
		public override void Render(Keystone.Cameras.RenderingContext context, Keystone.Entities.Entity entity, Vector3d cameraSpacePosition, double elapsedSeconds, Keystone.FX.FX_SEMANTICS source)
		{
			if (entity is Keystone.Simulation.IEntitySystem == false) throw new ArgumentException ("EntitySystemModel.Render() - Entity must be of type IEntitySystem.");
			if (mGeometry == null || mGeometry.PageStatus != PageableNodeStatus.Loaded) return;
			
			Mesh3d mesh = (Mesh3d)this.Geometry;
			// radius can be relatively small at 10k because background3d nodes are always rendered first and then depth buffer is cleared
			double RADIUS = 10000; 
			
			Keystone.Simulation.DigestRecord[] records = ((Keystone.Simulation.IEntitySystem)entity).Records;
				
			for (int i = 0; i < records.Length; i++)
			{
				// TODO: include bool on record indicating if Translation is global?
				Vector3d position = Vector3d.Normalize (records[i].GlobalTranslation - context.Viewpoint.GlobalTranslation) * RADIUS;
				mesh.SetVertex (i, position);
			}
			
			base.Render (context, entity, cameraSpacePosition, elapsedSeconds, source);
			
			
			// TODO: our normal Model.Render() path will not calc individual axial rotations for each billboard element.
			//       - i think it's why ideally we should be doing that in shader... because adding that code here
			//         is lame...
			
		}
	}
    
}
