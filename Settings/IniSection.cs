using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Settings
{
    [XmlType(TypeName = "section"), XmlRoot(), Serializable()] // TypeName "section" is what will appear in the xml output
    public struct IniSection : IComparable<string>
    {
        [XmlAttributeAttribute(AttributeName="name", Form=XmlSchemaForm.Unqualified, DataType="string")] 
        public string name;

        [XmlElement(Type= typeof(KeyValuePair), ElementName="key", IsNullable=false, Form=XmlSchemaForm.Qualified)]
        public  List<KeyValuePair> keys;

        public IniSection (string sectionName)
        {
            name = sectionName;
            keys = new List<KeyValuePair>();
        }

        public int CompareTo(string obj)
        {
            if (obj == name) return -1;
            return 0;
        }
    }
}