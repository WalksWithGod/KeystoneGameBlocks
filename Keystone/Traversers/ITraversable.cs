namespace Keystone.Traversers
{
    public interface ITraversable
    {
        object Traverse(ITraverser target, object data);
    }
}