

namespace Keystone.EditTools
{
    // a brush that can be used to drag and drop (via click and hold) a "selection" box across items
    // these items can then be operated on simulatenously.  
    // Alternatively, instead of dragging (click and hold) you can click once 
    // and move the mouse and hit SHIFT + click again to grab everything between the min/max bounds
    //  - note: min != max
    // Alternatively, the group select brush can have items manually inserted into the box
    // via CTRL+ left mouse click
    public class GroupSelect : Tool
    {
        public GroupSelect(Keystone.Network.NetworkClientBase netClient) : base(netClient)
        {}

        public override void HandleEvent(Keystone.Events.EventType type, System.EventArgs args)
        {
            throw new System.NotImplementedException();
        }
    }
}
