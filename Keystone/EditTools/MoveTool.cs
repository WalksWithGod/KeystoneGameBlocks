using System;
using Keystone.Collision;
using Keystone.Commands;
using Keystone.EditOperations;
using System.Collections.Generic;
using Keystone.Events;
using Keystone.Types;
using Keystone.Controllers ;
using Keystone.Controls;
using System.Diagnostics;

namespace Keystone.EditTools
{
    
    /// <summary>
    /// Unlike Selection Tool which can be used when in NON Edit mode, move select and defaults with rendering the
    /// move widget.
    /// </summary>
    public class MoveTool : TransformTool
    {
        
        public MoveTool(Keystone.Network.NetworkClientBase netClient)
            : base(netClient)
        {
            mActiveMode = TransformationMode.TranslationAxis;

            mControl = EditTools.Widgets.LoadTranslationWidget();

            mControl.MouseEnter += OnMouseEnter;
            mControl.MouseLeave += OnMouseLeave;
            mControl.MouseDown += OnMouseDown;
            mControl.MouseUp += OnMouseUp;
            mControl.MouseClick += OnMouseClick;
            mControl.MouseDrag += OnMouseDrag;

            // keep this control in memory
            Resource.Repository.IncrementRef(mControl);
            
            // TODO: where is this control added to the HUD3DRoot? I think in the Hud instance for that workspace (i.e FloorplanHud.cs or EditorHud.cs)


            // NOTE: following LoadTVResourceSynchronously() not required because this widget
            // is rendered as Retained HUD element, not Immediate... right?
            // Immediate Rendered Hud items must load pageable resources manually
            // Keystone.IO.PagerBase.LoadTVResourceSynchronously (mAxisIndicator);
        }

		protected override void OnMouseUp(object sender, EventArgs args)
		{
			Transform (ComponentTranslation);
			base.OnMouseUp(sender, args);
		}

        private void Transform (Keystone.Types.Vector3d position)
        {
      
        	if (mSource.Parent is Keystone.Portals.Interior)
        	{
        		Keystone.Portals.Interior celledRegion = (Keystone.Portals.Interior)mSource.Parent;
        		// translation of interior component must be governed by footprint validation
        		int[,] footprint = null;
        		if (mSource.Footprint != null) footprint = mSource.Footprint.Data ;
                int[,] destFootprint = null;
                bool isPlaceable = celledRegion.IsChildPlaceableWithinInterior(footprint, position, mSource.Rotation, out destFootprint);
	
	            if (isPlaceable == false) 
	            {
	            	System.Diagnostics.Debug.WriteLine ("MoveTool.Transform() - Not Placeable.");
	            	// TODO: this preview does not update the original on disk!  thus when command goes over wire
	            	//        we end up reloading from the disk version that has no footprint set and we do another
	            	//        placemenet validity test for security and that fails.  so what option now?
	            	//        - further, how does that affect resizing these autogen components?!
	            	return;
	            }
        	}

        	// normally to move an object, you'd just modify the property... but there is a case where
        	// transforms need bounds checking and/or footpring checking and so we either need to special case
        	// the handling for those properties or we have a seperate command.  I think a seperate command would
        	// just make it confusing
        	SendCommand (mSource.ID, "position", typeof(Vector3d), position);
    	}
        
        // EDITABLE MESH
        //        case EventType.MouseDown:
        //            // TODO: should we filter left button here? or in EditController?
        //            if (mPickResult.HasCollided && mouse.Button == Keystone.Enums.MOUSE_BUTTONS.XAXIS)
        //            {
        //                if (mPickResult.CollidedObjectType == CollidedObjectType.EditableMesh)
        //                {
        //                    // in sketchup, after moving the program recomputes new faces if moving results in some faces now intersecting others
        //                    // that's nasty :(  I'm not sure how it manages that.  I suppose it just treats all intersections as resulting in faces
        //                    // That's simple enough but how does it determine when a new intersection has occured or stopped occuring?
        //                    // I guess if moving a vertex(s) or edge(s) is resulting in that and other lines from "changing" then those lines are retested

        //                    // But who cares, for now we're just going to   
        //                    // pass the ID and begin the move commands

        //                    // just because we're attempting to select with the move tool does not mean there is anything to move
        //                    // and just because we have geometry (vertex, edge) that is near the cursor does not mean it's close enough
        //                    // to be auto highlighted.

        //                    if (mPickResult.EdgeID > -1)
        //                    {
        //                        Entity = mPickResult.Entity;
        //                        Target = (EditDataStructures.EditableMesh)mPickResult.Geometry;
                                
        //                        // TODO: I've removed Matrix property from Geometry.  
        //                        // regular Mesh3d, Actor3d, etc are fine but what did i use it
        //                        // for here on EditableMesh?
        //                        //Target.Matrix = pickResult.Entity.Matrix;
                                
        //                        // determine whether we're going to be moving an edge, vertex or face.  Depending on orthozoom
        //                        // the distance in screenspace from mouse to a geometry element varies... 
        //                        //AddVertex(pickResult.);
        //                        AddEdge((uint)mPickResult.EdgeID);
        //                        //AddFace();

        //                        CurrentViewport = mouse.Viewport;
        //                        _hasInputCapture = true;
        //                        Activate();
        //                        mManipulator.Activate(mouse.Viewport.Context.Scene);
        //                    }
        //                    else
        //                    {
        //                        DeActivate();
        //                        mManipulator.Deactivate();
        //                        return;
        //                    }
        //                }
        //               

        //        case EventType.MouseMove:


        //            Vector3d  mouseProjectedCoords = (Vector3d )((MouseEventArgs)args).UnprojectedPosition ;

        //            if (mPickResult.CollidedObjectType == CollidedObjectType.EditableMesh)
        //            {
        //                // the unprojected mouse coords are in world coords and need to be translated to model space
        //                // because the primitive's vertices and info are all in model space
        //                Matrix m = _entity.LocalMatrix;
        //                Vector3d position = new Vector3d(m.M41, m.M42, m.M43);
        //                mouseProjectedCoords -= position;
        //                //  what we want here is for the center of this line to move to where the mouse is
        //                // and then compute the difference between it's original starting position and the current mouse
        //                // and use that to translate the two endpoints
        //                Vector3d translation;

        //                translation = mouseProjectedCoords - _primitives[0].Center;//new Vector3d (deltaX * translationRate , 0, -deltaY * translationRate);
        //                translation.y = 0; // keep this axis flat for top/bottom ortho views

        //                // TODO: make sure _mesh.GetVertex() has "false" param so we grab modelspace coords
        //                // once we modify translation to use modelspace coords
        //                // TODO: i should backup this section of code once i start to use the matrixinverse
        //                // but actually i dont believe i need any scaling or rotation cuz scaling/rotation geometry
        //                // doesnt maintain any matrix, its actual modification of the verts
        //                // so this simplifies things.
        //                // in sketchup, only full components can have seperate scaling and rotation matrices
        //                // and that means those would go in the ModelBase or EntityBase (i forget which atm)

        //                // convert a ray to model space.  above we want to convert the 3d world pick position to model space
        //                // so that should mean we transform the unprojected mouse pick coord by the inverse of the 
        //                // composite entity Matrix
        //                //D3DXMatrixInverse(&matInv, NULL, &m_FloorMesh.m_matComposite);
        //                //D3DXVec3TransformCoord(&vPickRayOrig_tmp, &m_vPickRayOrig, &matInv);
        //                //D3DXVec3TransformNormal(&vPickRayDir_tmp, &m_vPickRayDir, &matInv);

        //                for (int i = 0; i < _primitives.Count; i++)
        //                {
        //                    PrimitiveInfo info;
        //                    info.ID = _primitives[i].ID;
        //                    info.Type = _primitives[i].Type;
        //                    info.OriginalPosition = _primitives[i].OriginalPosition; // frak this is in world position and to use "Move" in the MoveOp it wants local
        //                    info.Center = _primitives[i].Center + translation;
        //                    info.Position = new Vector3d[_primitives[i].Position.Length];
        //                    for (int j = 0; j < _primitives[i].Position.Length; j++)
        //                    {
        //                        info.Position[j] = _primitives[i].Position[j] + translation; // this is in world, needs local coords for MoveOp
        //                    }

        //                    _primitives[i] = info;

        //                    // locally we're going to directly move the verts but also we're going to create a command
        //                    // that will both be serialized and sent to the server and added to the undo stack
        //                    if (_primitives[i].Type == PrimitiveType.Vertex)
        //                        _mesh.TranslateVertex(_primitives[i].ID, translation);
        //                    else if (_primitives[i].Type == PrimitiveType.Edge)
        //                    {
        //                        _mesh.TranslateEdge(_primitives[i].ID, translation);
        //                    }
        //                    else if (_primitives[i].Type == PrimitiveType.Face)
        //                        _mesh.TranslateFace(_primitives[i].ID, translation);
        //                    else
        //                        throw new Exception("Unsupported primitive type.");
        //                }
        //            }


        //        case EventType.MouseUp :
        //            // compute final positions for all and create a command that will be able to undo \redo the action 
        //            // i could move the primitive back to it's original position and then compute a cumulative translation
        //            // and use that in TMove() and undo/redo is easy... 
        //            // is there a way to create the move 
        //            // TODO: the primitive's values are in world coords and need to be in local.  That's why
        //            // translating was preferred.  Now if there is no scale, we could translate but i think a better idea is to 
        //            // do all the editing in model space... that just requires converting the mouse pick to model space
        //            // by removing the Entity's translation (assuming scaling is never allowed on EditableMesh :((

        //            if (mManipulator.IsActive)
        //            {
        //                // if the control has input capture, then we've just completed a move operation?  
        //                if (mManipulator.HasInputCapture)
        //                {
        //                    mManipulator.ActiveControl.HandleEvent(type, args);
        //                    // verify after the active control processes the event, input capture in the manipulator is relinquished
        //                    // but we won't de-activate because we do want the move widget still visible
        //                    System.Diagnostics.Trace.Assert(mManipulator.HasInputCapture == false);
        //                    _hasInputCapture = mManipulator.HasInputCapture;
                            
        //                    // Todo: in fact, most of this code seems can be shared with either Rotate, Scale or Translate tools
        //                    //          and instead just have different ManipulatorController's installed (eg PositioningManip.cs vs ScalingManipulator.cs)
        //                    //_primitives.Clear(); // primitives must clear after a move so that subsequent moves dont include the previous geometry

        //                    //    DeActivate(); // TODO: is regular DeActivate() even needed anymore?
        //                    //    _manipulator.Deactivate ();
        //                    //    System.Diagnostics.Trace.WriteLine("MoveTool Mouse Up...");
        //                }
        //            }
        //}

        //// TODO: should not have any duplicates added... currently not checking for that
        //public void AddVertex(uint vertexID)
        //{
        //    if (_hasInputCapture) throw new Exception("Can't add new vertices while moving.");
        //    PrimitiveInfo prim;
        //    prim.Type = PrimitiveType.Vertex;
        //    prim.ID = vertexID;
        //    prim.Center = _mesh.GetVertex(vertexID, false);
        //    prim.OriginalPosition = new[]{ prim.Center};
        //    prim.Position = prim.OriginalPosition;
        //    _primitives.Add(prim); 
        //}
        
        //public void AddEdge(uint edgeID)
        //{
        //    EditDataStructures.Edge e = _mesh._cell.GetEdge(edgeID);
        //    PrimitiveInfo prim;
        //    prim.Type = PrimitiveType.Edge;
        //    prim.ID = edgeID;
        //    Vector3d v1 = _mesh.GetVertex(e.Origin.ID, false);
        //    Vector3d v2 = _mesh.GetVertex(e.Destination.ID, false);
        //    prim.Center =  Line3d.Center(v1, v2);
        //    prim.OriginalPosition = new[] {v1, v2};
        //    prim.Position = prim.OriginalPosition;
        //    _primitives.Add(prim); 
        //}

        //public void AddFace(uint faceID)
        //{
        //    // adding a face is the equivalent of adding each vertex that makes up the face

        //    //if (_hasInputCapture) throw new Exception("Can't add new faces while moving.");
        //    //PrimitiveInfo prim;
        //    //prim.Type = PrimitiveType.Face;
        //    //prim.ID = faceID;
        //    //Vector3d vec;

        //    //prim.Position = vec;
        //    //_primitives.Add(prim); 
        //}
    }
}
