using System;


namespace KeyPlugins
{
    public class ResourceEventArgs : EventArgs
    {
        public string ParentID;
        public string ChildID;
        public object Value;  // contextual based on whether this will be handled by Scaled/Rotated or Translated events       
        public string ResourceType;
    }
}
