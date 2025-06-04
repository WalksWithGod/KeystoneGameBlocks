using System;
using System.Collections.Generic;
using System.Text;
using Keystone.Appearance;
using Keystone.Celestial;
using Keystone.Elements;
using Keystone.Entities;
using Keystone.Lights;
using Keystone.Octree;
using Keystone.Portals;
using Keystone.Shaders;

namespace Keystone.Traversers
{
    public class SuperSetter
    {
        private Node _parent;

        public SuperSetter(Node parent)
        {
            if (parent == null) throw new ArgumentNullException();
            _parent = parent;
        }

        #region ITraverser Members

        // TODO: i could override the typename for InterpolatorAnimation to be
        // propertyName + _InterpolatorAnimation"
        readonly string translationInterpolator = typeof(Keystone.Animation.KeyframeInterpolator<Keystone.Types.Vector3d>).FullName;

        // TODO: typically this Apply should not be needed.  We should be using double dispatch
        // as in targetNode.Traverse (superSetter);
        // and the only thing preventing that is we did not make SuperSetter implement ITraverser
        public void Apply(Node node)
        {
        	try 
        	{
	            switch (node.TypeName)
	            {
	                case "Viewpoint":
	                    Apply((Viewpoint)node);
	                    break;
	                case "Occluder":
	                case "ZoneRoot":
	                case "Zone":
	                case "Interior": // formerly CelledRegion
	                case "Root":
	                case "Region":
	                    Apply((Region) node);
	                    break;
                    case "Structure":
	                    Apply ((TileMap.Structure)node);
	                    break;

	                    
	                case "Star":
	                    Apply((Star) node);
	                    break;
	                case "StellarSystem":
	                    Apply((Celestial.StellarSystem) node);
	                    break;
	                case "World":
	                    Apply((World) node);
	                    break;

	                case "PlayerVehicle":
	                case "Vehicle":
	                case "Container":
	                    Apply((Container)node);
	                    break;
                    case "Background3D":
	                case "ModeledEntity":
	                case "DynamicEntity":
	                case "Player":
	                case "NPC":
	                    Apply((ModeledEntity)node);
	                    break;
	                case "BonedEntity":
	                    Apply((BonedEntity)node);
	                    break;
                    case "DefaultEntity":
                        Apply((DefaultEntity)node);
                        break;
	                case "Light":
	                case "SpotLight":
	                case "DirectionalLight":
	                case "PointLight":
	                    Apply((Light) node);
	                    break;
	                case "DomainObject":
	                    // TODO: this should be obsolete since now we only create internally
	                    // not through Node.Clone() or IO.SceneReader
	                    Apply((Keystone.DomainObjects.DomainObject)node);
	                    break;
	                case "CellFootprint":
	                    // TODO: this should be obsolete since now we only create internally
	                    // not through Node.Clone() or IO.SceneReader
	                    Apply((Keystone.Portals.CellFootprint)node);
	                    break;
	                case "Model":
	                    Apply((Model)node);
	                    break;
	                case "ModelSelector":
	                case "ModelSequence":
	                    Apply((ModelSelector)node);
	                    break;
	                //case "GeometrySwitch":
	                //case "LODSwitch":
	                //    Apply((ModelSwitch)node);
	                //    break;
	                case "Actor3d":
	                    Apply((Actor3d) node);
	                    break;
	                case "Billboard":
	                    Apply((Mesh3d)node);
	                    break;
	                case "Mesh3d":
	                    Apply((Mesh3d) node);
	                    break;
                   case "InstancedGeometry":
	                    Apply ((InstancedGeometry)node);
	                    break;
	                case "MinimeshGeometry":
	                    Apply((MinimeshGeometry) node);
	                    break;
	                case "Terrain":
	                    Apply((Terrain)node);
	                    break;
                    case "ParticleSystem":
                        Apply((Geometry)node);
                        break;
                    case "BillboardText":
	                case "LinesGeometry3D":
	                case "TexturedQuad2D":
	                    Apply((Geometry)node);
	                    break;
	
	                case "BillboardChain":
	                
	                case "Emitter":
	                case "Attractor":
	                    break;

                    //case "AnimationSet":
                    //    Apply((Animation.AnimationController)node);
                    //    break;
                    case "TextureAnimation":
	                case "EllipticalAnimation":
	                case "LinearInterpolator":
	                case "TranslationInterpolator":
	                case "ScaleInterpolator":
	                case "RotationInterpolator":
	                case "BonedAnimation":
	                case "AnimationClip":
	                case "KeyframeInterpolator_translation":
	                case "KeyframeInterpolator_scale":
	                case "KeyframeInterpolator_rotation":
	                    Apply((Animation.AnimationClip)node);
	                    break;
	                
	                case "Animation":
	                    Apply((Animation.Animation)node);
	                    break;
	
	                case "Appearance":
	                case "SplatAppearance":
	                case "DefaultAppearance":
	                    Apply((Appearance.Appearance) node);
	                    break;
	
	                case "GroupAttribute":
	                    Apply((GroupAttribute) node);
	                    break;
	                case "Shader":
	                case "ProceduralShader":
	                    Apply((Shader) node);
	                    break;
	                case "Material":
	                    Apply((Material) node);
	                    break;
	                case "Texture":
	                case "TextureAtlas":
	                    Apply((Texture)node);
	                    break;

	                case "TextureCycle":
					case "SplatAlpha":
                    case "Diffuse":
                    case "Specular":
                    case "NormalMap":
                    case "Emissive":
                    case "VolumeTexture":
                    case "DUDVTexture":
                    case "CubeMap":
	                    Apply((Layer) node);
	                    break;
	
                    case "Selector":
	                case "Sequence":
	                case "Action":
	                case "Parallel":
	                case "Script": // Keystone.Behavior.Actions.Script
                        Apply((Keystone.Behavior.Behavior)node);
	                    break;
                    // Physics
                    case "RigidBody":
                        Apply((Keystone.Physics.RigidBody)node);
                        break;
                    case "BoxCollider":
                        Apply((Keystone.Physics.Colliders.BoxCollider)node);
                        break;
                    case "SphereCollider":
                        Apply((Keystone.Physics.Colliders.SphereCollider)node);
                        break;
                    case "CapsuleCollider":
                        Apply((Keystone.Physics.Colliders.CapsuleCollider)node);
                        break;
	                default:
	                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported node type '{0}'", node.TypeName));
	                    throw new ArgumentOutOfRangeException();
	            }
        	}
        	catch (Exception ex)
        	{
        		System.Diagnostics.Debug.WriteLine ("ChildSetter.Apply() - " + node.TypeName);
        	}
        }

        public void Apply(SceneNode sn)
        {
            throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'SceneNode'", sn.GetType().Name));
            throw new NotImplementedException();
        }

        public void Apply(EntityNode en)
        {
            throw new NotImplementedException();
        }

        public void Apply(RegionNode rn)
        {
            throw new NotImplementedException();
        }
        
//                /// <summary>
//        /// EntitySystems can be attached to Regions
//        /// </summary>
//        /// <param name="viewpoint"></param>
//        public virtual void Apply(Simulation.StarDigest digest)
//        {
//            if (_parent is Keystone.Portals.Zone || _parent is Keystone.Portals.ZoneRoot)
//            {
//                ((Region)_parent).AddChild(digest);
//            }
//            else
//            {
//                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type Viewpoint", _parent.TypeName)); 
//            }
//        }

        /// <summary>
        /// Viewpoints can be attached to any entity 
        /// </summary>
        /// <param name="viewpoint"></param>
        public virtual void Apply(Viewpoint viewpoint)
        {
            if (_parent is Entities.Entity)
            {
                ((Entities.Entity)_parent).AddChild(viewpoint);
            }
            else
            {
                switch (_parent.TypeName)
                {
                    case "SceneInfo":
                        ((Keystone.Scene.SceneInfo)_parent).AddChild(viewpoint);
                        break;
                    case "Zone":
                        ((Keystone.Portals.Zone)_parent).AddChild(viewpoint);
                        break;
                    case "Root":
                        ((Keystone.Portals.Root)_parent).AddChild(viewpoint);
                        break;
                    default:
                        throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type Viewpoint", _parent.TypeName));
                        throw new NotImplementedException();
                }
            }
        }


        public virtual void Apply(Region region)
        {
            switch (_parent.TypeName)
            {
                case "ZoneRoot":
                case "Interior":
                case "Zone":
                case "Root": // TODO: should override Root's "AddChild" so it can only accept child Regions.  Even a 1x1x1 region universe must have Root->Region0_0_0
                case "Region":
                    ((Region) _parent).AddChild(region);
                    break;
                case "StellarSystem":
                    ((Celestial.StellarSystem)_parent).AddChild(region); // typcially would be an octree region containing planetoids
                    break;

                case "PlayerVehicle":
                case "Vehicle":
                    ((Vehicles.Vehicle)_parent).AddChild(region);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type region", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

   		public virtual void Apply(TileMap.Structure structure)
        {
            switch (_parent.TypeName)
            {
                //case "ZoneRoot": // ZoneRoot can only accept Zone children
        		case "Root": 
                case "Zone":
                case "Region":
                    ((Region) _parent).AddChild(structure);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type structure", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }
                
        // stars can apply to systems
        public virtual void Apply(Celestial.Star star)
        {
            switch (_parent.TypeName)
            {
            	case "Root":
                case "Zone":
                    ((Region)_parent).AddChild(star);
                    break;
                case "StellarSystem":
                    ((Celestial.StellarSystem) _parent).AddChild(star);
                    break;

                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'star'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(Celestial.StellarSystem system)
        {
            switch (_parent.TypeName)
            {
                case "ModeledEntity":
                    // todo: i dont think a stellarSystem should ever be child of a ModeledEntity.. hmm
                    ((ModeledEntity)_parent).AddChild(system);
                    break;
                case "StellarSystem":
                    ((Celestial.StellarSystem) _parent).AddChild(system);
                    break;
                case "Root":
                case "ZoneRoot":
                //case "CelledRegion":
                case "Zone":
                case "Region":
                    ((Region) _parent).AddChild(system);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'StellarSystem'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        // a planet can be child of a star or an entire system
        public virtual void Apply(Celestial.World world)
        {
            switch (_parent.TypeName)
            {
                case "Root":
                case "ZoneRoot":
                //case "CelledRegion":
                case "Zone":
                case "Region":  // TODO: this shouldn't be allowed except so far when i just add a Blue WOrld or Gas Giant from
                                     // my FX ribbon tab, it just adds directly to region and to get it to reload after saving, i need region here
                                     // what i need to do is enforce that the world gets added to a stellar system or something first
                     ((Region) _parent).AddChild(world);
                    break;
                case "StellarSystem":
                    ((Celestial.StellarSystem) _parent).AddChild(world);
                    break;
                case "Star":
                    ((Celestial.Star) _parent).AddChild(world);
                    break;
                case "World":
                    ((Celestial.World) _parent).AddChild(world);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'world'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public void Apply(Portal p)
        {
            throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Portal'", _parent.TypeName));
            throw new NotImplementedException();
        }

        public virtual void Apply(ModeledEntity entity)
        {
            switch (_parent.TypeName)
            {
                case "Root":
                case "ZoneRoot":
                case "Interior":
                case "Zone":
                case "Region":
                case "Background3D":
                    ((Region) _parent).AddChild(entity);
                    break;
                case "Star":
                    ((Star) _parent).AddChild(entity);
                    break;
                case "World":
                    ((World)_parent).AddChild(entity);
                    break;
                case "Container":
                case "Vehicle":
                case "PlayerVehicle":
                    ((Container)_parent).AddChild(entity);
                    break;
                case "ModeledEntity":
                    ((ModeledEntity)_parent).AddChild(entity);
                    break;
                case "DefaultEntity":
                    // todo: wait, should we allow DefaultEntity to be parent to ModeledEntity?
                    ((DefaultEntity)_parent).AddChild(entity);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'ModeledEntity'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(DefaultEntity entity)
        {
            switch (_parent.TypeName)
            {
                case "Root":
                case "ZoneRoot":
                case "Interior":
                case "Zone":
                case "Region":
                case "Background3D":
                    ((Region)_parent).AddChild(entity);
                    break;
                case "Star":
                    ((Star)_parent).AddChild(entity);
                    break;
                case "World":
                    ((World)_parent).AddChild(entity);
                    break;
                case "Container":
                case "Vehicle":
                case "PlayerVehicle":
                    ((Container)_parent).AddChild(entity);
                    break;
                case "ModeledEntity":
                    ((ModeledEntity)_parent).AddChild(entity);
                    break;
                case "DefaultEntity":
                    ((DefaultEntity)_parent).AddChild(entity);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'DefaultEntity'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public void Apply(Keystone.Physics.RigidBody body)
        {
            if (_parent is Entity)
                ((Entity)_parent).AddChild(body);
            else
                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'RigidBody'", _parent.TypeName));
        }

        public void Apply(Keystone.Physics.Colliders.BoxCollider collider)
        {
            if (_parent is Entity)
                ((Entity)_parent).AddChild(collider);
            else
                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'RigidBody'", _parent.TypeName));
        }

        public void Apply(Keystone.Physics.Colliders.SphereCollider collider)
        {
            if (_parent is Entity)
                ((Entity)_parent).AddChild(collider);
            else
                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'RigidBody'", _parent.TypeName));
        }

        public void Apply(Keystone.Physics.Colliders.CapsuleCollider collider)
        {
            if (_parent is Entity)
                ((Entity)_parent).AddChild(collider);
            else
                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'RigidBody'", _parent.TypeName));
        }

        public void Apply (Keystone.DomainObjects.DomainObject domainObject)
        {
            if (_parent is Entity)
                ((Entity)_parent).AddChild(domainObject);
            else
                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'DomainObject'", _parent.TypeName));

        }

        public void Apply(Keystone.Portals.CellFootprint footprint)
        {
            if (_parent is Entity)
                ((Entity)_parent).AddChild(footprint);
            else
                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'CellFootprint'", _parent.TypeName));

        }

        // TODO: following DomainObjectScript apply should be obsolete because now the script
        // is merged in with DomainObject
        //public object Apply(DomainObjectScript script)
        //{
        //    switch (_parent.TypeName)
        //    {
        //        case "DomainObject":
        //            ((DomainObjects.DomainObject)_parent).AddChild(script);
        //            break;
                
        //        default: 
        //            throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'DomainObjectScript'", _parent.TypeName));
        //            throw new NotImplementedException();
        //    }
        //}


        public virtual void Apply(ModelSelector selector)
        {
            switch (_parent.TypeName)
            {
                case "ModelSequence":
                case "ModelSelector": // nested ModelSelector's allowed
                    ((ModelSelector)_parent).AddChild(selector);
                    break;
                case "Interior":
                    ((Interior)_parent).AddChild(selector);
                    break;
                case "Vehicle":
                case "PlayerVehicle":
                case "Star":
                case "World":
                case "BonedEntity":
                case "ModeledEntity":
                    ((ModeledEntity)_parent).AddChild(selector);
                    break;
                case "Background3D":
                    ((ModeledEntity)_parent).AddChild(selector);
                    break;

                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'ModelSelector'", _parent.TypeName));
            }
        }

        public virtual void Apply(Model model)
        {
            switch (_parent.TypeName)
            {
                case "ModelSequence":
                case "ModelSelector":
                    ((ModelSelector)_parent).AddChild(model);
                    break;
                case "Interior":
                    ((Interior)_parent).AddChild(model);
                    break;
                case "Vehicle":
                case "PlayerVehicle":
                case "Star":
                case "World":
                case "BonedEntity":
                case "ModeledEntity":
                    ((ModeledEntity)_parent).AddChild(model);
                    break;

                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Model'", _parent.TypeName));
            }
        }

        public virtual void Apply(Actor3d actor)
        {
            switch (_parent.TypeName)
            {
                case "Model":
                    ((Model)_parent).AddChild(actor);
                    break;
                
                default: 
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Actor3d'", _parent.TypeName));
            }
        }

        public virtual void Apply(Mesh3d mesh)
        {
            switch (_parent.TypeName)
            {
                case "Model":
                    ((Model)_parent).AddChild(mesh);
                    break;
                default: // case "ModelBase": <-- abstract
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Mesh3d'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }


        // TODO: obsolete because now Mini's are created and then added to models dynamically
        // if "UseInstancing" is checked, and are not created during de-serializion of the xml
        public virtual void Apply(MinimeshGeometry minimesh)
        {
            switch (_parent.TypeName)
            {
                case "Model":
                    ((Model)_parent).AddChild(minimesh);
                    break;
                default: // case "ModelBase": <-- abstract
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'MinimeshGeometry'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

    public virtual void Apply(Terrain terrainGeometry)
        {
            switch (_parent.TypeName)
            {
                case "Model":
                    ((Model)_parent).AddChild(terrainGeometry);
                    break;
                default: // case "ModelBase": <-- abstract
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Terrain'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }
                
        public virtual void Apply(Geometry geometry)
        {
            switch (_parent.TypeName)
            {
                case "Model":
                    ((Model)_parent).AddChild(geometry);
                    break;
                default: // case "ModelBase": <-- abstract
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Mesh3d'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(Light light)
        {
            switch (_parent.TypeName)
            {
                case "Root": // regular root is allowed because regular root never has child zones.  
                case "ZoneRoot":  // TODO: ZoneRoot on the other hand should NEVER have entities under it, only it's child Zones.
                case "Interior":
                case "Zone":
                case "Region":
                    ((Region)_parent).AddChild(light);
                    break;
                case "Star":
                case "ModeledEntity":
                    ((ModeledEntity)_parent).AddChild(light);
                    break;
                case "DefaultEntity":
                    ((DefaultEntity)_parent).AddChild(light);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Light'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(Appearance.Appearance app)
        {
            switch (_parent.TypeName)
            {
                case "Model":
                    ((Model) _parent).AddChild(app);
                    break;

                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Appearance'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(Appearance.GroupAttribute ga)
        {
            switch (_parent.TypeName)
            {
                case "SplatAppearance":
                case "DefaultAppearance":
                case "Appearance":
                    ((Appearance.Appearance) _parent).AddChild(ga);
                    break;

                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'GroupAttribute'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(Shader shader)
        {
            switch (_parent.TypeName)
            {
                case "GroupAttribute":
                    ((GroupAttribute) _parent).AddChild(shader);
                    break;
                case "DefaultAppearance":
                case "SplatAppearance":
                case "Appearance":
                    ((Appearance.Appearance) _parent).AddChild(shader);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Shader'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(Appearance.Layer layer)
        {
            switch (_parent.TypeName)
            {
                case "DefaultAppearance":
                case "SplatAppearance":
                case "GroupAttribute":
                    ((GroupAttribute)_parent).AddChild(layer);
                    break;

                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Texture'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(Material material)
        {
            switch (_parent.TypeName)
            {
                case "GroupAttribute":
                case "DefaultAppearance":
                case "SplatAppearance":
                    ((GroupAttribute)_parent).AddChild(material);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Material'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        public virtual void Apply(Texture texture)
        {
            switch (_parent.TypeName)
            {
                case "Layer":     // layer is abstract type should never occur
				case "TextureCycle":
            	case "SplatAlpha":
                case "Diffuse":
                case "Specular":
                case "NormalMap":
                case "Emissive":
                case "VolumeTexture":
                case "DUDVTexture":
                case "CubeMap":
                    ((Layer)_parent).AddChild(texture);
                    break;
                default:
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Texture'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }

        //public virtual void Apply(Keystone.Animation.AnimationController set)
        //{
        //    switch (_parent.TypeName)
        //    {
        //        case "PlayerVehicle":
        //        case "Vehicle":
        //        case "NPCVehicle":
        //        case "ModeledEntity":
        //        case "BonedEntity":
        //            ((ModeledEntity)_parent).AddChild(set);
        //            break;
        //        default:
        //            throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'AnimationSet'", _parent.TypeName));
        //            throw new NotImplementedException();
        //    }
        //    return null;
        //}

        public virtual void Apply(Keystone.Animation.Animation animation)
        {
            if (_parent is Entity)
            {
                ((Keystone.Entities.Entity)_parent).AddChild(animation);
            }
            else
                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Animation'", _parent.TypeName));

            //switch (_parent.TypeName)
            //{
            //    case "BonedEntity":
            //    case "ModeledEntity":
            //    case "Entity":

            //        ((Keystone.Entities.Entity)_parent).AddChild(animation);
            //        break;
            //    default:
            //        throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Animation'", _parent.TypeName));
            //        throw new NotImplementedException();
            //}

        }

        public virtual void Apply(Keystone.Animation.AnimationClip clip)
        {
            if (_parent is Animation.Animation)
            {
                ((Animation.Animation)_parent).AddChild(clip);
            }
            else
                throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Animation'", _parent.TypeName));


            //switch (_parent.TypeName)
            //{
            //    case "BoneedEntity":
            //    case "ModeledEntity":
            //    case "Entity":

            //        ((Keystone.Entities.ModeledEntity)_parent).AddChild(animation);
            //        break;
            //    default:
            //        throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'BonedAnimation'", _parent.TypeName));
            //        throw new NotImplementedException();
            //}
        }

        public virtual void Apply(Keystone.Behavior.Behavior behavior)
        {
            switch (_parent.TypeName)
            {
            	case "Viewpoint":
            		((Entity)_parent).AddChild(behavior);
            		break;
                case "World":
                    ((World)_parent).AddChild(behavior);
                    break;
                case "Star":
                case "Vehicle":
                case "DynamicEntity":
                case "Player":
                case "NPC":
                case "ModeledEntity":
                case "BonedEntity":
                    ((ModeledEntity)_parent).AddChild(behavior);
                    break;
                case "DefaultEntity":
                    ((DefaultEntity)_parent).AddChild(behavior);
                    break;
                case "Sequence":
                case "Selector":
                case "Parallel":
                    ((Keystone.Behavior.Composites.Composite)_parent).AddChild(behavior);
                    break;

                //default:
                //    ((Entity)_parent).AddChild(behavior);
                //    break;

                default: // case "EntityBase": <-- abstract class
                    throw new Exception(string.Format("ChildSetter.Apply() -- Invalid, Unimplemented or Unsupported parent type '{0}' for child type 'Behavior'", _parent.TypeName));
                    throw new NotImplementedException();
            }
        }
        #endregion
    }

    // maybe we can simplify this with a switch statement so we dont need a gazillion different versions of the Setter...
    // because there are fewer group types than there are "leaf" types
    public class RegionSetter : ChildSetter
    {
        private Region _region;

        public RegionSetter(Region region)
        {
            _region = region;
        }

        public override void Apply(Region region)
        {
            _region.AddChild(region);

        }

        public override void Apply(global::Keystone.Celestial.StellarSystem subsystem)
        {
            _region.AddChild(subsystem);
        }

        public override void Apply(Light light)
        {
            _region.AddChild(light);
        }

        public override void Apply(ModeledEntity entity)
        {
            _region.AddChild(entity);
        }


        public override void Apply(ModelLODSwitch lod)
        {
            throw new NotImplementedException();
        }

        public override void Apply(Geometry element)
        {
            //_model.AddChild(element);
        }

        public override void Apply(Appearance.Appearance app)
        {
            //_model.AddChild(element);
        }
    }


    // TODO: this is used in our node.ChildSetter() which is not used at all
    // for now.  We may very well delete it.  The idea originally was that
    // we could do  node.ChildSetter(child) and have it automatically
    // do what we do now with SuperSetter.
    public abstract class ChildSetter
    {
        #region ITraverser Members

        public void Apply(Node node)
        {
            switch (node.TypeName)
            {
                case "Occluder":
                case "Region":
                    Apply((Region) node);
                    break;
                case "Star":
                    Apply((Star) node);
                    break;
                case "StellarSystem":
                    Apply((Celestial.StellarSystem) node);
                    break;
                case "Planet":
                case "World":
                case "Moon":
                    Apply((World) node);
                    break;
                case "DefaultEntity":
                    Apply((DefaultEntity)node);
                    break;
                case "ModeledEntity":
                case "DynamicEntity":
                case "Player":
                case "NPC":
                case "BonedEntity":
                    Apply((ModeledEntity)node);
                    break;
                case "Light":
                case "DirectionalLight":
                case "SpotLight":
                case "PointLight":
                    Apply((Light)node);
                    break;

                case "LODSwitch":


                case "Actor3d":
                case "Mesh3d":
                case "Minimesh":
                case "Terrain":
                case "Billboard":
                case "BillboardChain":
                case "ParticleSystem":
                case "Emitter":
                case "Attractor":


                case "Appearance":
                case "SplatAppearance":
                case "DefaultAppearance":
                case "GroupAttribute":
                case "Material":
                case "Texture":
                case "TextureCycle":
				case "SplatAlpha":
                case "Diffuse":
                case "Specular":
                case "NormalMap":
                case "Emissive":
                case "VolumeTexture":
                case "DUDVTexture":
                case "CubeMap":

                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public object Apply(SceneNode sn)
        {
            throw new NotImplementedException();
        }


        public object Apply(EntityNode en)
        {
            throw new NotImplementedException();
        }

        public object Apply(RegionNode rn)
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(Region region)
        {
            //switch (parent.TypeName)
            //{
            //    case "Region":

            //    case "Vehicle":

            //    default:

            //}
        }

        // stars can apply to systems
        public virtual void Apply(Celestial.Star childbody)
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(Celestial.StellarSystem subsystem)
        {
            throw new NotImplementedException();
        }

        // a planet can be child of a star or an entire system
        public virtual void Apply(Celestial.World world)
        {
            throw new NotImplementedException();
        }

        public object Apply(Portal p)
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(ModeledEntity entity)
        {
            throw new NotImplementedException();
        }

        public object Apply(Terrain terrain)
        {
            throw new NotImplementedException();
        }


        public virtual void Apply(ModelLODSwitch lod)
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(Geometry element)
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(Light light)
        {
            throw new NotImplementedException();
        }

        public virtual void Apply(Appearance.Appearance app)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}