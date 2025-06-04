
namespace Lidgren.Network
{
    /// <summary>
    /// Although structs can implement interfaces, it results in a boxing of the value which negates any performance advantage of 
    /// using the struct in the first place.  Thus, all IRemotableType's should be implemented as classes.
    /// Lidgren is only responsible for guaranteeing that incoming and outgoing messages are read/written properly 
    /// and prevent buffer overruns throughout, however it is the Application's responsibility to verify that 
    /// incoming IRemotableType's are read properly given the length of the message.buffer and the data that is to be read.
    /// </summary>
    public interface IRemotableType
    {
        int Type { get; }
        NetChannel Channel { get; } //default channel that it should be sent on.  
        void Read(NetBuffer buffer);
        void Write(NetBuffer buffer);
    }
}
