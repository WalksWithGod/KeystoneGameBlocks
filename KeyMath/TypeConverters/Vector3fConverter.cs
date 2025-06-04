using System;
using System.ComponentModel;
using Keystone.Types;
using Settings;

namespace Keystone.TypeConverters
{
    // http://www.propertygridresourcelist.com/
    // http://www.codeproject.com/Articles/26992/Using-a-TypeDescriptionProvider-to-support-dynamic
    // http://www.codeproject.com/Articles/17092/Creating-Property-Editors-in-DesignTime-for-VS-Net
    public class Vector3fConverter : ExpandableObjectConverter
    {
        public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
        {
            return new Vector3f((float)propertyValues["x"], (float)propertyValues["y"], (float)propertyValues["z"]);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(value, attributes);

            PropertyBag bag = (PropertyBag)context.Instance;

            // we want modification of any of these fields to result in 
            // calls to convertto/from.  Thus we should
            // set the converter to use to be this, and NOT 




            //foreach (PropertyDescriptor desc in properties)
            //{
            //    // we need to know which PropertySpec this belongs to...  
            //   desc.Converter = this;

            //    if (desc.Name == "x")
            //    {
            //        desc.AddValueChanged(bag.);
            //    }


            //}

            string[] sortOrder = new string[4];

            sortOrder[0] = "x";
            sortOrder[1] = "y";
            sortOrder[2] = "z";


            // Return a sorted list of properties
            return properties.Sort(sortOrder);
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }


        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Vector3f)) return true;
            // TODO: i should test here the parse and then return false if it fails
            // and don't do it in ConvertFrom
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null)
                return new Vector3f();

            if (value is string)
            {
                try
                {
                    string s = (string)value;
                    Vector3f vec = Vector3f.Parse(s);
                    return vec;

                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.WriteLine("Can not convert '" + value.ToString() + "' to type Vector3d.");
                    return null;
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            // is correct type?
            if (value != null)
                if (!(value is Vector3f))
                {
                    System.Diagnostics.Trace.WriteLine("Vector3dConverter.ConverTo() - value is wrong type.");
                    return null;
                }

            if (destinationType == typeof(string))
            {
                if (value == null)
                    return String.Empty;

                Vector3f vec = (Vector3f)value;
                return vec.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}
