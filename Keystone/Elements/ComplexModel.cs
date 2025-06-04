using System;
using System.Xml;
using MTV3D65;

namespace Keystone.Elements
{
    // todo: the question is whether i get rid of the concept of a complex model and use just composite entities 
    // the upside is that the parent entity can manage it's child entities like clouds rotating around a planet 
    // well here's the issue, an entity would have to maintain instance data for each model.  Even though an atmosphere and cloud
    // layer share the same position and possibly rotation, the scale if i share the sphere mesh would not be shared. Further
    // besides overlapped spheres, there's not anything else i can think of where even just position and rotation would be shared.
    // so it would have to be that any model underneath a model would have to take on the instance data of the entity that ultimately
    // is parent and grandparent (respectively) to both.
    //
    // I really think that there is a good case to be made for having the equivalent of TV's Mesh Groups but via Models.
    // I think the restriction must be that a Model that contains child Model's directly _must_ use the same scale/position/rotation
    // of the parent's entity.  
    public class ComplexModel : SimpleModel
    {
        public ComplexModel(string id)
            : base(id)
        {
        }

        public ComplexModel(string id, XmlNode node)
            : base(id)
        {
            this.ReadXml(node);
        }


        public virtual void AddChild(LODSwitch lodSwitch)
        {
            if (_geometry != null && _lod != null)
                throw new Exception("Geometry or LOD node already exists.  Only one node of either type allowed.");

            _lod = lodSwitch;
            AddChild((Node) lodSwitch);
            SetChangeFlags(Enums.ChangeStates.GeometryAdded | Keystone.Enums.ChangeStates.BoundingBoxDirty, Enums.ChangeSource.Self);
        }
    }
}