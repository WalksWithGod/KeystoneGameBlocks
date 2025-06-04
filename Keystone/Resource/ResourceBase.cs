using System;
using System.Xml;
using Keystone.IO;

namespace Keystone.Resource
{
    public abstract class ResourceBase : IResource // TODO: is there any object that uses ResourceBase but does not inherit from node?
    {                                              // seems RenderSurfaces, ObjectPool, and FXLasers
        internal double CheckOutTime;
        protected bool _disposed = false;
        private int _refCount;
        protected string _id;  // TODO: id is good for filename resources, but bad for other things cuz its too long and ugly
                               // TODO: there is no "ref" and "def" anywhere...
                               // nor are links being managed between "def" nodes and 
                               // live instances.
        private string _ref; // ref to a node in another document table within a prefab (eg. Models.xml, Entities.xml, Zones.xml) 
        private string _src; // only NON "shareable" nodes can be "src'd."
                               // and this means that even though they require unique instances they should still
                               // maintain a reference link.
                               // nodes that are "src'd" should remain in MOD db?
                               // however since there are sub-nodes like Entities, Models, Appearances, Layers
                               // that are not accessible withotu first loading the root Entity in the prefab, we should
                               // keep all prefabs we load stored in our Repository and have the Root entity use it's
                               // saved prefab resourcedescriptor as ID!
                               // Otherwise share-able nodes even ones with random id's i think will automatically be shared
                               // during deserialization / cloning
        public ResourceBase(string id)
        {
            _id = id;
            Repository.Add( this);
        }

        #region ResourceBase
        /// <summary>
        /// Returns just the property of the specified name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="specOnly"></param>
        /// <returns></returns>
        public Settings.PropertySpec GetProperty(string name, bool specOnly)
        {
            Settings.PropertySpec[] properties = GetProperties(specOnly);
            if (properties == null) return null;
            for (int i = 0; i < properties.Length; i++)
                if (properties[i].Name == name) return properties[i];

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="specOnly">True returns the properties without any values assigned</param>
        /// <returns></returns>
        public virtual Settings.PropertySpec[] GetProperties(bool specOnly)
        {
            // the last property in the base most inherit typed (ResourceBase) must be "id"
            // so currently ref is at subscript 0 and id at subscript 1.  This will put "id" as the
            // last most property in every node's GetProperties() results array.
            Settings.PropertySpec[] properties;
            
            properties = new Settings.PropertySpec[3];
            properties[0] = new Settings.PropertySpec("ref", typeof(string).Name);
            properties[1] = new Settings.PropertySpec("src", typeof(string).Name);
            properties[2] = new Settings.PropertySpec("id", typeof(string).Name);

            if (!specOnly)
            {
                properties[0].DefaultValue = _ref;
                properties[1].DefaultValue = _src;
                properties[2].DefaultValue = _id;
            }
            

            return properties;
        }

        public virtual void SetProperties(Settings.PropertySpec[] properties)
        {
            if (properties == null) return;
            for (int i = 0; i < properties.Length; i++)
            {
                // use of a switch allows us to pass in all or a few of the propspecs depending
                // on whether we're loading from xml or changing a single property via server directive
                switch (properties[i].Name)
                {
                    case "id":
                        _id = (string)properties[i].DefaultValue;
                        break;
                    case "ref":
                        _ref = (string)properties[i].DefaultValue;
                        break;
                    case "src":
                        _src = (string)properties[i].DefaultValue;
                        break;
                }
            }
        }
        
        public void SetProperty (string name, Type t, object value)
        {
        	Settings.PropertySpec property = new Settings.PropertySpec (name, t.Name);
        	property.DefaultValue = value;
        	SetProperties (new Settings.PropertySpec[] {property});
        }

        public void SetProperty(string name, string typeName, object value)
        {
            Settings.PropertySpec property = new Settings.PropertySpec(name, typeName);
            property.DefaultValue = value;
            SetProperties(new Settings.PropertySpec[] { property });
        }

        #endregion

        public int RefCount
        {
            get { return _refCount; }
            set { _refCount = value; }
        }

        public string ID
        {
            get { return _id; }
        }

        public string SRC
        {
            get { return _src; }
            set { _src = value; }
        }

        public virtual string TypeName
        {
            get 
            {
                return GetType().Name; 
            }
        }

        #region IDisposable Members
        ~ResourceBase()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) here is optimal in terms of
            // readability and maintainability.
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // pass true if managed resources should be disposed. Otherwise false.
        private void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    DisposeManagedResources();
                    _disposed = true;
                }
                DisposeUnmanagedResources();
            }
        }

        protected virtual void DisposeManagedResources()
        {
            //note: when a node is disposed, it's reference count should be down to 0 at this point
            // because when the ref count reaches 0, there should be no more references to the node
            // unless they are held by the original caller and they shouldnt be.  
            //
            // IMPORTANT: Thus the Repository will call .Dispose() on the object and the 
            //  overriden DisposeManaged/UnmangedResoruces should call an appropriate tv....Destroy() on any TV3D resource
            // thus if there are any lingering references, they will be invalid so it is the Creator's responsibility to manually
            // increment the reference count of this object if they dont want it to be disposed.

            //But there's no way to enforce
            // that.  In normal file save/load this can't happen, but when the user manually imports/creates from a static load procedure
            // then there is no way to guarantee that so we leave those resources loaded.  There's no harm in that when we unload a scene
            // only when we want to shut down the engine completely.


            // Note: Remove() of the node is what will decremenet the reference count
         
        }

        protected virtual void DisposeUnmanagedResources()
        {
        }

        protected void CheckDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(TypeName + " is disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion
    }
}