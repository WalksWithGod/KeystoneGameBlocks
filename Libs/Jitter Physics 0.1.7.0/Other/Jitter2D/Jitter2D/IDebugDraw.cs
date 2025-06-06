﻿/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/

#region Using Statements
using Jitter2D.LinearMath;
#endregion

namespace Jitter2D
{

    public interface IDebugDrawable
    {
        void DebugDraw(IDebugDrawer drawer);
    }

    /// <summary>
    /// Interface for providing the possibility to get drawable debug information
    /// from an object.
    /// </summary>
    public interface IDebugDrawer
    {
        void SetColor(float r, float g, float b, float a);
        void DrawLine(JVector start, JVector end);
        void DrawPoint(JVector pos);
        //void DrawPoint(JVector pos, float r, float g, float b, float a);
        void DrawTriangle(JVector pos1, JVector pos2, JVector pos3);
    }
}
