using Keystone.Elements;
using Keystone.Collision;
using Keystone.Types;
using System;
using KeyCommon.Flags;

namespace Keystone.Entities
{

    public class DefaultEntity : Entity
    {

        // NOTE: Entities don't need static Create's because they can never be shared.
        public DefaultEntity(string id)
            : base(id)
        {

            // Triggers and Occluder entities would NOT be .VisibleInGame == true
            // However when in EDIT mode, Cull traverser will add as VisibleItem so that
            // the HUD can iconize a clickable proxy so that the DefaultEntity can be
            // edited in the plugin

            SetEntityAttributesValue((uint)EntityAttributes.Awake, true);
            SetEntityAttributesValue((uint)EntityAttributes.Awake, true);
        }

        #region ITraversable Members
        public override object Traverse(Keystone.Traversers.ITraverser target, object data)
        {
            return target.Apply(this, data);
        }
        #endregion

        #region IBoundVolume
        float DEFAULT_BBOX_SIZE = 1f;
        // DefaultEntities only have a boundingBox of DEFAULT_BBOX_SIZE
        // It does NOT combine volume of child Entities!
        // Only EntityNode's use hierarchical bound volumes of child EntityNodes.
        protected override void UpdateBoundVolume()
        {
            if ((mChangeStates & Enums.ChangeStates.BoundingBox_TranslatedOnly & Enums.ChangeStates.BoundingBoxDirty) == Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly)
            {
                // TODO: test the above & test to see if it only triggers the following code
                // if just BoundingBox_TranslatedOnly is set
                DisableChangeFlags(Enums.ChangeStates.BoundingBox_TranslatedOnly);

                mBox.Max -= mTranslationDelta;
                mBox.Min -= mTranslationDelta;
                mSphere = new BoundingSphere(mBox);
            }
            else if (mChildren != null && mChildren.Count > 0)
            {
                // initialize with region space boundingBox of default size
                mBox = new BoundingBox(mTranslation, DEFAULT_BBOX_SIZE);

                BoundingBox childboxes = BoundingBox.Initialized();
                for (int i = 0; i < mChildren.Count; i++)
                {
                    // NOTE: It was always a mistake for a NON REGION Entity.cs derived class
                    // to include the bounding volume of child _ENTITIES_ (child Models and ModelSelectors are OK). 
                    // The SceneNode's are  responsible for Hierarchical bounding volumes and Entities bounding volumes are
                    // only for themselves and never include their child entities.
                    //
                    // NOTE: we do NOT include child entities here.  Leave that to EntityNode.cs
                    //       childboxes = BoundingBox.Combine(childboxes, ((BoundTransformGroup)mChildren[i]).BoundingBox);
                    if (mChildren[i] is Entity)
                        continue;
                    else if (mChildren[i] is BoundTransformGroup) // BoundTransformGroup that is NOT an Entity
                    {
                        BoundingBox transformedChildBox;
                        transformedChildBox = BoundingBox.Transform(((BoundTransformGroup)mChildren[i]).BoundingBox, ((BoundTransformGroup)mChildren[i]).RegionMatrix);

                        childboxes.Combine(transformedChildBox);
                    }
                }
                mBox.Combine(childboxes);
                DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBoxDirty | Enums.ChangeStates.BoundingBox_TranslatedOnly);
            }

            if (mBox == BoundingBox.Initialized()) return; // if the box still hasn't been set

            mSphere = new BoundingSphere(mBox);
            double radius = mSphere.Radius;

            // TODO: with regards to planets, we could hardcode this such that
            // it's always visible within half region range for worlds, and 1/8th region range for moons
            // the problem is, in order to render even the icon for the world, it must pass
            // the cull test.   however, if we to treat it properly like a HUD item, we would
            // only find the worlds we detected via sensors (including visual sensors (telescopes and eyeballs))
            // In this way, the world would cull, but still show up in the hud.
            // This way the hud would independantly query for the nearby worlds and generate
            // proxies for them.
            // 
            // http://astrobob.areavoices.com/2012/01/05/what-would-the-sun-look-like-from-jupiter-or-pluto/
            // 30 arc minutes equal 1/2 degree or the diameter of the sun and moon. 
            double angleDegrees = 0.5d;
            double angleRadians = Keystone.Utilities.MathHelper.DEGREES_TO_RADIANS * angleDegrees;
            // http://www.bautforum.com/showthread.php/106931-Calculating-angular-size
            // sin(angle/2) = radius/(distance+radius)
            _maxVisibleDistanceSq = radius / System.Math.Sin(angleRadians * .5); // / Radius;
            _maxVisibleDistanceSq *= _maxVisibleDistanceSq;


            //_maxVisibleDistanceSq = float.MaxValue;
            // below calc is same as above....hrm...
            double distance = radius / Math.Tan(angleRadians / 2d);


            distance *= distance;
        }
        #endregion
    }
}
