using System;
using Keystone.Entities;
using KeyCommon.Flags;

namespace Keystone.Elements
{
    //public class BackgroundModel3D : Model
    //{
        

    //    // TODO: Like individual particles, each sprite in PointSpriteCollection.cs can have
    //    // id, position, velocity, gravity and what else?

    //    // model flag should add BACKGROUND so this can be known to render first with zwrites off
    //    // the background entity ideally should have it's bounding volume always max with the region it's
    //    // in.  Similar to a dirlight that has no position, only a direction.
    //    // In that sense, a Background entity has no position, it's always relative to the camera 
    //    // when traversing a Background entity, the cull is skipped
    //    //
        

    //    // TODO: why not just add this to Model.cs? Do i really need a seperate class for this?
    //    public override void Render(Keystone.Entities.ModeledEntity entity, Keystone.Types.Vector3d cameraSpacePosition, Keystone.FX.FX_SEMANTICS source, float elapsedMilliseconds)
    //    {
    //        Keystone.Types.Vector3d position;

    //        if (mPositionMode == PositionMode.FIXED)
    //            position = cameraSpacePosition;
    //        else if (mPositionMode == PositionMode.RELATIVE_XYZ)
    //            position = Keystone.Types.Vector3d.Origin();
    //        else
    //        {
    //            position.x = 0;
    //            position.y = cameraSpacePosition.y;
    //            position.x = 0;
    //        }

    //        base.Render(entity, position, source, elapsedMilliseconds);
    //    }
    //}

    // TODO: HUD items as Foreground3D.cs?
    /// <summary>
    /// Basis for any type of Skybox, Skysphere, Starfield, Dust/Debris field bubble, etc
    /// and even Velocity HUD line indicators - http://www.youtube.com/watch?v=ZvwIlcH8Vbc
    /// 
    /// Background3D entities are similar to Viewpoints in that they are maintained as a stack.
    /// If the current viewpoint is in a region with a particular Background set, then that one is used.
    /// However if venturing into an inner region that has a new background, that background then gets pushed
    /// onto the stack of Backgrounds and is used instead.  
    /// A default background can be stored in SceneInfo.DefaultBackground so that it can be loaded immediately
    /// before any background paging of the full scene has started.
    /// A seperate Background can be placed under a root node which when the camera enters
    /// will be pushed onto the stack to override the default.
    /// 
    /// A Background3D has it's own traverser apply so that we ensure a bounding volume test is skipped.
    /// All that matters is it exists under the current camera's region
    /// 
    /// Picking of either faces of the background's model or pointsprites will occur through
    /// the picker as usual.  
    ///     -TODO: I how do i associate those picked pointsprites with stars?
    ///     a custom DomainObject perhaps...every pointsprite vertex index must correspond to a Star
    ///     id in xml.  That id can also be rendered as a label.  Also each star must have a global position
    ///     so that our script can update the stars to be in the "very near", "near" pointsprite mesh vs "medium", "far", "very far"
    ///     - so when generating our galaxy, this background data needs to be provided
    ///     - star position, star id, star temperature (so we can determine color), array position equates
    ///     to vertex index
    /// </summary>
    public class Background3D : Entities.ModeledEntity
    {
        private PositionMode mPositionMode = PositionMode.CAMERA_RELATIVE_XYZ;

        public enum PositionMode : byte
        {
            FIXED,         // does not track the camera on any axis
            CAMERA_RELATIVE_XZ,   // tracks the camera along x and z but is fixed at y axis
            CAMERA_RELATIVE_XYZ   // tracks the camera along all axis
        }

        public Background3D(string id) : base(id) 
        {
            mEntityFlags |= EntityAttributes.Background;
            Dynamic = false;
        }

        #region ITraversable Members
        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
        {
            // used by picking, not by renderer since Background is bindable node
            return target.Apply(this, data);
        }
        #endregion 

        // steps to implement
        // starting at camera region,
        //  traverse up through parent regions until we find a background node
        //  if none is found, use default.  if default is not defined use none.
        // if a background node is found, then we add it using the regionmatrix and perspective matrices
        // for the current region.  We add that as a VisibleItem with a position of 0,0,0 or
        // with y translation only multiplied by the current region's globalmatrix
        // 


        // a background entity has a bounding volume that matches the region it fits in.
        // I could force it to be background of Root node only.  
        // A background node could be a type of stack node like Viewpoint
        // that binds to RenderingContext as you traverse the scene.
        //
        //
        // ProceduralHelper perhaps can create this Background3D node which 
    
        // Perhaps the Model is what this should be... not 



        // TODO: perhaps rather than have a BackgroundModel3D 
        // we can override the translation so it always returns the 
        // context's position?  Well... no... this should be treated like a DirectionalLight
        // it has no position, it just has a region it exists under and there it is always
        // affecting.   Then only during Render of it's model, is it updated to be in the proper
        // location.  So now yes, maybe we get rid of BackgroundModel3d and we add the flags here
        // for PositionMode, and all Model's should check their PositionMode as well as whether
        // they are Background, Foreground, LargeFrustum, DefaultFrustum, Overlay, Text, etc
        // 

        /// <summary>
        /// Collision offered for faces or individual point sprites with a proxy Entity being offered
        /// for part of the collision result.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="worldMatrix"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public override Keystone.Collision.PickResults Collide(Keystone.Types.Vector3d start, Keystone.Types.Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            // normally 
            return base.Collide(start, end, parameters);
        }
    }
}
