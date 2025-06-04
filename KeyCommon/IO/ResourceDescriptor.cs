using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KeyCommon.IO
{
    public struct ResourceDescriptor
    {
        public readonly bool IsArchivedResource;
        public readonly string ModName; // can be either an archive filename (eg. common.zip) or a folder name (eg. caesar)

        public readonly string EntryName;
        public readonly string FileName;
        public readonly string Extension;
        //public string Type;  // primitive, file, but if it's a primitive then it needs more data such as number of slices and stacks for a sphere?

        public ResourceDescriptor(string modName, string entryName)
        {
        	
        	if (string.IsNullOrEmpty(modName))
        		IsArchivedResource = false;
        	else
	            IsArchivedResource = true;
    
        	ModName = modName;
            EntryName = entryName;

            Extension = System.IO.Path.GetExtension(EntryName).ToUpper();
            FileName = System.IO.Path.GetFileName(EntryName);
        }

        public ResourceDescriptor(string fromSourceString)
        {
            if (string.IsNullOrEmpty(fromSourceString)) throw new ArgumentNullException();

            string[] parts = fromSourceString.Split('|');

            if (parts == null || parts.Length == 0 || parts.Length > 2) throw new ArgumentOutOfRangeException();
            if (parts.Length == 1)
            {
                IsArchivedResource = false;
                ModName = null;
                EntryName = parts[0];
            }
            else
            {
                IsArchivedResource = true;
                ModName = parts[0];
                EntryName = parts[1];
            }

            FileName = System.IO.Path.GetFileName(EntryName);
            Extension = System.IO.Path.GetExtension(EntryName).ToUpper();
        }


        public override string ToString()
        {
            if (IsArchivedResource)
                return ModName + "|" + EntryName;
            else
                return EntryName;
        }
    }
}
