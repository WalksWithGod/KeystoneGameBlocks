namespace Keystone.Interfaces
{
    public interface INotifyDeviceReset
    {
        void OnBeforeReset();
        void OnAfterReset();
    }
}