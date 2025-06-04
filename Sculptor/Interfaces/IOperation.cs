using Keystone.Elements;

namespace Sculptor
{
    public interface IOperation
    {
        void Execute(Geometry target);
        void UnExecute();
    }
}
