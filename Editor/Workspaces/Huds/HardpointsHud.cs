using KeyCommon.Traversal;
using Keystone.Cameras;
using Keystone.EditTools;
using Keystone.Entities;
using Keystone.Hud;
using Keystone.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyEdit.Workspaces.Huds
{
    public class HardpointsHud : ClientHUD
    {
        private HardpointEditorWorkspace mHardpointsWorkspace;

        public HardpointsHud(HardpointEditorWorkspace workspace) : base()
        {

            mHardpointsWorkspace = workspace;

            InitializePickParameters();
                        
        }

        private void InitializePickParameters()
        {
            mPickParameters = new PickParameters[1];

            // KeyCommon.Flags.EntityAttributes.ExteriorRegion
            KeyCommon.Flags.EntityAttributes excludedObjectTypes =
                KeyCommon.Flags.EntityAttributes.HUD | KeyCommon.Flags.EntityAttributes.Background | KeyCommon.Flags.EntityAttributes.Structure; 


            // recall that "excluded" types will skip without traversing children whereas "ignored" will traverse children
            KeyCommon.Flags.EntityAttributes ignoredObjectTypes =
                KeyCommon.Flags.EntityAttributes.Region | KeyCommon.Flags.EntityAttributes.Root;

            PickParameters pickParams = new PickParameters
            {
                T0 = AppMain.MINIMUM_PICK_DISTANCE,
                T1 = AppMain.MAXIMUM_PICK_DISTANCE,
                SearchType = PickCriteria.Closest,
                SkipBackFaces = true,
                Accuracy = PickAccuracy.Face,
                ExcludedTypes = excludedObjectTypes,
                IgnoredTypes = ignoredObjectTypes,
                FloorLevel = int.MinValue
            };

            //p.ExcludedTypes = EntityFlags.AllEntityTypes & ~EntityFlags.HUD;

            mPickParameters[0] = pickParams;
        }

        public override void UpdateBeforeCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateBeforeCull(context, elapsedSeconds);
            // it can take a bit for the context to get connected to the region and then for that region
            // to get connected to the scene.  im not sure why... TODO: investigate some other time if this is safe and not indicative of some other fundamental issue
            if (context.Region == null || context.Region.RegionNode == null) return;

            Keystone.Collision.PickResults pickResult = context.Workspace.MouseOverItem;
            Keystone.EditTools.Tool currentTool = context.Workspace.CurrentTool;


            // tool previews
            if (mLastTool != currentTool)
            {
                OnEditTool_ToolChanged(currentTool);
                mLastTool = currentTool;
            }


            IHUDToolPreview currentPreviewGraphic = null;

            if (currentTool as TransformTool != null)
            {
                TransformTool transformTool = (TransformTool)currentTool;


                if (transformTool as PlacementTool != null)
                {
                    if (mPreviewGraphic as PlacementPreview == null)
                    {
                       
                        string cloneID = Keystone.Resource.Repository.GetNewName(currentTool.Source.TypeName);
                        ModeledEntity clone = (ModeledEntity)currentTool.Source.Clone(cloneID, true, false);

                        currentPreviewGraphic = new PlacementPreview(transformTool, clone,
                                                                    AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                       
                    }
                }
                else
                {
                    if (mPreviewGraphic is TransformPreview == false && transformTool.Source != null)
                    {
                        currentPreviewGraphic = new TransformPreview(transformTool, (ModeledEntity)transformTool.Source,
                                                              AddHUDEntity_Immediate, AddHudEntity_Retained, RemoveHUDEntity_Retained);
                        // when first initiating the transform preview for Move/Rotate/Scaling tools, we must init ComponentTranslation to Source.Translation
                        // or else the Source entity will snap to origin.
                        transformTool.ComponentTranslation = transformTool.Source.Translation;
                    }
                }

                // NOTE: once the placement tool actually drops the hardpoint, it uses the target entity (eg vehicle or pylon) as parent and pickResult.ImpactPointLocalSpace
                transformTool.ComponentTranslation = pickResult.ImpactPointRelativeToRayOriginRegion ;
            }
            else
            {
                currentPreviewGraphic = new NullPreview();
                context.PickParameters = mPickParameters[0];
            }

            if (mPreviewGraphic != currentPreviewGraphic && currentPreviewGraphic != null)
            {
                // dispose existing and assign new if the new one is NOT null, otherwise no change has occurred
                if (mPreviewGraphic != null)
                {
                    mPreviewGraphic.Clear();
                    mPreviewGraphic.Dispose();
                }

                mPreviewGraphic = currentPreviewGraphic;
            }


            if (mPreviewGraphic != null)
                mPreviewGraphic.Clear();

            if (mPreviewGraphic != null && pickResult != null && pickResult.Entity != null)
                mPreviewGraphic.Preview(context, pickResult);
            

            // TODO:  "cam_zoom" is a zoom for a specific branch... say our 3dHUDRoot only
            float cameraZoom = 1f;
            if (context.Viewpoint.BlackboardData != null)
                cameraZoom = context.Viewpoint.BlackboardData.GetFloat("cam_zoom");


            // TODO: can we use translation delta for grid?
            if (this.Grid.Enable)
            {
                // NOTE: Grdid 2d drawing occurs in UpdateAfterClear();
               // UpdateGridZoom(cameraZoom);
            }



        }

        public override void UpdateAfterCull(RenderingContext context, double elapsedSeconds)
        {
            base.UpdateAfterCull(context, elapsedSeconds);

            // TODO: can we use translation delta for grid?
            if (this.Grid.Enable && context.Viewpoint.Region != null)
            {
                Vector3d offset = -context.GetRegionRelativeCameraPosition(context.Viewpoint.Region.ID); // -context.Position
                Keystone.Immediate_2D.Renderable3DLines[] gridLines =
                    Grid.Update(offset, (bool)context.GetCustomOptionValue(null, "show axis indicator"));

                AddHUDEntity_Immediate(context.Viewpoint.Region, gridLines);
            }

        }
    }
}
