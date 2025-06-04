namespace Keystone.Interfaces
{
    public interface IObserver
    {
        bool HandleUpdate(ISubject notifier);
    }
}