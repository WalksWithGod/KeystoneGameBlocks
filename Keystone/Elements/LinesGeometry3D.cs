using System;
using System.Diagnostics;
using Keystone.Entities;
using Keystone.Extensions;
using Keystone.IO;
using Keystone.Resource;
using Keystone.Traversers;
using Keystone.Types;

namespace Keystone.Elements
{
    public class LinesGeometry3D : Geometry 
    {
        // a type of Geometry we can attach to a Model and have the information retained rather than
        // generated
        Line3d[] mLines;
        Color[] mColors;


        protected LinesGeometry3D(string resourcePath)
            : base(resourcePath)
        {
        }

        public static LinesGeometry3D Create(string id)
        {
            LinesGeometry3D lines;
            lines = (LinesGeometry3D)Repository.Get(id);
            if (lines != null) return lines;
            lines = new LinesGeometry3D(id);
            return lines;
        }
        

        public Line3d[] LineList { get { return mLines; } }

        public Color[] Colors { get { return mColors; } }

        public void AddLine(Line3d line)
        {
            mLines = mLines.ArrayAppend(line);
            // with no color specified we use default White
            mColors = mColors.ArrayAppend(Color.White);
        }

        public void AddLines(Line3d[] lines)
        {
            mLines = mLines.ArrayAppendRange(lines);
            Color[] colors = new Color[lines.Length];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = Color.White;

            mColors = mColors.ArrayAppendRange(colors);
        }

        public void SetEndPoints(int index, Vector3d start, Vector3d end)
        {
            if (mLines == null || index < 0 || index > mLines.Length - 1) throw new ArgumentOutOfRangeException(); 
            mLines[index].SetEndPoints(start, end);
        }

        public void SetColor(int index, Color color)
        {
            if (mColors == null || index < 0 || index > mColors.Length - 1) throw new ArgumentOutOfRangeException();
            mColors[index] = color;
        }

        #region IPageableNode Members
        public override void UnloadTVResource()
        {
        	_tvfactoryIndex = -1;
        }
                
        public override void LoadTVResource()
        {
            //throw new NotImplementedException();
            _tvfactoryIndex = int.MaxValue;

            SetChangeFlags(Keystone.Enums.ChangeStates.GeometryAdded |
                Keystone.Enums.ChangeStates.MatrixDirty, Keystone.Enums.ChangeSource.Self);
        }

        public override void SaveTVResource(string filepath)
        {
            throw new NotImplementedException();
        }
        #endregion

        internal override Keystone.Collision.PickResults AdvancedCollide(Vector3d start, Vector3d end, KeyCommon.Traversal.PickParameters parameters)
        {
            return new Keystone.Collision.PickResults(); 
            //throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldMatrix"></param>
        /// <param name="scene">Usually Context.Scene rather than Entity.Scene since AssetPlacementTool preview Entity's are not connected to Scene.</param>
        /// <param name="model"></param>
        /// <param name="elapsedSeconds"></param>
        internal override void Render(Matrix worldMatrix, Scene.Scene scene, Model model, double elapsedSeconds)
        {
            //throw new NotImplementedException();
            // TODO: it should be impossible for _resourceStatus to be "Loaded" before it's actually loaded
        	//       HOWEVER if paging out, we could start to render here first since it's not synclocked and then
        	//       while minimesh.Render() we page out and set _resourceStatus to Unloading but we're already in .Render()!
        	
        	// NOTE: we check PageableNodeStatus.Loaded and NOT TVResourceIsLoaded because that 
        	// TVIndex is set after Scene.CreateMesh() and thus before we've finished configuring geometry
            if (_resourceStatus != PageableNodeStatus.Loaded ) return;
                
        }


        #region IBoundVolume Members
        // From Geometry always return the Local box/sphere
        // and then have the model's return World box/sphere based on their instance
        protected override void UpdateBoundVolume()
        {
            if (!TVResourceIsLoaded) return;

           	_box.Reset();
                

            for (int i = 0; i < mLines.Length; i++)
                _box.Combine (new BoundingBox(mLines[i].Point[0].x, mLines[i].Point[0].y, 0,
                                       mLines[i].Point[1].x, mLines[i].Point[1].y, 0));

            _sphere = new BoundingSphere(_box);
            DisableChangeFlags(Keystone.Enums.ChangeStates.BoundingBox_TranslatedOnly | Keystone.Enums.ChangeStates.BoundingBoxDirty);
        }
        #endregion
    }
}
