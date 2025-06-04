//using System;
//using System.Xml;
//using Keystone.Resource;

//namespace Keystone.Elements
//{
//    /// <summary>
//    /// Simple NON Hierarchical Model that can only contain Appearance and Geometry as children.
//    /// For Minimesh Instancing, im debating whether there I want to also enforce a rule that only
//    /// single group tvmeshes can be used?  
//    /// TODO: i might just have entity.Model.SelectGeometry() include an 'out' param for the selected geometry?
//    //         But wait, that screws up things since our minimesh is named by the Model so if a model has multiple LOD's
//    //         for internal geometry that ruins that.  Might be best to have a new class InstancedModel that cannot be
//    //         hierarchical NOR contain any LOD and perhaps also enforce only a tvmesh with a single group in it.
//    //((InstancedModel)entity).Model.Minimesh.AddInstanceData(cameraRelativePosition, entity.Scale, entity.Rotation);
//    /// </summary>
//    public class InstancedModel : ModelBase
//    {
//        public Minimesh Minimesh; // the key difference is InsancedModel retains a reference to the underlying Minimesh
//                                              // but id rather ability to just check a box that sets a flag on a single Model (no LODModel or SimpleModel or InstancedModel)
//                                              // where 

//        internal InstancedModel(string id)
//            : base(id)
//        {
//        }

//        public static InstancedModel Create(string id)
//        {
//            InstancedModel model = (InstancedModel)Repository.Get(id);
//            if (model != null) return model;
//            model = new InstancedModel(id);
//            return model;
//        }

//        public static InstancedModel Create(string id)
//        {
//            InstancedModel model;
//            model = (InstancedModel)Repository.Get(id);
//            if (model != null) return model;
//            model = new InstancedModel(id);

//            return model;
//        }

//        public void AddChild(Mesh3d geometry)
//        {
//            base.AddChild(geometry);
//        }
//    }
//}