using System;
using System.Collections.Generic;
using Keystone.Controls;
using Keystone.Appearance;
using Keystone.Elements;
using MTV3D65;
using Keystone.Entities;
using Keystone.Resource;
using Keystone.Types;
using System.IO;
using System.Diagnostics;



namespace Keystone.EditTools
{
    // TODO: should this be moved to KeyEdit\\GUI\\  
    // then wiring up our own events makes good sense... we can have as many
    // widgets as we want because they are actually controlled not by Keystone
    // but by the exe application
    public class Widgets
    {
        public static Controls.Control LoadHingeWidget()
        {

            string id = Repository.GetNewName(typeof(Control));
            Control _control = new Control(id);
            _control.Name = "hinge edit widget";
            _control.Enable = false;
            _control.Overlay = true;
            _control.Dynamic = false;
            _control.CollisionEnable = false;
            _control.Pickable = true;

            DefaultAppearance app;
            Material greenMat = Material.Create(Material.DefaultMaterials.green_fullbright);
            Material blueMat = Material.Create(Material.DefaultMaterials.blue_fullbright);

            // hinge_half.obj")
            Mesh3d hingeMesh = (Mesh3d)Repository.Create(Path.Combine(Core._Core.DataPath, @"editor\widgets\hinge_1.obj"), "Mesh3d");
            hingeMesh.CullMode = (int)CONST_TV_CULLING.TV_DOUBLESIDED;
            
            id = Repository.GetNewName(typeof(Control));
            ModeledEntity entityHinge = new Control(id);
            entityHinge.Name = "Hinge";
            entityHinge.Overlay = true;
            entityHinge.InheritScale = true;

            Model model = new Model(Repository.GetNewName(typeof(Model)));
            // we create an app here, because above we ignore any appearance returned from Mesh3d.Create's out param
            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
            app.AddChild(greenMat);
            model.AddChild(hingeMesh);
            model.AddChild(app);
            entityHinge.AddChild(model);


            _control.AddChild(entityHinge);

            return _control;
        }

        public static Controls.Control LoadHingeAxisWidget()
        {
            double angleRadians = Utilities.MathHelper.DEGREES_TO_RADIANS * 90;

            string id = Repository.GetNewName(typeof(Control));
            Control _control = new Control(id);
            _control.Name = "hinge edit widget";
            _control.Enable = false;
            _control.Overlay = true;
            _control.Dynamic = false;
            _control.CollisionEnable = false;
            _control.Pickable = true;

            DefaultAppearance app;
            Material redMat = Material.Create(Material.DefaultMaterials.red_fullbright);
            Material greenMat = Material.Create(Material.DefaultMaterials.green_fullbright);
            Material blueMat = Material.Create(Material.DefaultMaterials.blue_fullbright);

            Mesh3d axisMesh = (Mesh3d)Repository.Create(Path.Combine(Core._Core.DataPath, @"editor\widgets\hinge_half.obj"), "Mesh3d");
            axisMesh.CullMode = (int)CONST_TV_CULLING.TV_DOUBLESIDED;

            Mesh3d tabMesh = (Mesh3d)Repository.Create(Path.Combine(Core._Core.DataPath, @"editor\widgets\scaletab.obj"), "Mesh3d"); 
            tabMesh.CullMode = (int)CONST_TV_CULLING.TV_DOUBLESIDED;
            
            id = Repository.GetNewName(typeof(Control));
            ModeledEntity entityAxis = new Control(id);
            entityAxis.Name = "Hinge Axis";
            entityAxis.Overlay = true;
            entityAxis.InheritScale = true;
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            //Material mat = Material.Create(Repository.GetNewName(typeof(Material)), new Core.Types.Color(0f, 1f, 0f, 0f), new Core.Types.Color(), new Core.Types.Color(), new Core.Types.Color());
            //app.AddChild(mat);
            // we create an app here, because above we ignore any appearance returned from Mesh3d.Create's out param
            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
            app.AddChild(greenMat);
            model.AddChild(axisMesh);
            model.AddChild(app);
            entityAxis.AddChild(model);


            // Max Angle constraint indicator rotate 90 degrees on z axis
            id = Repository.GetNewName(typeof(Control));
            ModeledEntity entityMaxAngleBar = new Control(id);
            entityMaxAngleBar.Name = "Max Angle Bar";
            entityMaxAngleBar.Overlay = true;
            entityMaxAngleBar.InheritScale = true;

            model = new Model(Repository.GetNewName(typeof(Model)));
            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
            app.AddChild(redMat);
            model.AddChild(tabMesh);
            model.AddChild(app);
            entityMaxAngleBar.Rotation = new Quaternion(new Vector3d(0, 0, 1), angleRadians);
            entityMaxAngleBar.AddChild(model);


            _control.AddChild(entityAxis);
            _control.AddChild(entityMaxAngleBar);

            return _control;
        }

        public static Controls.Control LoadTranslationWidget()
        {
            return LoadWidget(Path.Combine(Core._Core.DataPath, @"editor\widgets\axis_arrow.obj"), 
                null);
        }

        public static Controls.Control LoadScalingWidget()
        {
            return LoadWidget(Path.Combine(Core._Core.DataPath, @"editor\widgets\axis_arrow.obj"),
               null);
        }

        public static Controls.Control LoadRotationWidget()
        {
            return LoadWidget(Path.Combine(Core._Core.DataPath, @"editor\widgets\axis_arrow.obj"),
                null);
        }

        private static Controls.Control LoadWidget(string path, string friendlyName)
        {
            double angleRadians = Utilities.MathHelper.DEGREES_TO_RADIANS * 90;

            string id = Repository.GetNewName(typeof(Control));
            //id = "axis_move_tool"; // TEMP so we can find this tool in scale culler
            Control _control = new Control(id);
             _control.SetEntityAttributesValue ( (uint)KeyCommon.Flags.EntityAttributes.HUD, true);
            _control.UseFixedScreenSpaceSize = true;
            _control.ScreenSpaceSize = .3f; // TODO: does this effect mouse picking since we're picking against normal scaled Control and not the ScreenSpaceSize modified scaling.
            _control.InheritScale = false;
            
            _control.Name = friendlyName;
            _control.Enable = false;
            _control.Pickable = true;
            _control.CollisionEnable = false;
            _control.Overlay = true;
            _control.Dynamic = false;
            
            DefaultAppearance app;

            Mesh3d axisMesh = (Mesh3d)Repository.Create (path, "Mesh3d");
            
            axisMesh.CullMode = (int)CONST_TV_CULLING.TV_DOUBLESIDED;
            
            // create sequence
            id = Repository.GetNewName(typeof(ModelSequence));
            ModelSequence sequence = new ModelSequence(id);

            // x arrow has just translation no rotation
            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
            app.AddChild(Material.Create(Material.DefaultMaterials.red_fullbright));
            Model model = new Model(Repository.GetNewName(typeof(Model)));
            model.Rotation = new Quaternion(new Vector3d(0, 0, 1), -angleRadians);
            model.Name = "xAxisModel";
            model.AddChild(axisMesh);
            model.AddChild(app);
            sequence.AddChild(model);

            // y arrow has just translation no rotation
            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
            app.AddChild(Material.Create(Material.DefaultMaterials.green_fullbright));
            model = new Model(Repository.GetNewName(typeof(Model)));
            model.Name = "yAxisModel";
            model.AddChild(axisMesh);
            model.AddChild(app);
            sequence.AddChild(model);

            // z arrow rotate 90 degrees on x axis
            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
            app.AddChild(Material.Create(Material.DefaultMaterials.blue_fullbright));
            model = new Model(Repository.GetNewName(typeof(Model)));
            model.Rotation = new Quaternion(new Vector3d(1, 0, 0), angleRadians);
            model.Name = "zAxisModel";
            model.AddChild(axisMesh);
            model.AddChild(app);
            sequence.AddChild(model);

            _control.AddChild(sequence);
            Keystone.IO.PagerBase.LoadTVResource(_control);
            return _control;
        }


//        public static Controls.Control LoadWidget()
//        {
//            double angleRadians = Utilities.MathHelper.DEGREES_TO_RADIANS * 90;
//            string id = Repository.GetNewName(typeof(Control));

//            Control _control = new Control(id);
//            _control.Name = "manipulator";
//            _control.Enable = false;
//            _control.Overlay = true;
//            _control.Dynamic = false;
//            _control.CollisionEnable = false;
//            _control.Pickable = true;

//            DefaultAppearance app;
//            Material redMat = Material.Create(Material.DefaultMaterials.red_fullbright);
//            Material greenMat = Material.Create(Material.DefaultMaterials.green_fullbright);
//            Material blueMat = Material.Create(Material.DefaultMaterials.blue_fullbright);

//            // TODO: we want to be able to attach a rollover material changer directly into the model itself
//            //          so that it's done during traversal, how do we do this?  Similarly it's what we want to do
//            //         for damage textrue modeling so that the same model can be shared and represented on screen in various
//            //         states.
//            //        TODO: i think the rollover could be done in a script and using a second
//            //              appearance
//            Mesh3d arrowMesh = Mesh3d.Create(Path.Combine(Core._Core.DataPath, @"editor\widgets\positiontab.obj"), false, false, out app);
//            arrowMesh.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED;

//            Mesh3d axisMesh = Mesh3d.Create(Path.Combine(Core._Core.DataPath, @"editor\widgets\axis.obj"), false, false, out app);
//            axisMesh.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED;

//            Mesh3d originMesh = Mesh3d.Create(Path.Combine(Core._Core.DataPath, @"editor\widgets\origin.obj"), false, false, out app);
//            originMesh.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED;

//            //id = Repository.GetNewName(typeof(Control));
//            //ModeledEntity entityOrigin = new Control(id);
//            //entityOrigin.Name = "Widget Origin";
//            //entityOrigin.Overlay = true;
//            ////Material mat = Material.Create(Repository.GetNewName(typeof(Material)), new Core.Types.Color(0f, 1f, 0f, 0f), new Core.Types.Color(), new Core.Types.Color(), new Core.Types.Color());
//            ////app.AddChild(mat);
//            //// we create an app here, because above we ignore any appearance returned from Mesh3d.Create's out param
//            //Model model = new Model(Repository.GetNewName(typeof(Model)));
//            //app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            //model.AddChild(originMesh);
//            //model.AddChild(app);
//            //entityOrigin.AddChild(model);

//            // x arrow rotate 90 degrees on z axis
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityPositionX = new Control(id);
//            entityPositionX.Name = "X Arrow";
//            Model model = new Model(Repository.GetNewName(typeof(Model)));
//            entityPositionX.Overlay = true;
//            entityPositionX.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(redMat);
//            model.AddChild(arrowMesh);
//            model.AddChild(app);
//            entityPositionX.Rotation = new Quaternion(new Vector3d(0, 0, 1), angleRadians);
//            entityPositionX.AddChild(model);

//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityAxisX = new Control(id);
//            entityAxisX.Name = "xAxisEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityAxisX.Overlay = true;
//            entityAxisX.InheritScale = true;

//            model.AddChild(axisMesh);
//            model.AddChild(app);
//            entityAxisX.Rotation = new Quaternion(new Vector3d(0, 0, 1), angleRadians);
//            entityAxisX.AddChild(model);

//            // y arrow has just translation no rotation
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityPositionY = new Control(id);
//            entityPositionY.Name = "yArrowEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityPositionY.Overlay = true;
//            entityPositionY.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(greenMat);
//            model.AddChild(arrowMesh);
//            model.AddChild(app);
//            entityPositionY.AddChild(model);

//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityAxisY = new Control(id);
//            entityAxisY.Name = "yAxisEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityAxisY.Overlay = true;
//            entityAxisX.InheritScale = true;

//            model.AddChild(axisMesh);
//            model.AddChild(app);
//            entityAxisY.AddChild(model);

//            // z arrow rotate 90 degrees on x axis
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityPositionZ = new Control(id);
//            entityPositionZ.Name = "zArrowEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityPositionZ.Overlay = true;
//            entityPositionZ.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(blueMat);
//            model.AddChild(arrowMesh);
//            model.AddChild(app);
//            entityPositionZ.Rotation = new Quaternion(new Vector3d(1, 0, 0), -angleRadians);
//            entityPositionZ.AddChild(model);

//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityAxisZ = new Control(id);
//            entityAxisZ.Name = "zAxisEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityAxisZ.Overlay = true;
//            entityAxisZ.InheritScale = true;

//            model.AddChild(axisMesh);
//            model.AddChild(app);
//            entityAxisZ.Rotation = new Quaternion(new Vector3d(1, 0, 0), -angleRadians);
//            entityAxisZ.AddChild(model);

//            // the origin model0 is a Model and not an Entity and as such it assumes the bounding volume of the entire
//            // manipulator control.  If we want to be able to seperately click on just the origin, it must be added as an entity
//            //_control.Overlay = true;
////            _control.AddChild(entityOrigin);
//            _control.AddChild(entityPositionX);
//            _control.AddChild(entityPositionY);
//            _control.AddChild(entityPositionZ);
//            _control.AddChild(entityAxisX);
//            _control.AddChild(entityAxisY);
//            _control.AddChild(entityAxisZ);


//            //////////////////////////////////////////////////////////////////////////////////
//            // SCALING - re-uses the same axis meshes and models just under a new control and with different 
//            Mesh3d scaleTabMesh = Mesh3d.Create(Path.Combine(Core._Core.DataPath, @"editor\widgets\scaletab.obj"), false, false, out app);
//            scaleTabMesh.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED;

//            // x arrow rotate 90 degrees on z axis
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityScaleX = new Control(id);
//            entityScaleX.Name = "xScaleEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityScaleX.Overlay = true;
//            entityScaleX.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(redMat);
//            model.AddChild(scaleTabMesh);
//            model.AddChild(app);
//            entityScaleX.Rotation = new Quaternion(new Vector3d(0, 0, 1), angleRadians);
//            entityScaleX.AddChild(model);

//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityScaleAxisX = new Control(id);
//            entityScaleAxisX.Name = "xScaleAxisEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityScaleAxisX.Overlay = true;
//            entityScaleAxisX.InheritScale = true;

//            model.AddChild(axisMesh);
//            entityScaleAxisX.Rotation = new Quaternion(new Vector3d(0, 0, 1), angleRadians);
//            entityScaleAxisX.AddChild(model);

//            // y scaling tab has just translation no rotation
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityScaleY = new Control(id);
//            entityScaleY.Name = "yScaleEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityScaleY.Overlay = true;
//            entityScaleY.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(greenMat);
//            model.AddChild(scaleTabMesh);
//            model.AddChild(app);
//            entityScaleY.AddChild(model);

//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityScaleAxisY = new Control(id);
//            entityScaleAxisY.Name = "yScaleAxisEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityScaleAxisY.Overlay = true;
//            entityScaleAxisY.InheritScale = true;

//            model.AddChild(axisMesh);
//            entityScaleAxisY.AddChild(model);

//            // z scale tab rotate 90 degrees on x axis
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityScaleZ = new Control(id);
//            entityScaleZ.Name = "zScaleEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityScaleZ.Overlay = true;
//            entityScaleZ.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(blueMat);
//            model.AddChild(scaleTabMesh);
//            model.AddChild(app);
//            entityScaleZ.Rotation = new Quaternion(new Vector3d(1, 0, 0), -angleRadians);
//            entityScaleZ.AddChild(model);

//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity entityScaleAxisZ = new Control(id);
//            entityScaleAxisZ.Name = "zScaleAxisEnt";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            entityScaleAxisZ.Overlay = true;
//            entityScaleAxisZ.InheritScale = true;

//            model.AddChild(axisMesh);
//            entityScaleAxisZ.Rotation = new Quaternion(new Vector3d(1, 0, 0), -angleRadians);
//            entityScaleAxisZ.AddChild(model);

//            //_control.AddChild(modelO);
//            _control.AddChild(entityScaleX);
//            _control.AddChild(entityScaleY);
//            _control.AddChild(entityScaleZ);
//            _control.AddChild(entityScaleAxisX);
//            _control.AddChild(entityScaleAxisY);
//            _control.AddChild(entityScaleAxisZ);

//            //////////////////////////////////////////////////////////////////////////////////
//            // ROTATION
//            Mesh3d circleaxis = Mesh3d.Create(Path.Combine(Core._Core.DataPath, @"editor\widgets\circleaxis.obj"), false, false, out app);
//            circleaxis.CullMode = CONST_TV_CULLING.TV_DOUBLESIDED;

//            // x circle rotate 90 degrees on z axis
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity xCircleAxis = new Control(id);
//            xCircleAxis.Name = "xcircleaxis";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            xCircleAxis.Overlay = true;
//            xCircleAxis.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(redMat);
//            model.AddChild(circleaxis);
//            model.AddChild(app);
//            xCircleAxis.Rotation = new Quaternion(new Vector3d(0, 0, 1), angleRadians);
//            xCircleAxis.AddChild(model);

//            // y circle
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity yCircleAxis = new Control(id);
//            yCircleAxis.Name = "ycircleaxis";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            yCircleAxis.Overlay = true;
//            yCircleAxis.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(greenMat);
//            model.AddChild(circleaxis);
//            model.AddChild(app);
//            yCircleAxis.AddChild(model);

//            // z circle rotate 90 degrees on x axis
//            id = Repository.GetNewName(typeof(Control));
//            ModeledEntity zCircleAxis = new Control(id);
//            zCircleAxis.Name = "zcircleaxis";
//            model = new Model(Repository.GetNewName(typeof(Model)));
//            zCircleAxis.Overlay = true;
//            zCircleAxis.InheritScale = true;

//            app = new DefaultAppearance(Repository.GetNewName(typeof(DefaultAppearance)));
//            app.AddChild(blueMat);
//            model.AddChild(circleaxis);
//            model.AddChild(app);
//            zCircleAxis.Rotation = new Quaternion(new Vector3d(1, 0, 0), -angleRadians);
//            zCircleAxis.AddChild(model);

//            _control.AddChild(xCircleAxis);
//            _control.AddChild(yCircleAxis);
//            _control.AddChild(zCircleAxis);

//            return _control;
//        }
    }
}
