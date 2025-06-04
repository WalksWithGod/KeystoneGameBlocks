using System;
using System.ComponentModel;
using Keystone.Types;


namespace Keystone.TypeConverters
{
    public class BoundingBoxConverter : ExpandableObjectConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(BoundingBox)) return true;
            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            if (value == null)
                return new BoundingBox();

            if (value is string)
            {
                try
                {
                    string s = (string)value;
                    BoundingBox box = BoundingBox.Parse(s);
                    return box;
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Can not convert '" + value.ToString() + "' to type Vector3d.");
                }
            }
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
        {
            // is correct type?
            if (value != null)
                if (!(value is BoundingBox))
                    throw new Exception("BoundingBoxConverter.ConverTo() - value is wrong type.");

            if (destinationType == typeof(string))
            {
                if (value == null)
                    return String.Empty;

                BoundingBox box = (BoundingBox)value;
                return box.ToString();
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
