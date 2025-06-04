using System;


namespace Keystone.EditTools
{
    public enum ToolType
    {
        Select,
        InteriorSelectionTransformTool,
        NavPointPlacer,
        Rotate,
        Scale,
        Position,
        HingeEdit
        // TODO: some "tools" may wind up being Tools that are in the game itself
        // such as a welding item, a weapon, a lockpick.  I think there is no distinctions
        // between a Rotate tool in the editor and a tool that allows you to manipulate crates in a game.
        // However, in our ClientController.Tool   {get {return mPlayerEntity.Tool; }}
        // we may have to have a level of indirection where we are returning the player's tool... right?  Or what?
    }

    public interface ITool : IDisposable 
    {

        //bool HasInputCapture();
    }
}
