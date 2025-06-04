using System;

namespace KeyPlugins
{
    public class DragDropContext
    {
        public string Source;
        public string EntityID;
        public string ParentID;
        public string TypeName;
        public string ModName; 			//relative zip file path (eg. common.zip) or folder name (eg. caesar)
        public string ResourcePath;     //relative path within the archive or folder

        public object Data;
    }
}
