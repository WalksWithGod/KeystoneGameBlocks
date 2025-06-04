using System;
using Keystone.Extensions;
using Keystone.CSG;
using Keystone.Portals;

namespace KeyEdit.Workspaces.Tools
{
    public class WallSegmentPainter : InteriorSegmentPainter 
    {
        

        public WallSegmentPainter(Keystone.Network.NetworkClientBase netClient)
            : base(netClient)
        {
            LoadVisuals();
        }

        public void SetValue(string layerName, EdgeStyle style)
        {
            LayerName = layerName;
            mValue = style;
        }

        // preview graphic used in FloorplanHud.cs
        private void LoadVisuals()
        {
            Keystone.Appearance.Appearance appearance = Keystone.Celestial.ProceduralHelper.CreateAppearance(MTV3D65.CONST_TV_LIGHTINGMODE.TV_LIGHTING_MANAGED, null, null, null, null, null, true);
            // TODO: I want to be able to turn this preview wall mesh Color.Red when invalid placement.
            appearance.Material.Ambient = Keystone.Types.Color.Green;
            appearance.Material.Opacity = 0.5f;

            Keystone.Elements.Geometry mesh = Keystone.Elements.Mesh3d.CreateBox(2.5f, 1f, 2.5f);
            Keystone.Elements.Model model = (Keystone.Elements.Model)Keystone.Resource.Repository.Create("Model");

            model.AddChild(appearance);
            model.AddChild(mesh);

            Keystone.Entities.ModeledEntity boxEntity = (Keystone.Entities.ModeledEntity)Keystone.Resource.Repository.Create("ModeledEntity");
            boxEntity.AddChild(model);

            Keystone.IO.ClientPager.LoadTVResource(boxEntity, true);
            mSource = boxEntity;
        }

        private void UpdateVisual(uint[] edgeIndices)
        {
            // update scale and position based on mPrevCells

            //if (edgeIndices == null || edgeIndices.Length == 0)
            //    mSource.Visible = false;
            //else
            //    mSource.Visible = true;


            mPrevCells = edgeIndices;
        }

        internal uint[] Edges { get { return mPrevCells; } }

        protected override void MouseDown(Interior celledRegion)
        {
            mStartPickResults = mPickResult;
            UpdateVisual(null);
        }

        protected override void MouseMove(Interior celledRegion)
        {
            // if mouse down == false, then mStartPickResults will be null
            if (mStartPickResults == null) return;

            // TODO: the following should only be used to get a Preview Hud set of walls showing and not the final wall which is done in MouseUp() event.
            CellEdge[] newEdges = celledRegion.GetEdgeList((uint)mStartPickResults.FaceID, (uint)mStartPickResults.TileVertexIndex,
                    (uint)mPickResult.FaceID, (uint)mPickResult.TileVertexIndex);
            if (newEdges == null) return;

            uint[] edgeIndices = new uint[newEdges.Length];
            for (int i = 0; i < newEdges.Length; i++)
                edgeIndices[i] = newEdges[i].ID;

            // NOTE: the following call PainCell() is too expensive for real time so we only apply the wall segments on MouseUp() event.
            //PaintCell(celledRegion.ID, edgeIndices);
            mPrevCells = edgeIndices;

            UpdateVisual(edgeIndices);
        }

        protected override void MouseUp(Interior celledRegion)
        {
            if (celledRegion != null && mStartPickResults != null)
            {
                // TODO: 
                CellEdge[] newEdges = celledRegion.GetEdgeList((uint)mStartPickResults.FaceID, (uint)mStartPickResults.TileVertexIndex,
                        (uint)mPickResult.FaceID, (uint)mPickResult.TileVertexIndex);

                if (newEdges != null)
                {
                    uint[] edgeIndices = new uint[newEdges.Length];
                    for (int i = 0; i < newEdges.Length; i++)
                        edgeIndices[i] = newEdges[i].ID;

                    PaintCell(celledRegion.ID, edgeIndices);
                }
            }

            mPrevCells = null;
            mStartPickResults = null;
        }
    }
}
