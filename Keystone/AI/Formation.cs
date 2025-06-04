using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Keystone.AI
{
    // Utility function AI (used by The Sims)
    // http://intrinsicalgorithm.com/IAonAI/2011/12/getting-more-behavior-out-of-numbers-gdmag-article/
    // http://www.gamasutra.com/blogs/MiguelNieves/20121212/183350/Artificial_Intelligence_Utility_builds_Character.php
    // - I also like the above from the context of letting players script their own AI.
    //   Behavior trees are obviously useful for some things, but Utility seems better for strategic AI.
    //  
    //
    //
    // collapsable formations video
    // http://aigamedev.com/open/releases/pathengine-formations/
//    Technical Details

//The default behavior in this demo is designed to have a very low update cost per individual agent, a minimal number of long distance pathfinding requests, using pathfinding based on the size of a single agent, and allowing the group to adapt to obstructions. Here's how it works in practice:

//   1.

//      A path is planned according to the size of the maximum size of an individual unit in the formation, rather than the formation size.
//   2.

//      The corridor around the path is used to calculate the ideal formation width, and positions available.
//   3.

//      Sub-formations are created based on the distance between entities, so occasionally groups split up to move around obstacles.
//   4.

//      Only one long distance pathfinding request is used per sub-formation, which reduces the cost of searching for paths.
//   5.

//      While following the path, there's a piece of code responsible for organising movement orders within sub-formations using local queries only.

//Thomas Young points out another few interesting details about this demo:

//    “Local, short distance pathfinding requests are currently used to move to start positions, but in most cases these are not actually required, and will early out on a line collision test direct to goal.

//    This is a good example, I guess, of the extreme usefulness of a paired pathfinding/collision setup, and it should be straightforward to apply this kind of approach to any situation where paired collision and pathfinding queries are available! ”

//Voxel & BSP Processing

//Voxel-based approaches to processing polygon soup have been gaining traction lately. However, compared to solutions based on processing polygonal geometry, the decision of which to use is not so clearcut. PathEngine's latest release includes both, so I took a few comparison screenshots to visualize the differences in practice.

//In the followings images, you'll see the following from left to right:

//    *

//      Blue — The original input geometry with polygon outlines.
//    *

//      Grey — Walkable surface extracted with voxel processing.
//    *

//      Green — Same walkable area extracted using BSP processing.

    class Formation
    {
    }
}
