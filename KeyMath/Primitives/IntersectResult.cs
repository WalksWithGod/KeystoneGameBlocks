namespace Keystone.Types
{
    public enum IntersectResult
    {
        OUTSIDE = 0,
        INTERSECT,
        // unlike partially visible, these are good for quadtree node culling to eliminate testing children.  sicne by definition, if a parent is fully visible (or not visible) than so are its children.
        INSIDE
    }
}
