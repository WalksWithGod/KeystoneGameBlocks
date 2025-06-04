using System;

namespace KeyPlugins
{
    public interface IPlugin
    {
        IPluginHost Host { get;}
        EventHandler EditResourceRequest { get; set; }

        string Name { get; }
        string Description { get; }
        string Author { get; }
        string Version { get; }
#if DEBUG
        string Summary { get; }
#endif
        void WatchedNodeCustomPropertyChanged(string id, string typename);
        void WatchedNodePropertyChanged(string id, string typename);
        void WatchedNodeAdded(string parent, string child, string typename);
        void WatchedNodeRemoved(string parent, string child, string typename);
        void WatchedNodeMoved(string newParent, string oldParent, string child, string typename);

        void ProcessEventQueue();
        void SelectTarget (string sceneName, string id, string typeName);
        string TargetID { get; }
        string TargetTypename { get; }
        string[] SupportedTypes { get; }
        string Profile { get; }
        bool ContainsSupportedType(string typeName);

        System.Windows.Forms.UserControl MainInterface { get; }

        System.Windows.Forms.ContextMenuStrip GetContextMenu(string resourceID, string parentID, System.Drawing.Point location);
        void Initialize();
        void Dispose();
    }
}
