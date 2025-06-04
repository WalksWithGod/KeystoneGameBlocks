using System;
using System.Collections.Generic;
using System.Diagnostics;

using Keystone.Elements;
using Keystone.Entities;
using Keystone.IO;
using Keystone.Types;

namespace Keystone.FX
{
    // This class is no longer an FXBase derived class.  This is now treated
    // more correctly as an alternate renderer (such as ImposterRenderer todo:)
    // which can now be called anywhere within the pipeline.
    public class InstanceRenderer : IDisposable 
    {
        private Dictionary<string, Minimesh2> _minimeshes;

        public InstanceRenderer()
        {
        }

        /// <summary>
        /// Called by ModeledEntity.UseInstancing property if value = true
        /// note: but this may be problematic if when loading the properties of saved Entity
        /// this gets called via .UseInstancing property xml read as true and yet
        /// the underlying mesh not created yet...  
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="appearance"></param>
        /// <param name="maxElements"></param>
        public void CreateMinimesh(Mesh3d mesh, Appearance.Appearance appearanceToClone, uint maxInstances)
        {
        	// TODO: this method should only occur during LoadTVResources() of the Mesh3d that is UseInstancing
        	//       or, as a delayed IPageable operation when the mesh is changed to UseInstancing after the mesh has already been loaded
            if (_minimeshes == null) _minimeshes = new Dictionary<string, Minimesh2>();
            if (mesh == null) throw new ArgumentNullException("Mesh is null.");
            
            // TODO: i may allow appearance to be null and instead track appearances only on
            // AddInstance within the minimesh and then track every single appearance and sort them
            // by appearance and depth so i can render them seperately using seperate arrays.
            // Of course we will only have to enforce one thing, LightingMode = tangent space for certain.
            // That is to say, we'll enforce that once a mini has been created from a mesh based on a specific
            // lighting mode, it cant be changed at runtime.

            //if (appearance == null) throw new ArgumentNullException("Instance render is not properly initialized.  Appearance == null!");

            // if the minimesh is already added, skip it
            if (_minimeshes.ContainsKey(mesh.ID))
            {
                mesh.Minimesh = _minimeshes[mesh.ID];
                return;
            }
            
            // note: appearance cannot be shared... what we need is to clone
            // the Appearance and the ProceduralShader so that a new minimesh specific
            // shader can be compiled.
            // TODO: however, should i maintain a reference to the original appearance so that
            // if changes are made to it, they can be reflected here? (what we really need is a ref to a master definition/prefab)
            // Well that's problematic
            // because appearances can't be shared and that means every unique Entity instance
            // has a different one, and our rule is that instanced geometry to be batched together
            // must have an appearance with same HashCode.  So that brings us back to the notion of
            // tracking all Appearances during the Minimesh.AddInstance() so we can sort by hashcode
            // and batch appropriately.  In that case 
            Appearance.Appearance clonedAppearance = null; // appearance can be null. Deferred Pointlights use null appearance
            if (appearanceToClone != null)
            {
                string clonedAppearanceID = Resource.Repository.GetNewName(typeof(Appearance.DefaultAppearance));
                
                // this app since it it's never added to a parent Node but just stays in the
                // Minimesh.cs, Minimesh2 will increment it's refcount and then decrement it
                // when it's no longer needed.  This way all of the clonedAppearance children
                // will get automatically dereferenced as well.
                clonedAppearance = (Appearance.Appearance)appearanceToClone.Clone(clonedAppearanceID, true, false);
            }

            Minimesh2 mini = new Minimesh2(mesh, clonedAppearance, maxInstances);
            mesh.Minimesh = mini;
            _minimeshes.Add(mesh.ID, mini);
            
            IPageableTVNode pageable = (IPageableTVNode)mini;
            if (pageable.PageStatus != PageableNodeStatus.Loading)
                IO.PagerBase.QueuePageableResourceLoad(pageable, null);
                    
        }


		
        // TODO: dont believe i ever track the removal of Minimesh and attempt to
        // then remove it from this InstanceRenderer
        public void RemoveMinimesh(string id)
        {
            if (_minimeshes == null) return;
            try
            {
                _minimeshes.Remove(id);
            }
            catch
            {}
        }

        public void Update()
        {
            if (_minimeshes == null) return;
            foreach (Minimesh2 minimesh in _minimeshes.Values)
            {
                minimesh.Clear();
            }
        }

        public void Render()
        {
            // TODO: this seems to not be thread safe when loading a minimesh 
            // in one of our FormCommands worker threads i suspect...
            if (_minimeshes == null) return;
            foreach (Minimesh2 mini in _minimeshes.Values)
            {
                if (mini.TVResourceIsLoaded)
                    mini.Render();
            }
        }

        #region IDisposable Members
        private bool _disposed;
        ~InstanceRenderer()
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


            // Note: Minimesh is not a Node and thus no Repository.Increment/Decrement ref applies
            try
            {
                foreach (Minimesh2 mini in _minimeshes.Values)
                    if (mini != null)
                       mini.Dispose ();

                _minimeshes.Clear ();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("InstanceRenderer.DisposeUnmanagedResources() - " + ex.Message);
            }
        }

        protected virtual void DisposeUnmanagedResources()
        {

        }

        protected void CheckDisposed()
        {
            if (IsDisposed) throw new ObjectDisposedException(this.GetType().Name + " is disposed.");
        }

        protected bool IsDisposed
        {
            get { return _disposed; }
        }
        #endregion
    }
}
