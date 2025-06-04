//using System;
//using Keystone.Appearance;
//using Keystone.Controls;
//using Keystone.Elements;
//using Keystone.Entities;
//using Keystone.Events;
//using Keystone.Resource;
//using Keystone.Types;
//using MTV3D65;

//namespace Keystone.Controllers
//{
//    /// <summary>
//    /// The scalingManipulator is the GUI 3d gizmo that appears in the world and which has pickable
//    /// boxes in the corner, center of edges and center of faces to allow fast & easy scaling of the underlying geometry
//    /// by the user.  
//    /// A scalingManipulator instance is created by the user's selection of the "scale tool" from the toolbox.
//    /// Once initiated, the various pickable boxes that comprise the manipulator will fire events upon
//    /// user interaction and those events will be handled here.  
//    /// So there's two types of "tools".  Tools like scaling tool that use a widget, and tools like MoveTool
//    /// which directly move's geometry without an intermediary GUI mesh.  
//    /// </summary>
//    public class ScalingManipulator : ManipulatorController  
//    {
//        private Control[] _scalingTabs;
//        private ModeledEntity _yellowModel;
//        private ModeledEntity _greenModel;
//        private const float SCREEN_SPACE_PERCENTAGE = .1f;
//        public ScalingManipulator()
//        {
//            LoadVisuals();
//            WireEvents();
//        }

//        public override void Activate(Scene.ClientScene scene)
//        {
//            base.Activate(scene);
//            UpdateScalingTabPositions();
//        }

//        private void LoadVisuals()
//        {
//            throw new Exception("After eliminating ModelBase, this still has issues");
//            //// if the visual is already loaded, just grab it
//            //_control = (Control)Repository.Get("scalingwidget");

//            //// TODO: the control is not Repository.DecrementRef when destroyed after we purposefully increment it's ref
//            //// then, the child controls are never recursively decremented and so stay in repository
//            //// so the child controls are still there, but the _scalingTab array is empty cuz this is a new instance
//            //// and because this is not an EntityBase the child refs are not retained here...
//            //if (_control == null)
//            //{
//            //    Model model = new Model(Repository.GetNewName(typeof(Model)));
//            //    AppearanceLOD appLOD = new AppearanceLOD(Repository.GetNewName(typeof (DefaultAppearance)));
//            //    DefaultAppearance app;
//            //    Material yellowMat = Material.Create(Material.DefaultMaterials.yellow_fullbright);
//            //    Material greenMat = Material.Create(Material.DefaultMaterials.green_fullbright);
//            //    Mesh3d scalebox = Mesh3d.CreateBox("scaleboxhandle", 1, 1, 1);
//            //    scalebox.Overlay = false;
//            //    scalebox.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED;

//            //    _yellowModel = new StaticEntity(Repository.GetNewName(typeof(StaticEntity)));
//            //    app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            //    app.AddChild(yellowMat);
//            //    model.AddChild(app);
//            //    model.AddChild(scalebox);
//            //    _yellowModel.AddChild(model);

//            //    // TODO: these appearance LODs need to just be attached to Models under
//            //    // a geometry switch since ive re-added Models
//            //    //_yellowModel.AddChild(app);
//            //    appLOD.AddChild(app);
//            //    _yellowModel.AddChild(appLOD);

//            //    // perhaps rather than set a seperate model, instead we have some kind of "appearance" selector node
//            //    // where the draw traverser will pick up a value from Entity and use that to select (potentially) a different appearance
//            //    // child node?
//            //    //_greenModel = new SimpleModel(Repository.GetNewName(typeof(SimpleModel)));
//            //    DefaultAppearance greenapp = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            //    greenapp.AddChild(greenMat);
//            //    //_greenModel.AddChild(scalebox);
//            //    //_greenModel.AddChild(greenapp);
//            //    appLOD.AddChild(greenapp);

//            //    // there's 8 boxes for a 2d face (4 corners, 4 centers of edges)
//            //    // there's 26 boxes for a 3d face (8 corners, 6 center of each face on the scaling cube, and 12 center of edges)
//            //    // our array will contain 27 because the center is actually not used
//            //    int index = 0;
//            //    _control = new Control("scalingwidget");
//            //    _control.Enable = false;
//            //    _scalingTabs = new Control[27];
//            //    for (int i = -1; i < 2; i ++)
//            //        for (int j = -1; j < 2; j ++)
//            //            for (int k = -1; k < 2; k++)
//            //            {
//            //                if (index == 13) // skip the center
//            //                {
//            //                    index++;  
//            //                    continue; 
//            //                }
//            //                string name = string.Format("scaleent_{0},{1},{2}", i, j, k);
//            //                _scalingTabs[index] = new Control(name);
//            //                // all children must have this set independantly so we dont have to deal with hierarchical settings
//            //                // which is a no no in our framework (too much aded complexity when it's simple to set these manually)
//            //                // TODO: actually now that i've gotten rid of ModelBase
//            //                // and attach Geometry directly to entities, the hierarchical model is
//            //                // much easier.  All we have to do is set the scale of the Parent entity
//            //                // and all children will be scaled appropriately.
//            //                //_scalingTabs[index].UseFixedScreenSpaceSize = true;
//            //                //_scalingTabs[index].ScreenSpaceSize = SCREEN_SPACE_PERCENTAGE;
//            //                _scalingTabs[index].AddChild(_yellowModel);
//            //                _control.AddChild(_scalingTabs[index]);
//            //                index++;
//            //            }
                
//            //    // screen space quad lines
//            //    // http://www.flipcode.com/archives/Textured_Lines_In_D3D.shtml
                


//            //    // the origin model0 is a Model and not an Entity and as such it assumes the bounding volume of the entire
//            //    // manipulator control.  If we want to be able to seperately click on just the origin, it must be added as an entity

                
//            //    // TODO: i remember having written somewhere that upon Adding, recursive IncrementRef should take place for all children
//            //    //         and same with removing.  Same with a manual IncrementRef I believe we shoudl recurse all children here too.
//            //    //         A question is, should the recursion be done in IncrementRef or in AddChild and in cases of manual increment, just recurse
//            //    //         with new code right there?
//            //        Repository.IncrementRef(null, _control);
//            //    // Repository.IncrementRef(yarrow);

//            //    // TODO: recall that a model has no instance data.  So even if we had each seperate arrow model under the arcball entity
//            //    //       we would need seperate instance data for translation, scale, rotation, matrix.  Thats why i ultimately decided
//            //    //      that a Entity could only have one model for which it would hold instance data for that, and then child sub entities which
//            //    //      would retain the instance data for their own respective models.  The parent entities always has control over any 1st child entities.
//            //    }
//            //_control.UseFixedScreenSpaceSize = true;
//            //_control.ScreenSpaceSize = SCREEN_SPACE_PERCENTAGE;
//        }

//        private void WireEvents()
//        {
//            // TODO: i should move the origin mesh into an actual sub-Control and not just as a mesh directly added to Children array
//            //((Control)_manipulator.Children[0])  // origin mesh.  Meshes aren't Controls obviously.  The origin entity is actually the manipulator itself
//            //((Control)_manipulator).MouseEnter += OnMouseEnter;
//            //((Control)_manipulator).MouseLeave += OnMouseLeave;
//            //((Control)_manipulator).MouseDown += OnMouseDown;
//            //((Control)_manipulator).MouseUp += OnMouseUp;
//            //((Control)_manipulator).MouseClick += OnMouseClick;
//            //((Control)_manipulator).MouseDrag += OnMouseDrag;

//            for (int i = 0; i < _control.Children.Length; i++)
//            {
//                ((Control) _control.Children[i]).MouseEnter += OnMouseEnter;
//                ((Control) _control.Children[i]).MouseLeave += OnMouseLeave;
//                ((Control) _control.Children[i]).MouseDown += OnMouseDown;
//                ((Control) _control.Children[i]).MouseUp += OnMouseUp;
//                ((Control) _control.Children[i]).MouseClick += OnMouseClick;
//                ((Control) _control.Children[i]).MouseDrag += OnMouseDrag;
//            }
//        }

//        private void UpdateScalingTabPositions()
//        {
//            BoundingBox _box = _target.BoundingBox;
//            Vector3d[] vertices = GetScalingTabPositions(_box);

//            for (int i = 0; i < vertices.Length; i++)
//            {
//                if (i == 13) continue;
//                // TODO: somehow these are getting unloaded when we deactivate the manipulator
//                _scalingTabs[i].Translation = vertices[i];
//            }
//        }

//        private Vector3d [] GetScalingTabPositions(BoundingBox box)
//        {
//            Vector3d[] vertices = new Vector3d[27];
//            //Vector3d halfWidths = (box.Max - box.Min/2);
//            Vector3d center = box.Center;

//            vertices[0] = box.Min;

//            vertices[1].x = box.Min.x;
//            vertices[1].y = box.Min.y;
//            vertices[1].z = center.z;

//            vertices[2].x = box.Min.x;
//            vertices[2].y = box.Min.y;
//            vertices[2].z = box.Max.z;

//            vertices[3].x = box.Min.x;
//            vertices[3].y = center.y;
//            vertices[3].z = box.Min.z;

//            vertices[4].x = box.Min.x;
//            vertices[4].y = center.y;
//            vertices[4].z = center.z;

//            vertices[5].x = box.Min.x;
//            vertices[5].y = center.y;
//            vertices[5].z = box.Max.z;

//            vertices[6].x = box.Min.x;
//            vertices[6].y = box.Max.y;
//            vertices[6].z = box.Min.z;

//            vertices[7].x = box.Min.x;
//            vertices[7].y = box.Max.y;
//            vertices[7].z = center.z;

//            vertices[8].x = box.Min.x;
//            vertices[8].y = box.Max.y;
//            vertices[8].z = box.Max.z;
//            //////////////////////////////
//            vertices[9].x = center.x;
//            vertices[9].y = box.Min.y;
//            vertices[9].z = box.Min.z;

//            vertices[10].x = center.x;
//            vertices[10].y = box.Min.y;
//            vertices[10].z = center.z;

//            vertices[11].x = center.x;
//            vertices[11].y = box.Min.y;
//            vertices[11].z = box.Max.z;

//            vertices[12].x = center.x;
//            vertices[12].y = center.y;
//            vertices[12].z = box.Min.z;

//            vertices[13] = center;

//            vertices[14].x = center.x;
//            vertices[14].y = center.y;
//            vertices[14].z = box.Max.z;

//            vertices[15].x = center.x;
//            vertices[15].y = box.Max.y;
//            vertices[15].z = box.Min.z;

//            vertices[16].x = center.x;
//            vertices[16].y = box.Max.y;
//            vertices[16].z = center.z;

//            vertices[17].x = center.x;
//            vertices[17].y = box.Max.y;
//            vertices[17].z = box.Max.z;
//            //////////////////////////////
//            vertices[18].x = box.Max.x;
//            vertices[18].y = box.Min.y;
//            vertices[18].z = box.Min.z;

//            vertices[19].x = box.Max.x;
//            vertices[19].y = box.Min.y;
//            vertices[19].z = center.z;

//            vertices[20].x = box.Max.x;
//            vertices[20].y = box.Min.y;
//            vertices[20].z = box.Max.z;

//            vertices[21].x = box.Max.x;
//            vertices[21].y = center.y;
//            vertices[21].z = box.Min.z;

//            vertices[22].x = box.Max.x;
//            vertices[22].y = center.y;
//            vertices[22].z = center.z;

//            vertices[23].x = box.Max.x;
//            vertices[23].y = center.y;
//            vertices[23].z = box.Max.z;

//            vertices[24].x = box.Max.x;
//            vertices[24].y = box.Max.y;
//            vertices[24].z = box.Min.z;

//            vertices[25].x = box.Max.x;
//            vertices[25].y = box.Max.y;
//            vertices[25].z = center.z;

//            vertices[26] = box.Max;

//            return vertices;
//            //  finding the opposite box is simply -x,-y,-z  // reverse the current array index values
//            // from the above itteration, the order that we'd get verts is 
//            // -x-y-z - index 0
//            // -x-y0  
//            // -x-y+z
//            // -x0-z
//            // -x00
//            // -x0+z - index 5
//            // -x+y-z
//            // -x+y0
//            // -x+y+z

//            // 0-y-z - index 9
//            // 0-y0
//            // 0-y+z
//            // 00-z
//            // 000   - index 13 
//            // 00+z
//            // 0+y-z
//            // 0+y0
//            // 0+y+z - index 17

//            // +x-y-z
//            // +x-y0
//            // +x-y+z - index 20
//            // +x0-z
//            // +x00
//            // +x0+z - index 23
//            // +x+y-z
//            // +x+y0
//            // +x+y+z - index 26
//        }
//        protected override void OnMouseDown(object sender, EventArgs args)
//        {
//            base.OnMouseDown(sender, args);

//            _selectedTab = (Control)sender;
//            Vector3d scale = GetSelectedScalingTabScale(_selectedTab.ID);
//            _scalingTabStartPosition = _selectedTab.Translation;
//            _scalingTabOppositeStartPosition = GetOppositeScalingTab(_selectedTab.ID).Translation;
//            _targetStartingScale = _target.Scale;
//            // cache the starting mouse position and get the cstarting world bounding box for the target box
//            Vector3d mouseStartingPosition = ((MouseEventArgs)args).UnprojectedPosition;
//        }

//        private Control _selectedTab;
//        private Vector3d _targetStartingScale; 
//        private Vector3d _scalingTabOppositeStartPosition;
//        private Vector3d _scalingTabStartPosition;
//        private Vector3d GetSelectedScalingTabScale(string name)
//        {
//            Vector3d scale;
//            scale.x = 0;
//            scale.y = 0;
//            scale.z = 0;

//            // using the name of the box, we'll figure out which way we need to scale the target
//            // as well as where to (potentially)anchor the target during the scaling operation
//            string[] scales = name.Split(new[] { '_' });
//            string[] vals = scales[1].Split(new[] { ',' });
//            scale.x = float.Parse(vals[0]);
//            scale.y = float.Parse(vals[1]);
//            scale.z = float.Parse(vals[2]);
//            return scale;
//        }


//        private Control GetOppositeScalingTab(string name)
//        {
//            Vector3d vscale = -GetSelectedScalingTabScale(name);

//            string oppositeName = string.Format("scaleent_{0},{1},{2}", (int)vscale.x, (int)vscale.y, (int)vscale.z);

//            int selected = 0;
//            for (int i = 0; i < 27; i++)
//            {
//                if (i == 13) continue;
//                if (_scalingTabs[i].ID == oppositeName)
//                {
//                    System.Diagnostics.Debug.WriteLine("Opposite scaling tab = " + oppositeName);
//                    selected = i;
//                    break;
//                }
//            }
//            return _scalingTabs[selected];
//        }

//        protected override void OnMouseUp(object sender, EventArgs args)
//        {
//            base.OnMouseUp(sender, args);
//        }

//        protected override void OnMouseClick(object sender, EventArgs args)
//        {
//            base.OnMouseClick(sender, args);
//        }

//        protected override void OnMouseDrag(object sender, EventArgs args)
//        {
//            double scale = 1;

//            //Control ctrl = (Control)sender;
//            // for component/entity translation, the unprojected mouse coords should remain in world coords 
//            // (unlike in MoveTool where we convert to modelspace coords primarily because our editable geometry verts are in modelspace until rendered)
//            Vector3d mouseProjectedCoords = ((MouseEventArgs)args).UnprojectedPosition ;

//            //// get the model space bounding box of the target so that we know what scale of 1 equates to.
//            //BoundingBox modelSpaceBox = ((ModeledEntity)_target)._model.Geometry.BoundingBox;

//            //// unproject the corner to mouse coords, then project it again onto the near plane so it's now the same coords as the mouse's projected coords
//            ////scale = v1 / v2;

//            //Matrix m = _target.Matrix;
//            //Vector3d position = new Vector3d(m.M41, m.M42, m.M43);
//            //mouseProjectedCoords -= position;

//            // we need to unproject the position coordinate of the selected scaling tab
//            Vector3d screenCoords = ((MouseEventArgs)args).Viewport.Project(_scalingTabStartPosition);
//            // then we project the result onto our 0.0 plane in world coords
//            Vector3d coords = ((MouseEventArgs)args).Viewport.UnProject((int)screenCoords.x, (int)screenCoords.y, 0.0f);

//            // then we can divide that value by the mouse's current projectedCoords to find a scale 
//            // whether we use negative or positive scaling is dependant on which tab it is....
//            // i think what we shoudl do is multiply the vertices[selected].x * scale; along with every component
//            Vector3d vscale = GetSelectedScalingTabScale(_selectedTab.ID);
//            Vector3d tmp;
//            tmp.x = coords.x / mouseProjectedCoords.x;
//            tmp.y = coords.y / mouseProjectedCoords.y;
//            tmp.z = coords.z / mouseProjectedCoords.z;

//            scale =  coords.x / mouseProjectedCoords.x;
//            //vscale *= scale;
//            //vscale *= tmp;
//            vscale += _targetStartingScale;

//           // vscale = new Vector3d(1, 1, 1);
//            // what we want to compute is simply this, based on the screen location of the mouse cursor
//            // what would be the position of that same corner of the bounding box and then what scale would be needed
//            // to have it fall on that position.  Simple.

//            // scalingTabStartPosition and OppositePositions are not affected by scaling of each tab to maintain constant screenspace.  Only
//            // the scale/size of the tabs are affected, not their positions.
//           // Vector3d dimension = _scalingTabOppositeStartPosition - _scalingTabStartPosition;
//           // Vector3d rayDir = Vector3d.Normalize(dimension);

//           // //  what we want here is for the center of this line to move to where the mouse is
//           // // and then compute the difference between it's original starting position and the current mouse
//           // // and use that to translate the two endpoints
//           // Vector3d front = new Vector3d();
//           // Vector3d up = new Vector3d();
//           // Vector3d right = new Vector3d();

//           //((MouseEventArgs)args).Viewport.Context.Camera.GetBasisVectors(ref front, ref up, ref right);
            
//           // //Plane p = new Plane(right,  -mouseProjectedCoords.x);
//           // Plane p = new Plane(right, -mouseProjectedCoords.Length); // TODO: for computing this plane, ideally we want orthoprojected mouse coords even in perspective
//           // Ray r = new Ray(_scalingTabOppositeStartPosition, rayDir);
//           // Vector3d hit = new Vector3d();
//           // double d = 0;
//           // // find the point where a ray intersect p
//           // p.Intersects(r,ref d, ref hit);

//           // Vector3d distance = _scalingTabOppositeStartPosition - hit;
//           // scale = distance.Length / dimension.Length;

//           // // check if we should be mirroingr the scaling
//           // if (Vector3d.DotProduct(Vector3d.Normalize( distance), rayDir) <= -1 + .001)
//           // {
//           //     scale *= -1;
//           // }
//            //Vector3d vscale;
//            //vscale.x = scale;
//            //vscale.y = scale;
//            //vscale.z = scale;

//            // TODO:is there an issue when scaling something such that it overlaps or unoverlaps multiple regions?
//            _target.Scale = vscale ;
//            UpdateScalingTabPositions();
//        }

//        protected override void OnMouseEnter(object sender, EventArgs args)
//        {
//            base.OnMouseEnter(sender, args);
//            Control ctrl = (Control) sender;
//            throw new NotImplementedException("Appearance flags renamed ModelFlags and moved to model");
//            //ctrl.AppearanceFlags = 1;
//        }

//        protected override void OnMouseLeave(object sender, EventArgs args)
//        {
//            base.OnMouseLeave(sender, args);
//            Control ctrl = (Control)sender;
//            throw new NotImplementedException("Appearance flags renamed ModelFlags and moved to model");
//            //ctrl.AppearanceFlags = 0;
//        }
//    }
//}
