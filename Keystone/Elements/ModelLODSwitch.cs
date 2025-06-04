using System;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Elements
{
    // TODO: This is not used at the moment.  ModelSelector is being used for everything

    // https://developer.valvesoftware.com/wiki/File:Lodcomparison2.png

    //When to use LOD models  https://developer.valvesoftware.com/wiki/LOD_Models#When_to_use_LOD_models
     
    //It may be worth creating one or more LOD models if your model has: 
    //small details which increase the polycount but cannot be seen at a distance 
    //rounded or organic areas using a lot of polygons to suggest 'soft' edges 
    //physic props - you never know where the player is going to carry them 
    //expensive shaders (e.g. Normal Maps, Reflective Materials or the eye shader.) 

    //Some cases in which an LOD model is unlikely provide any noticeable performance increase:
    // your model already has a small polycount or is low detail 
    //you don't think you can reduce the polycount by at least 30% (the model has no round/organic features)
    // a static model that is only going to be used in areas with a good performance 

    //Choosing where to draw the line is your decision; but it may be helpful to look at the HL2 models to see when Valve has created LOD models and when they thought it was unnecessary.

//    Modeling LODs
 
//Tips
 
//The work in the model editor should be kept to a minimum. You don't want to spend as much time on your LOD models as you did on the original model! Our goal is to make a few modifications on the model that will reduce the polycount within a few minutes. However, we don't want to make a whole new model, redo our UV map or something else that consumes a lot of time.
 
//Get used to the fact that a LOD model can only be seen from a large distance. If a player wants to see details, he'll step closer. Since the player does not focus on the details, we can remove them without him noticing if it is done in a subtle way. Things you don't want to do:
// Changing the outline of the model or anything that gives cover. You can remove the seats within a car, but don't remove the roof.
// Moving details on the UV map. The texture will most likely get stretched during the process, however only if details move from one place to another it'll be noticeable.
// Smoothing groups changing in a way that one large area turns from shadowed into brightened or similar.
 
//Techniques
 
//Removing small details 

//Often details are too small to be seen from a distance - they'll simply not be drawn with the resolution of modern day hardware or can't be noticed.
 
//Reducing polys of complex surfaces 

//This can be done by using the tools your modeling package offers. It'll weld vertices without destroying the UV map.
 
//Rebuilding complex geometry with very simple one 

//This is only recommend if the automatic tools fail as it is rather timeconsuming and needs a new UV map (use the same skin in any case). Rebuild the basic shape of the model (or a part of it) as low poly as possible.
 
//Use .QC commands to simplify the model 

//By using pre-existing .QC Commands you can automatically remove or replace parts of your models mesh during compile without having to edit them yourself.
 
//Use a low polycount mesh for your model's shadow 

//Often overlooked, you should create or re-use an existing low poly LOD mesh as the mesh used for dynamic shadow rendering. (prop_static casts lightmap shadows, useless in this case.)
 
//Replace an expensive texture/shader with a simpler one 

//If your using an "expensive" shader, such as the eye shader or a normal map, on a model you can replace the texture which uses it with a less expensive one for specific LOD levels.
 
//Simplify your models skeleton 

//More useful for character models, you can simplify the skeleton by replacing or collapsing bones when the model is a certain distance from the viewer. For example, you could simplify the hands by removing the finger bones leaving just the hand bone. This will result in the fingers being out straight, but at the distance the model is, the viewer wouldn't notice this.
 
//Disable facial animation 

//For character models, it's pointless having the face animated when in the distance and it can't be seen.
 

    /// <summary>
    /// This is an LOD switch specifically for Models.  The highest LOD 
    /// child should be (but doesnt have to) a Models node. 
    /// It should be noted that an LOD is NOT an "element" nor is it a 
    /// boundVolume.  Instead, it is a switch for a "Model" and is always
    /// the child of a Model and thus you can think of the "position"
    /// of this LOD in world space as being the "position" of the Model
    /// itself.  This is very different than how X3D and other scene graphs
    /// implement a LOD node but I feel this way is superior because
    /// it acts strictly as a "switch" and not as an invisible virtual entity in the world
    /// </summary>
    public class ModelLODSwitch : ModelSwitch
    {
        protected const int DISTANCE_CHECK_INTERVAL = 500; //ms
        protected const double DEFAULT_SEPERATION = 10.0F;

        protected double[] _switchDistances;
        protected bool _isOrdered ;

        // TODO: YOu know, i dont think there would be anything wrong at all with
        // LOD's being able to host an Appearance node... hrm... but... well..
        // complicates the plugin... but i think just in terms of how i traverse,
        // when i hit an LOD, now the Geometry can use the Appearance that is supplied
        // here to the root LOD which also means it can even use a seperate LOD for appearance
        // well not terribly important for now.  we'll see.
        internal ModelLODSwitch (string id) : base (id) 
        {
            Shareable = false; // transformable derived nodes can never be shared
        }

        // TODO: I think LODSwitches should not be shareable... it's too difficult to modify
        // one particular shared instance.  Or do we prompt for user to de-link the entire
        // entity if they wish to only have the changes apply to the current instance?
        public static ModelLODSwitch Create (string id)
        {
            ModelLODSwitch lodswitch = (ModelLODSwitch)Keystone.Resource.Repository.Get(id);
            if (lodswitch != null) return lodswitch;
            lodswitch = new ModelLODSwitch(id);
            return lodswitch;
        }

        internal override ChildSetter GetChildSetter()
        {
            throw new NotImplementedException();
        }


        #region ITraversable Members
        public override object Traverse(ITraverser target, object data)
        {
            throw new Exception("The method or operation must be overriden by derived types.");
        }
        #endregion

        //#region ResourceBase members
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="specOnly">True returns the properties without any values assigned</param>
        ///// <returns></returns>
        //public override Settings.PropertySpec[] GetProperties(bool specOnly)
        //{

        //    Settings.PropertySpec[] tmp = base.GetProperties(specOnly);
        //    Settings.PropertySpec[] properties = new Settings.PropertySpec[2 + tmp.Length];
        //    tmp.CopyTo(properties, 2);

        //    properties[0] = new Settings.PropertySpec ("switchmodes", typeof(string).Name);
        //    properties[0] = new Settings.PropertySpec("switchdistances", _switchDistances.GetType().Name);
        //    properties[1] = new Settings.PropertySpec("ordered", _ordered.GetType().Name);
        //    if (!specOnly)
        //    {
        //        properties[0].DefaultValue = SwitchModesToString();
        //        properties[0].DefaultValue = (int)_entityFlags;
        //        properties[1].DefaultValue = _ordered;
        //    }

        //    return properties;
        //}

        //// TODO: this should return any broken rules ?
        //public override void SetProperties(Settings.PropertySpec[] properties)
        //{
        //    if (properties == null) return;
        //    base.SetProperties(properties);

        //    for (int i = 0; i < properties.Length; i++)
        //    {
        //        // use of a switch allows us to pass in all or a few of the propspecs depending
        //        // on whether we're loading from xml or changing a single property via server directive
        //        switch (properties[i].Name)
        //        {
        //            case "switchdistances":
        //                //_entityFlags = (EntityFlags)properties[i].DefaultValue;
        //                break;
        //            case "ordered":
        //                break;
        //        }
        //    }
        //}
        //#endregion


        /// <summary>
        /// Every subsequent child added gets increased seperation
        /// </summary>
        /// <param name="child"></param>
        /// <remarks></remarks>
        public new void AddChild(Model child)
        {
            // defaults it to previous distance + default_seperation
            AddChild(child, GetNextDistance());
        }

        public void AddChild(Model child, double distance)
        {
            AddSwitchDistance(distance);
            base.AddChild(child);
            _isOrdered = false;
        }

        // TODO: override RemoveChild and automatically remove the switchdistance
        // at that child's index?
        //public override void RemoveChild(Node child)
        //{
        //    base.RemoveChild(child);
        //}

        protected void AddSwitchDistance(double dist)
        {
            // TODO: use the following type of heuristic from SGL instead if no distance is provided?
            // this would of course require that users add the children in the order from closest to furthest.
            //double horiz = (screen_size_x * 0.5f) / tan(fov_x * 0.5f);
            //double vert = (screen_size_y * 0.5f) / tan(fov_y * 0.5f);
            //double max = (horiz > vert) ? horiz : vert;

            //double fov = (horiz > vert) ? fox_x : fov_y;

            //const double cTanPi8 = tan(M_PI / 8.0);
            //double lod_aspect_2 = (512.0f / cTanPi8) / max;
            //double lod_scale = (fov / (M_PI / 4.0)) * lod_aspect_2;
            if (_switchDistances == null)
            {
                _switchDistances = new double[1];
                _switchDistances[0] = dist;
            }
            else if (_switchDistances.Length <= mChildren.Count)
            {
                double[] tmp = new double[mChildren.Count + 1];
                Array.Copy(_switchDistances, tmp, _switchDistances.Length);
                _switchDistances = tmp;
                SetSwitchDistance(tmp.Length - 1, dist);
            }
        }

        protected double GetNextDistance()
        {
            if (mChildren == null ||mChildren.Count == 0)
                return DEFAULT_SEPERATION;

            return _switchDistances[mChildren.Count - 1] + DEFAULT_SEPERATION;
        }

        public double GetSwitchDistance(int index)
        {
            if ((index < 0) || (index > _switchDistances.Length - 1)) throw new ArgumentOutOfRangeException();
            return  Math.Sqrt(_switchDistances[index]);
        }

        public void SetSwitchDistance(int index, double value)
        {
            if ((index < 0) || (index > _switchDistances.Length - 1)) throw new ArgumentOutOfRangeException();
            _switchDistances[index] = value*value;
            //store them as squared distances to make distance computation simpler
            _isOrdered = false;
        }


        /// <summary>
        /// Checks from first array element to highest and exits when it finds the highest switch distance 
        /// that is still less than or equal to the distance
        /// </summary>
        /// <param name="fromPos">Typically the camera's position.</param>
        /// <param name="toPosition">Typically the Model node's position.</param>
        /// <param name="selectedIndex"></param>
        /// <returns></returns>
        public Node Select(Vector3d fromPos, Vector3d toPosition, ref int selectedIndex)
        {
            // TODO: i use distance from camera alot in various parts of code, i should probably just compute it for all visible ModelBAses during cull and cache.
            double distSquared = Vector3d.GetDistance3dSquared(fromPos, toPosition);
            //double distSquared =
            //    (double)
            //    (Math.Pow(fromPos.x - toPosition.x, 2) + Math.Pow(fromPos.y - toPosition.y, 2) +
            //     Math.Pow(fromPos.z - toPosition.z, 2));
            return Select(distSquared, ref selectedIndex);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distanceSquared">Pass in '0' here to always return the highest LOD child.</param>
        /// <param name="selectedIndex"></param>
        /// <returns></returns>
        public Node Select(double distanceSquared, ref int selectedIndex)
        {
            selectedIndex = -1;
            if (mChildren.Count == 0) return null;

            selectedIndex = 0; // since we have at least one child, we can update the selectedIndex to 0

            const string text = "Distance must be non negative.";
            if (distanceSquared < 0) throw new ArgumentOutOfRangeException(text);
            if (mChildren.Count == 1) return mChildren[0];

            if (!_isOrdered) OrderByDistance();
            if (distanceSquared == 0) return mChildren[0];

            

            Node current = null;
            //NOTE: At one point I had an elapsed time check to reduce the number of times we needed to check LOD
            // distances but then it occurred to me that doing so made it not possible to do things like "zoom"
            // or render to an MFD because we would ignore the distance and just return the cached node.  So ive removed that.
            int i = 0;
            do
            {
                // TODO: add a buffer zone so the LOD switching doesnt occur at a single line but rather a begin and end zone
                // Basically, same as we do with the terrain splat vs regular texturing switching.
                if (_switchDistances[i] <= distanceSquared)
                {
                    current = mChildren[i];
                    selectedIndex = i;
                }
                else break;

                i++;
            } while (i > mChildren.Count - 1);

            if (current == null)
            {
                current = mChildren[0];
                selectedIndex = 0;
            }

            return current;
        }

        /// <summary>
        /// Sorts from smallest to highest.  eg. array element 0 = 10, array element 1 = 60, array element 2 = 110
        /// </summary>
        /// <remarks></remarks>
        private void OrderByDistance()
        {
            // an in-place insertion sort is the best sort for such trivial case. Yes i know this isnt very .net. 
			// i could at least make it a static method or extension method            
            if (mChildren.Count > 1)
            {
                for (int i = 1; i < mChildren.Count; i++)
                {
                    double distance = _switchDistances[i];
                    Node m = mChildren[i];
                    int j = i;
                    while ((j > 0) && (_switchDistances[j - 1] > distance))
                    {
                        _switchDistances[j] = _switchDistances[j - 1];
                        mChildren[j] = mChildren[j - 1];
                        j--;
                    }
                    _switchDistances[j] = distance;
                    mChildren[i] = m;
                }
            }
            _isOrdered = true;
        }
    }
}