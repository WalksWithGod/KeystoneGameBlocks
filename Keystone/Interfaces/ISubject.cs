namespace Keystone.Interfaces
{
    public interface ISubject
    {
        void Notify(); // TODO: Notify should not be public.  it should be internal.  
        void Attach(IObserver observer);
        void Detach(IObserver observer);
    }
}