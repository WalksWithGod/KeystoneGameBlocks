
namespace KeyCommon.Traversal
{
    public enum BehaviorResult : byte
    {
        Fail = 0,          // fail will map to 0 and success to 1
        Success = 1 << 0,
        Running = 1 << 1,
        Error = 1 << 2,
        Error_Script_Exception = 1 << 3,  // the .cs script itself has a runtime error
        Error_Script_Invalid_Arguments = 1 << 4 // user has passed invalid arguments to the script
    }
}
