using System;
using System.Collections.Generic;
using Keystone.Elements;
using Keystone.Entities;
using MTV3D65;

namespace Keystone.EditOperations
{
    //public class RaiseAlt : ICommand // it's undo is LowerAlt and vice versa
    //{
    //    Brushes.PointBrush _brush;
    //    Stack <TV_3DVECTOR> _deltas = new Stack <TV_3DVECTOR>( );
    //    Terrain _target;
        
    //    /// <summary>
    //    /// 
    //    /// </summary>
    //    /// <param name="brush">The brush to be used during this operation </param>
    //    /// <param name="t"></param>
    //    public RaiseAlt (Brushes.PointBrush brush)
    //    {
    //        if (brush == null) throw new ArgumentNullException();
    //        _brush = brush;
    //    }
        
    //    #region IOperation Members
    //    public void Execute(Geometry  terrain)
    //    {
    //        if (!(terrain is Terrain)) throw new ArgumentException("Invalid type.");
    //        _target = terrain;

    //        // we could set the _brush state and trap it/hold it
    //        // then callback on completion 
            
    //        // on BeginExecute same thing
    //        // we can trap the brush, maybe add ourselves as a IListener
    //        // and receive notifications as it moves....
    //        // but i mean, we need to get exclusive access during the operation.
    //        // perhaps quite litterally, Brush.InputExclusivity = true;
    //        // then we can wait til ESC is pressed and we'll end the operation
    //        // or hrm....
            
            
    //        //TV_3DVECTOR start, end;
    //        //start = _meshPointer(i).GetPosition;
    //        //    end = _meshPointer(i).GetPosition;
    //        //    start.y = 1000;
    //        //    end.y = -1000;
            
    //        // he determines the spacing between bristles as eiterh 1/4 or 1 quad vertex length
    //        //  so we need to compute this so we can work at any scale.
            
    //        //// determine which bristles are actually over a vertex
    //        //if (AdvancedCollision(vStart, vEnd, CONST_TV_OBJECT_TYPE.TV_OBJECT_LANDSCAPE)).bHasCollided
    //        //{
    //        //    // enable it and set the position of the bristle at the impact point
                
    //        //}
    //        //// hrm.. but what about terrain borders?  i suppose we'd need two seperate RaiseAlt operators?
    //        //// probably easiest to keep it at just one and to ignore bristles (treat them as out of bounds)
    //        //// those that outside of this landscapes bounds
            
    //        //TV_3DVECTOR delta;
            
    //        //for (int i = 0 ; i < _brush.BristleCount ; i++ ) 
    //        //{
    //        //    if (_meshPointer(i).IsEnabled)

    //        //        delta.x = _meshPointer(i).GetPosition.x;
    //        //        delta.z = _meshPointer(i).GetPosition.z;
    //        //        delta.y = _meshPointer(i).GetPosition.y + (_fBrushStep * clsEngine.GetTick * 0.01F)
    //        //    ;
    //        //        _deltas.Push(delta);

    //        //        terrain.AdjustAltitude(deltaX, deltaY, deltaZ);
    //        //}   
            
    //        ////hrm proper handling of the COmmand pattern is to
    //        //// 1) call operation on the item being operated on thus:
    //        ////   RaiseAlt = new RaiseAlt (Brush b)
    //        ////   then you compute any deltas
    //        //// then you call terrain.Operation(RaiseAlt) and push RaiseAlt on the stack for Undo purposes
    //        //// 
    //        //// and the terrain would do RaiseAlt.Execute(this)
    //        //// and then the RaiseAlt would apply those results to the land
    //        //// and save a reference to the Terrain for undo purposes
    //        //_target = (Terrain)terrain;
    //    }
        
    //    public void UnExecute()
    //    {
    //    }
    //    #endregion
        
    //}
}
