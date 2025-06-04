using System;
using System.Collections;
using System.Collections.Generic;

namespace Keystone.Resource
{
    // TODO: See notes in RenderSurface.cs where I maintain a privat static reference to RenderSurfacePool
    // recall that you cant inherit a static class but thats not what we want anyway if you really think abou tit
    // what we want is just an internal static variable so we can access the pool from anywhere
    // 
    // a resource Pool which is used for recycling objects that are expensive to create at runtime
    // one such use will be for a RenderSurface resource pool, particularly useful for terrain chunks.
    // 
    //TODO: This needs some type of "CleanUp" or "Maintenance" or "Collection" event or method that must get called
    // periodically to ensure that resources expire because in the current implementation, expire only gets called
    // when an item is checked out.
    public abstract class ObjectPool<T> where T : ResourceBase
    {
        // http://stackoverflow.com/questions/2510975/c-sharp-object-pooling-pattern-implementation
        protected Dictionary<string, T> _unavailable = new Dictionary<string, T>();
        protected Dictionary<string, T> _available = new Dictionary<string, T>();

        protected int _expirationTime;


        protected ObjectPool(int expirationTime)
        {
            if (expirationTime <= 0) throw new ArgumentOutOfRangeException();
            _expirationTime = expirationTime;
        }

        protected abstract bool Validate(T obj, PoolContext context);
        protected abstract T Create();

        public void CheckIn(T obj)
        {
            lock (_unavailable)
            {
                _unavailable.Remove(obj.ID.ToString());
            }

            lock (_available)
            {
                _available.Add(obj.ID.ToString(), obj);
            }
        }

        public void CheckOut(T obj)
        {
            obj.CheckOutTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
            lock (_unavailable)
            {
                _unavailable.Add(obj.ID.ToString(), obj);
            }
        }

        //The checkOut() method first checks to see if there are any objects in the unlocked hashtable. 
        //If so, it cycles through them and looks for a valid one. Validation depends on two things. 
        //First, the object pool checks to see that the object's last-usage time does not exceed the 
        //expiration time specified by the subclass. Second, the object pool calls the abstract validate() 
        //method, which does any class-specific checking or reinitialization that is needed to re-use the 
        //object. If the object fails validation, it is freed and the loop continues to the next object in
        //the hashtable. When an object is found that passes validation, it is moved into the locked hashtable
        //and returned to the process that requested it. If the unlocked hashtable is empty, or none of its
        //objects pass validation, a new object is instantiated and returned.
        public T CheckOut(PoolContext context)
        {
            T resource;
            lock (_available)
            {
                if (_available.Count > 0)
                {
                    IDictionaryEnumerator en = _available.GetEnumerator();

                    while (en.MoveNext())
                    {
                        //if (IsExpired((T)en.Value))
                        //{
                        // remove it from available and add it to unavailable and return it
                        if (Validate((T) en.Value, context))
                        {
                            resource = (T) en.Value;
                            _available.Remove(resource.ID.ToString());
                            // update the checkout time
                            resource.CheckOutTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                            lock (_unavailable)
                            {
                                _unavailable.Add(resource.ID.ToString(), resource);
                            }
                            return resource;
                        }
                        //}
                        //else
                        //{
                        //    Expire((T)en.Value);
                        //    en = _available.GetEnumerator();
                        //}
                    }
                }

                // so we can handle creation ourselves.  Pool should 
                return null;

                //// still here so we couldn't find an available resource in the "free" list so create a new instance of the desired resource
                //resource = Create();
                //resource.CheckOutTime = DateTime.Now.TimeOfDay.TotalMilliseconds;
                //lock (_unavailable)
                //{
                //    _unavailable.Add(resource.CheckOutTime, resource);
                //}
                //return resource;
            }
        }

        private bool IsExpired(T obj)
        {
            return DateTime.Now.TimeOfDay.TotalMilliseconds - obj.CheckOutTime > _expirationTime;
        }

        protected void Expire(T obj)
        {
            lock (_available)
            {
                _available.Remove(obj.ID.ToString());
            }
        }

        protected int Count()
        {
            return _available.Count + _unavailable.Count;
        }

        protected int CountCheckedIn()
        {
            return _available.Count;
        }

        protected int CountCheckedOut()
        {
            return _unavailable.Count;
        }
    }
}