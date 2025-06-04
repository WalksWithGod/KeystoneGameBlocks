using System;
using System.Collections.Generic;

namespace KeyServerCommon
{
    public class EntityDefinition
    {
        public string TypeName;
        public long PrimaryKey ;
        public string PKeySequence; 
        public  List<TableDefinition> TableDefinitions;

        public EntityDefinition (string type_name )
        {
            TypeName = type_name;
            TableDefinitions = new  List< TableDefinition>();
        }
    }
}
