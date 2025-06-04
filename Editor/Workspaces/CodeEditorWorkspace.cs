using System;
using System.Collections.Generic;
using System.Diagnostics;
using Ionic.Zip;
using Keystone.Resource;
using System.Windows.Forms;

namespace KeyEdit.Workspaces
{
    class CodeEditorWorkspace : Keystone.Workspaces.IWorkspace 
    {
        KeyEdit.Scripting.ScriptEditorDocument mDocument;
        protected Keystone.Workspaces.IWorkspaceManager mWorkspaceManager;
        protected string mName;
        protected bool mIsActive;
        protected string mCaption;
        KeyCommon.IO.ResourceDescriptor mDescriptor;

        public CodeEditorWorkspace (string documentPath) 
        {
            mDescriptor = new KeyCommon.IO.ResourceDescriptor(documentPath);
            mName = mDescriptor.ToString();
        }

        public CodeEditorWorkspace(KeyCommon.IO.ResourceDescriptor descriptor)
        {
            mDescriptor = descriptor;
            mName = mDescriptor.ToString();
        }


        public virtual void Configure(Keystone.Workspaces.IWorkspaceManager manager, Keystone.Scene.Scene scene)
        {
            if (manager == null) throw new ArgumentNullException("WorkspaceBase.Configre() - No ViewManager set.");
            mWorkspaceManager = manager;
            EditCode(mDescriptor);
        }

        public virtual void UnConfigure()
        {
            if (mWorkspaceManager == null) throw new Exception("WorkspaceBase.Unconfigure() - No ViewManager set.");
        }

        #region IWorkspace Members
        public string Name
        {
            get { return mName; }
        }

        public bool IsActive {get {return mIsActive;}}
        
        public Keystone.Workspaces.IWorkspaceManager Manager { get { return mWorkspaceManager; } }

        public Keystone.Controllers.InputControllerBase IOController { get { return null; } set { } }

        public Control Document { get { return mDocument; } }
        
        public Keystone.Collision.PickResults SelectedEntity
        {
            set { throw new Exception(); }
            get { throw new Exception(); }
        }

        public Keystone.Collision.PickResults MouseOverItem
        {
            set { throw new Exception(); }
            get { throw new Exception(); }
        }
        
        public void OnEntityAdded(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            throw new NotImplementedException();
        }

        public void OnEntityRemoved(Keystone.Entities.Entity parent, Keystone.Entities.Entity child)
        {
            throw new NotImplementedException();
        }

        public void OnEntitySelected(Keystone.Collision.PickResults pickResults)
        {
            //throw new NotImplementedException();
        }

        public void OnEntityDeleteKeyPress ()
        {
        	//throw new NotImplementedException();
        }
        
        public void Resize() { }
        public void Show() { mIsActive = true; }
        public void Hide() { mIsActive = false; }
        #endregion

        public void OnNodeAdded(string parentID, string childID, string childTypeName)
        {
        }
        public void OnNodeRemoved(string parentID, string childID, string childTypeName)
        {
        }

        private void CodeEditor_OnChange(object sender, EventArgs e)
        {
        }


        private void EditCode(KeyCommon.IO.ResourceDescriptor descriptor)
        {
            // TODO: why not allow multiple scrpts to be loaded? so
            // mCodeEditDocument should be an array
            //  TODO:  is there already a script loaded into the editor?  if that script hasnt been saved
            // after any edits, we must prompt first. 

            if (mDocument == null )
            {
                mDocument = new KeyEdit.Scripting.ScriptEditorDocument(descriptor.ToString(), CodEditor_OnSave, CodeEditor_OnChange);

                // CreateWorkspaceDocumentTab also tells the dockbar to make this tab the active tab
                //string name = string.Format("Code Editor[{0}]", count);

                mWorkspaceManager.CreateWorkspaceDocumentTab(mDocument, GotFocus);
            }
            
            mDocument.LoadScript(descriptor);
            Debug.WriteLine("CodeEditorWorkspace.EditCode() - " + descriptor.ToString());
        }

        private void CodEditor_OnSave(object sender, EventArgs e)
        {
            // TODO: need extensive error testing here especially when deleting children from parents
            // and then trying to restore... very sloppy and currently i think there's cases where it
            // it may remove children but then not restore and then leave the script node stranded?  
            // not sure, must test.

            // this script needs to be stored in the relevant scripts or shader path of the mod 
            // note: scripts and shaders and entities can be in different mods and still useable together.
            //         That's the only way to properly share common functionality.  It is the users responsibility
            //         to ensure they have all the necessary mods downloaded and located in the correct paths.

            // by definition here, the script already exists in the archive so we zip.UpdateEntry()
            // (no need to first delete then save)
            // And since we have the contents right in our editor, it's very easy to update since Update only
            // expects the contents, not a filepath.

            // then, we find if any ScriptNode in Repository exists with this entry name and we 
            // hrm...  how do we tell it (and mesh3d and others that change resource or have the underlying resource updated 
            // in the archive even if it's the same path in the arcive) to reload?  Well for starters, even our mesh3d for instance
            // right now have a very simplistic "load" where when the node is added to the scene it gets queued for loading.
            // So the only real way to do this is to unload the node and then add a new one.  There's really nothing wrong with
            // doing that but... with CSScriptLibrary i need to find out what happens to scripts that are modified and recompiled..
            // I assume you can tell the library itself to remove the script 

            // ok, the instance of the script class we can always release no problem.  As for the actual assembly, that i believe
            // will stay in memory and nothing we can do except to unload all of CSSCript somehow.  But to verify this
            // we can just compare the csscript loadedassembly cache before and after

            // Wait, we don't want to replace the node because we want to maintain the connection to each parent.  If we removed 
            // the node, we'd have to remove from every parent then re-add to every parent the new one.
            //      as well as re-assign any children (though there shouldn't ever be children on a resource node which is like a leaf right?)


            // But maybe that is the elegant way to do it.  Remove then Add the new one keeping a list of the parents and children from
            // before (although that sucks then i still have to use the ChildSetter... but well, guess we do that anyway).

            Scripting.ScriptDocument script = mDocument.Script;
            KeyCommon.IO.ResourceDescriptor descriptor = new KeyCommon.IO.ResourceDescriptor(script.Filename);

            if (descriptor.IsArchivedResource)
            {
            	string fullZipPath = System.IO.Path.Combine(AppMain.MOD_PATH, descriptor.ModName);
            	ZipFile zip = new ZipFile(fullZipPath);
            	zip.UpdateEntry(descriptor.EntryName, script.Code);
            	zip.Save();
            }
            else
            {
            	string path = System.IO.Path.Combine(AppMain.MOD_PATH, script.Filename);
            	System.IO.File.WriteAllText (path, script.Code);
            }
            
            // update the node if it exists in the scene
            Keystone.Elements.Node node = (Keystone.Elements.Node)Repository.Get(descriptor.ToString());
			if (node == null) 
				return;  // the node is not necessarily actually loaded in the scene.  We may have just edited it only from gallery

			
            //Keystone.Elements.DomainObjectScript
            //Keystone.Behavior.Actions.Script node = (Keystone.Behavior.Actions.Script)Repository.Get(descriptor.ToString());
            
            Keystone.Elements.IGroup[] parents = node.Parents;
            //obsolete with ScriptNode2 ->        string eventName = node.EventName; // "OnUpdate";
            // remove this script node from every parent, add a new script node with the updated resource
            if (parents != null)
                for (int i = 0; i < parents.Length; i++)
                    parents[i].RemoveChild(node);

            if (node is Keystone.DomainObjects.DomainObject)
                node = Keystone.Resource.Repository.Create(descriptor.ToString(), "DomainObject");
            else
                node = Keystone.Resource.Repository.Create(descriptor.ToString(), "Script");
            //obsolete with ScriptNode2 ->       node.EventName = eventName;
            Debug.WriteLine("CSSCriptLibrary Loaded Cache Count BEFORE = " + CSScriptLibrary.CSScript.ScriptCache.Length);
            CSScriptLibrary.CSScript.LoadedScript[] scriptsBefore = CSScriptLibrary.CSScript.ScriptCache;

            Debug.WriteLine("CSSCriptLibrary Loaded Cache Count AFTER = " + CSScriptLibrary.CSScript.ScriptCache.Length);
            CSScriptLibrary.CSScript.LoadedScript[] scriptsAfter = CSScriptLibrary.CSScript.ScriptCache;

            Keystone.Traversers.SuperSetter setter;
            if (parents != null)
                for (int i = 0; i < parents.Length; i++)
                {
                    setter = new Keystone.Traversers.SuperSetter((Keystone.Elements.Node)parents[i]);
                    setter.Apply(node);
                }
        }


        private void GotFocus(Object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("EditorWorkspace.GotFocus() - ");
        }
    }
}
