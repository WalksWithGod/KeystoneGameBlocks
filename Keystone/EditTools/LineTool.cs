using System;
using Keystone.Collision;
using Keystone.Events;
using Keystone.Types;

namespace Keystone.EditTools
{
    public class LineTool : Tool
    {
        public Vector3d? Origin = null;
        public Vector3d? Destination = null;

        
        public LineTool(Keystone.Network.NetworkClientBase netClient) : base(netClient)
        {
            mIsActive = true; 
            mHasInputCapture = true;
        }
        
       
        public override void HandleEvent(EventType type, EventArgs args)
        {
            MouseEventArgs mouseArgs = (MouseEventArgs)args;
            PickResults pickResult = (PickResults)(mouseArgs).Data;
            // PreviousPoint.Value

            switch (type)
            {
                    // TODO: need an event type to know when we've completed plotting points?

                case EventType.MouseMove :
                    break;

                case EventType.MouseDown:
                    _mesh = (EditDataStructures.EditableMesh)pickResult.Geometry;
                    // for testing, we're going to simply add 4 vertices and attempt to make a square
                    // add the points in directx (d3d) clockwise order to denote the front side
                    _mesh.AddEdge(-10, 0, -10, -10, 0, 10);
                    _mesh.AddEdge(-10, 0, 10, 10, 0, 10);
                    _mesh.AddEdge(10, 0, 10, 10, 0, -10);
                    _mesh.AddEdge(10, 0, -10, -10, 0, -10);
                    

                    // the args will contain the x,y coords and the viewtype so we'll know which plane to plot the point
                    // our MeshEditable needs a coherent spec on how it fits into a "scene" and with editing ..

                    Vector3d position;

                    //// determine 3d points based on the current view
                    //switch (_viewport.Camera.ViewType)
                    //{

                    //    case Cameras.Viewport.ViewType.Top :
                    //        System.Diagnostics.Trace.WriteLine("LineTool mousedown top orthographic viewport");
                          

                    //        position = mouseArgs.UnprojectedPosition ;

                    //        // we can test the screenspace distance of the collision point and the nearest edge's nearest vertex to decide if we want to snap this

                    //        if (Origin== null )
                    //        {
                    //            System.Diagnostics.Trace.Assert(Destination == null); // if origin == null, destination must be null as well

                    //            // this is potentially the first vertex in a new face
                    //            if (pickResult.VertexIndex == -1)
                    //            {
                    //                // the vertex does not already exist in the list of vertices in the EditableMesh
                    //                // thus it's definetly start of a unique edge and we do not have to check for duplicate edge

                    //                // ugh. maybe if destination == null then we should still
                    //                // make call to add new edge with origin and destination being the same vertex
                    //                // then on subsequent linetool mousedown, we make the new point the dest if origin != null

                    //            }
                    //            // I believe that as long as you're starting a new line segment loop, it's always a  new face
                    //            // unless you bisect an existing face by selecting origin and dest points that exist on the same face
                    //            // where origin and dest are not the same and where origin and dest don't create an existing
                    //            // edge on that face.

                    //            // but this also seems to mean that we need to track all verts added during the line tool's start correct?
                    //            // cuz a lineTool is really a "face" construction tool.  But also, we need to check if a line's origin and dest
                    //            // are already on a face's existing edge, to ignore all but the dest?

                    //            Origin = position;
                    //        }
                    //        else if (Destination == null)
                    //        {
                    //            // this is the 2nd+ vertex, thus if the origin + the new vertex position form a valid (non existant) edge
                    //            // then we will be making a call to split a face or create a new face edge
                    //            // TODO: hrm, i think it's best if we do not attempt to manage edges ourselves at all...  instead we should
                    //            // be able to get all the info we need just by knowing the vertices and the face id's.
                    //            // But any calls we need to make for the cell's quadedge datastructure, should go through
                    //            // encapsulated calls from the EditableMesh itself.  Here instead
                    //            // we should be doing "CreateFace" , AddVertex(face), etc and nothing about cell.MakeVertexEdge or cell.MakeFaceEdge
                    //            // and such

                    

                    //            // is the previous vertex new?  if it wasnt new, would it already be an existing vertex by now since we
                    //            // added it immediately?  or are we basically just storing until we have a fully completed face?
                    //            // I'm thinking we are adding them immediately...   
                    //            // and to undo, we'd have to remove them... as sucky as that may seem, it should be possible to properly
                    //            // track the original state to know which functions in our QuadEdge api to undo the change

                    //            // indeed, i think that our main "unit" for dealing with QuadEdge is the "edge" so our line tool should deal
                    //            // in terms of segments at a time.  So when starting, if the "origin" is null, then we havent started.
                    //            // if the destination is not null, then on next vertex we swap destination with origin and the new vertex
                    //            // becomes the destination.

                    //            Destination = position;
                    //        }
                    //        else // destination and origin are both NON null
                    //        {
                    //            // swap origin with dest
                    //            Origin = Destination;
                    //            Destination = position;
                    //        }
                    //        // if pickResult tells us we've picked an existing vertex or close enough to one, then we're just adding the index
                    //        // to a new face.  Sketchup will automatically halt the "connect the dot" process once a face is completely closed.
                    //        // so this means the Face (which we can get from the Face Index in PickResults) and check if it is "closed" or not.
                    //        EditDataStructures.Face f = _mesh._cell.Faces [ pickResult.FaceID];
                    //        //if (f.IsClosed())
                    //        //{
                    //            // the user must now initiate another line 
                    //            Origin  = null;
                    //            Destination = null;
                    //        //}

                    //        // we still need the Command  pickResult details for undo purposes, and so the face ID is needed
                    //        // so when we remove an edge for a face, it only removes that face's edge and not the verts entirely which
                    //        // might be in use by other faces that shared that edge
                            

                    //        //  for now we are going to skip the Command Processor entirely
                    //        // and just write code to directly add new verices projected onto the x,z plane
                    //        // we're not going to test for existing verts or anything, just adding as we click.


                    //        // if the current face we are plotting points in has > 3 points then any future points must be projected
                    //        // to 3d such that they are planar.  Sketchup allows skewed polygons, but we wont... (for now?)
                    //        //Plane.Intersects(r, p, ref distance, ref intersectionPoint);

                    //        Vector3d point3d = pickResult.ImpactPoint;
                    //        point3d.z = 0;
                    //        _mesh.AddVertex((float)point3d.x, (float)point3d.y, (float)point3d.z); // TODO: are we losing precision with this cast that will result in non planar faces?
                            
                    //       // AddPoint();
                            
                    //        break;

                    //}
                    // add the point to the target 

                    break;

                default:
                    break;
            }
        }

        private void AddPoint(Vector3d position)
        {
            // should the LineTool determine based on the mouse's x,y position and the scene state, where to plot the point in the scene? 
            // like how does sketchup determine which plane to plot on?
            // I think initially what ill do is plot on y = 0 and then use mouse x, y as X,Z respectively from a top down view with positive z at top of screen
            // so here's a question regarding creating commands...  i really dont think i should have to
            // EnQueue them so they are threaded... wtf? I just want them added to Stack though

            // create a command that will perform the operation and allow us tor undo/redo the operation if commanded

          //  Commands.ICommand command = new EditOperations.PlotPoint(_mesh, position);
            // this is a problem... because even if we want this command to be processed _now_ it can't be because it's been enqueued
            // so one of two things must be done here...
            // either that command knows all it needs to know to carry out hte command and this LineTool isn't really needed aymore...
            // or we get rid of EnQueue and just have CommandProcessor.Execute()
            // if it returns because it would block, we abort the entire command and 
            // Trace.WriteLine ("Operation would block...");
            // 
            // now as far as rendering the vertex and edges, well as soon as we get to the point where we render our verts, it's fine.
            // and from there, we have a simple rule, OUTSIDE of the EditableMesh we take it upon ourselves to draw another line
            // from the current mouse to the last plotted vertex made by the LineTool.  This way it has nothing to do with EditableMesh
            // and there's no cause to add/delete temporary points if the user hits ESC to cancel the operation.

            // I think perhaps from a Network remote editing point of view, the "Would block..." check is something we could do there.  You issue
            // a command and the CommandProcessor knows that the command also needs to go over the wire to a shared version of the game database
            // and then it waits for a result or a timeout.  Then the user can only issue a 2nd command when the Command object
            // has no more blocking operations.  Now perhaps a bit better is, we can do all our own non blocking operations sequentially
            // but if the ACKs start getting too far behind we finally get a "would block" and are prevented from carrying out an operation.
            // But then eventually it resumes.  
            // Can you undo another user's work?  I would think no.. for simplicity, entire zones should be locked for one person or it's too
            // unwieldly... although, Google's Wave is pretty neat.

            // the EditController should query the "lastVertex" of LineTool.. or perhaps LineTool should do it because
            // the EditController doesnt have to know what these various tools do, it just calls the interface.

          //  Core._Core.CommandProcessor.Execute(command);



        }
    }
}
