/********************************************************************
 *
 *  PropertyBag.cs
 *  --------------
 *  Copyright (C) 2002  Tony Allowatt
 *  Last Update: 12/14/2002
 * 
 *  THE SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS", WITHOUT WARRANTY
 *  OF ANY KIND, EXPRESS OR IMPLIED. IN NO EVENT SHALL THE AUTHOR BE
 *  LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OF THIS
 *  SOFTWARE.
 * 
 *  Public types defined in this file:
 *  ----------------------------------
 *  namespace Flobbster.Windows.Forms
 *     class PropertySpec
 *     class PropertySpecEventArgs
 *     delegate PropertySpecEventHandler
 *     class PropertyBag
 *        class PropertyBag.PropertySpecCollection
 *     class PropertyTable
 *
 ********************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Collections.Generic; //used by UITypeEditor

namespace Settings
{
	/// <summary>
	/// Represents a collection of custom properties that can be selected into a
	/// PropertyGrid to provide functionality beyond that of the simple reflection
	/// normally used to query an object's properties.
	/// </summary>
	public class PropertyBag : ICustomTypeDescriptor
	{

        protected string defaultProperty;
        protected PropertySpecCollection mSpecs; // TODO: this whole thing can likely be replaced by List<PropertySpec>


		#region PropertySpecCollection class definition
		/// <summary>
		/// Encapsulates a collection of PropertySpec objects.
		/// </summary>
		[Serializable]
		public class PropertySpecCollection : IList
		{
			private List<PropertySpec> _innerArray;
			
			/// <summary>
			/// Initializes a new instance of the PropertySpecCollection class.
			/// </summary>
			public PropertySpecCollection()
			{
				_innerArray = new List<PropertySpec>();
			}

			/// <summary>
			/// Gets the number of elements in the PropertySpecCollection.
			/// </summary>
			/// <value>
			/// The number of elements contained in the PropertySpecCollection.
			/// </value>
			public int Count
			{
				get { return _innerArray.Count; }
			}

			/// <summary>
			/// Gets a value indicating whether the PropertySpecCollection has a fixed size.
			/// </summary>
			/// <value>
			/// true if the PropertySpecCollection has a fixed size; otherwise, false.
			/// </value>
			public bool IsFixedSize
			{
				get { return false; }
			}
			
			/// <summary>
			/// Gets a value indicating whether the PropertySpecCollection is read-only.
			/// </summary>
			public bool IsReadOnly
			{
				get { return false; }
			}

			/// <summary>
			/// Gets a value indicating whether access to the collection is synchronized (thread-safe).
			/// </summary>
			/// <value>
			/// true if access to the PropertySpecCollection is synchronized (thread-safe); otherwise, false.
			/// </value>
			public bool IsSynchronized
			{
				get { return false; }
			}

			/// <summary>
			/// Gets an object that can be used to synchronize access to the collection.
			/// </summary>
			/// <value>
			/// An object that can be used to synchronize access to the collection.
			/// </value>
			object ICollection.SyncRoot
			{
				get { return null; }
			}

			/// <summary>
			/// Gets or sets the element at the specified index.
			/// In C#, this property is the indexer for the PropertySpecCollection class.
			/// </summary>
			/// <param name="index">The zero-based index of the element to get or set.</param>
			/// <value>
			/// The element at the specified index.
			/// </value>
			public PropertySpec this[int index]
			{
				get { return _innerArray[index]; }
				set { _innerArray[index] = value; }
			}

            ///// <summary>
            ///// Gets or sets the element at the specified index.
            ///// In C#, this property is the indexer for the PropertySpecCollection class.
            ///// </summary>
            ///// <param name="key">The hash key of the element to get or set.</param>
            ///// <value>
            ///// The element at the specified index.
            ///// </value>
            //public PropertySpec this[string key]
            //{
            //    get { return _innerArray[key]; }
            //    set 
            //    { 
            //        int index = IndexOf (key)
            //        _innerArray[key] = value; 
            //    }
            //}

			/// <summary>
			/// Adds a PropertySpec to the end of the PropertySpecCollection.
			/// </summary>
			/// <param name="value">The PropertySpec to be added to the end of the PropertySpecCollection.</param>
			/// <returns>The PropertySpecCollection index at which the value has been added.</returns>
			public int Add(PropertySpec value)
			{
				_innerArray.Add(value);
                return _innerArray.Count - 1;
			}

			/// <summary>
			/// Adds the elements of an array of PropertySpec objects to the end of the PropertySpecCollection.
			/// </summary>
			/// <param name="array">The PropertySpec array whose elements should be added to the end of the
			/// PropertySpecCollection.</param>
			public void AddRange(PropertySpec[] array)
			{
				_innerArray.AddRange(array);
			}

			/// <summary>
			/// Removes all elements from the PropertySpecCollection.
			/// </summary>
			public void Clear()
			{
				_innerArray.Clear();
			}

			/// <summary>
			/// Determines whether a PropertySpec is in the PropertySpecCollection.
			/// </summary>
			/// <param name="item">The PropertySpec to locate in the PropertySpecCollection. The element to locate
			/// can be a null reference (Nothing in Visual Basic).</param>
			/// <returns>true if item is found in the PropertySpecCollection; otherwise, false.</returns>
			public bool Contains(PropertySpec item)
			{
				return _innerArray.Contains(item);
			}

			/// <summary>
			/// Determines whether a PropertySpec with the specified name is in the PropertySpecCollection.
			/// </summary>
			/// <param name="name">The name of the PropertySpec to locate in the PropertySpecCollection.</param>
			/// <returns>true if item is found in the PropertySpecCollection; otherwise, false.</returns>
			public bool Contains(string name)
			{
				foreach(PropertySpec spec in _innerArray)
					if(spec.Name == name)
						return true;

				return false;
			}

			/// <summary>
			/// Copies the entire PropertySpecCollection to a compatible one-dimensional Array, starting at the
			/// beginning of the target array.
			/// </summary>
			/// <param name="array">The one-dimensional Array that is the destination of the elements copied
			/// from PropertySpecCollection. The Array must have zero-based indexing.</param>
			public void CopyTo(PropertySpec[] array)
			{
				_innerArray.CopyTo(array);
			}

			/// <summary>
			/// Copies the PropertySpecCollection or a portion of it to a one-dimensional array.
			/// </summary>
			/// <param name="array">The one-dimensional Array that is the destination of the elements copied
			/// from the collection.</param>
			/// <param name="index">The zero-based index in array at which copying begins.</param>
			public void CopyTo(PropertySpec[] array, int index)
			{
				_innerArray.CopyTo(array, index);
			}

			/// <summary>
			/// Returns an enumerator that can iterate through the PropertySpecCollection.
			/// </summary>
			/// <returns>An IEnumerator for the entire PropertySpecCollection.</returns>
			public IEnumerator GetEnumerator()
			{
				return _innerArray.GetEnumerator();
			}

			/// <summary>
			/// Searches for the specified PropertySpec and returns the zero-based index of the first
			/// occurrence within the entire PropertySpecCollection.
			/// </summary>
			/// <param name="value">The PropertySpec to locate in the PropertySpecCollection.</param>
			/// <returns>The zero-based index of the first occurrence of value within the entire PropertySpecCollection,
			/// if found; otherwise, -1.</returns>
			public int IndexOf(PropertySpec value)
			{
				return _innerArray.IndexOf(value);
			}

			/// <summary>
			/// Searches for the PropertySpec with the specified name and returns the zero-based index of
			/// the first occurrence within the entire PropertySpecCollection.
			/// </summary>
			/// <param name="name">The name of the PropertySpec to locate in the PropertySpecCollection.</param>
			/// <returns>The zero-based index of the first occurrence of value within the entire PropertySpecCollection,
			/// if found; otherwise, -1.</returns>
			public int IndexOf(string name)
			{
				int i = 0;

				foreach(PropertySpec spec in _innerArray)
				{
					if(spec.Name == name)
						return i;

					i++;
				}

				return -1;
			}

			/// <summary>
			/// Inserts a PropertySpec object into the PropertySpecCollection at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which value should be inserted.</param>
			/// <param name="value">The PropertySpec to insert.</param>
			public void Insert(int index, PropertySpec value)
			{
				_innerArray.Insert(index, value);
			}

			/// <summary>
			/// Removes the first occurrence of a specific object from the PropertySpecCollection.
			/// </summary>
			/// <param name="obj">The PropertySpec to remove from the PropertySpecCollection.</param>
			public void Remove(PropertySpec obj)
			{
				_innerArray.Remove(obj);
			}

			/// <summary>
			/// Removes the property with the specified name from the PropertySpecCollection.
			/// </summary>
			/// <param name="name">The name of the PropertySpec to remove from the PropertySpecCollection.</param>
			public void Remove(string name)
			{
				int index = IndexOf(name);
				RemoveAt(index);
			}

			/// <summary>
			/// Removes the object at the specified index of the PropertySpecCollection.
			/// </summary>
			/// <param name="index">The zero-based index of the element to remove.</param>
			public void RemoveAt(int index)
			{
				_innerArray.RemoveAt(index);
			}

			/// <summary>
			/// Copies the elements of the PropertySpecCollection to a new PropertySpec array.
			/// </summary>
			/// <returns>A PropertySpec array containing copies of the elements of the PropertySpecCollection.</returns>
			public PropertySpec[] ToArray()
			{
				return _innerArray.ToArray();
			}

			#region Explicit interface implementations for ICollection and IList
			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			void ICollection.CopyTo(Array array, int index)
			{
				CopyTo((PropertySpec[])array, index);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			int IList.Add(object value)
			{
				return Add((PropertySpec)value);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			bool IList.Contains(object obj)
			{
				return Contains((PropertySpec)obj);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			object IList.this[int index]
			{
				get
				{
					return ((PropertySpecCollection)this)[index];
				}
				set
				{
					((PropertySpecCollection)this)[index] = (PropertySpec)value;
				}
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			int IList.IndexOf(object obj)
			{
				return IndexOf((PropertySpec)obj);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			void IList.Insert(int index, object value)
			{
				Insert(index, (PropertySpec)value);
			}

			/// <summary>
			/// This member supports the .NET Framework infrastructure and is not intended to be used directly from your code.
			/// </summary>
			void IList.Remove(object value)
			{
				Remove((PropertySpec)value);
			}
			#endregion
		}
		#endregion

		#region PropertySpecDescriptor class definition
		private class PropertySpecDescriptor : PropertyDescriptor
		{
			private PropertyBag bag;
			private PropertySpec item;

			public PropertySpecDescriptor(PropertySpec item, PropertyBag bag, string name, Attribute[] attrs) :
				base(name, attrs)
			{
				this.bag = bag;
				this.item = item;
			}

			public override Type ComponentType
			{
				get { return item.GetType(); }
			}

			public override bool IsReadOnly
			{
				get { return (Attributes.Matches(ReadOnlyAttribute.Yes)); }
			}

            public override Type PropertyType
            {

                get
                {
                    try
                    {
                        return Type.GetType(item.TypeName);
                    }

                    catch
                    {
                        Trace.WriteLine("Type '" + item.TypeName + "' unsupported." );
                        throw new ArgumentNullException("Type '" + item.TypeName + "' unsupported.");
                    }
                }
            }

			public override bool CanResetValue(object component)
			{
				if(item.DefaultValue == null)
					return false;
				else
					return !this.GetValue(component).Equals(item.DefaultValue);
			}

			public override object GetValue(object component)
			{
				// Have the property bag raise an event to get the current value
				// of the property.

				PropertySpecEventArgs e = new PropertySpecEventArgs(item, null);
				bag.OnGetValue(e);
				return e.Value;
			}

			public override void ResetValue(object component)
			{
				SetValue(component, item.DefaultValue);
			}

			public override void SetValue(object component, object value)
			{
				// Have the property bag raise an event to set the current value
				// of the property.

				PropertySpecEventArgs e = new PropertySpecEventArgs(item, value);
				bag.OnSetValue(e);
			}

			public override bool ShouldSerializeValue(object component)
			{
				object val = this.GetValue(component);

				if(item.DefaultValue == null || val == null)
					return false;
				else
					return !val.Equals(item.DefaultValue);
			}
		}
		#endregion


		/// <summary>
		/// Initializes a new instance of the PropertyBag class.
		/// </summary>
		public PropertyBag()
		{
			defaultProperty = null;
			mSpecs = new PropertySpecCollection();
		}

		/// <summary>
		/// Gets or sets the name of the default property in the collection.
		/// </summary>
		public string DefaultProperty
		{
			get { return defaultProperty; }
			set { defaultProperty = value; }
		}

		/// <summary>
		/// Gets the collection of properties contained within this PropertyBag.
		/// </summary>
		public PropertySpecCollection Properties
		{
			get { return mSpecs; }
		}

		/// <summary>
		/// Occurs when a PropertyGrid requests the value of a property.
		/// </summary>
		public event PropertySpecEventHandler GetValue;

		/// <summary>
		/// Occurs when the user changes the value of a property in a PropertyGrid.
		/// </summary>
		public event PropertySpecEventHandler SetValue;

		/// <summary>
		/// Raises the GetValue event.
		/// </summary>
		/// <param name="e">A PropertySpecEventArgs that contains the event data.</param>
		protected virtual void OnGetValue(PropertySpecEventArgs e)
		{
			if(GetValue != null)
				GetValue(this, e);
		}

		/// <summary>
		/// Raises the SetValue event.
		/// </summary>
		/// <param name="e">A PropertySpecEventArgs that contains the event data.</param>
		protected virtual void OnSetValue(PropertySpecEventArgs e)
		{
			if(SetValue != null)
				SetValue(this, e);
		}

		#region ICustomTypeDescriptor explicit interface definitions
		// Most of the functions required by the ICustomTypeDescriptor are
		// merely pssed on to the default TypeDescriptor for this type,
		// which will do something appropriate.  The exceptions are noted
		// below.
		AttributeCollection ICustomTypeDescriptor.GetAttributes()
		{
			return TypeDescriptor.GetAttributes(this, true);
		}

		string ICustomTypeDescriptor.GetClassName()
		{
			return TypeDescriptor.GetClassName(this, true);
		}

		string ICustomTypeDescriptor.GetComponentName()
		{
			return TypeDescriptor.GetComponentName(this, true);
		}

		TypeConverter ICustomTypeDescriptor.GetConverter()
		{
			return TypeDescriptor.GetConverter(this, true);
		}

		EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
		{
			return TypeDescriptor.GetDefaultEvent(this, true);
		}

		PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
		{
			// This function searches the property list for the property
			// with the same name as the DefaultProperty specified, and
			// returns a property descriptor for it.  If no property is
			// found that matches DefaultProperty, a null reference is
			// returned instead.

			PropertySpec propertySpec = null;
			if(defaultProperty != null)
			{
				int index = mSpecs.IndexOf(defaultProperty);
				propertySpec = mSpecs[index];
			}

			if(propertySpec != null)
				return new PropertySpecDescriptor(propertySpec, this, propertySpec.DisplayName , null);
			else
				return null;
		}

		object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
		{
			return TypeDescriptor.GetEditor(this, editorBaseType, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
		{
			return TypeDescriptor.GetEvents(this, true);
		}

		EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
		{
			return TypeDescriptor.GetEvents(this, attributes, true);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
		{
			return ((ICustomTypeDescriptor)this).GetProperties(new Attribute[0]);
		}

		PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
		{
			// Rather than passing this function on to the default TypeDescriptor,
			// which would return the actual properties of PropertyBag, I construct
			// a list here that contains property descriptors for the elements of the
			// Properties list in the bag.

			ArrayList props = new ArrayList();

			foreach(PropertySpec property in mSpecs)
			{
				ArrayList attrs = new ArrayList();

				// If a category, description, editor, or type converter are specified
				// in the PropertySpec, create attributes to define that relationship.
				if(property.Category != null)
					attrs.Add(new CategoryAttribute(property.Category));

				if(property.Description != null)
					attrs.Add(new DescriptionAttribute(property.Description));

				if(property.EditorTypeName != null)
					attrs.Add(new EditorAttribute(property.EditorTypeName, typeof(UITypeEditor)));

				if(property.ConverterTypeName != null)
					attrs.Add(new TypeConverterAttribute(property.ConverterTypeName));

				// dec.24.2013 - Hypno - using System.Attributes in this way is unnecessary
				// Additionally, append the custom attributes associated with the
				// PropertySpec, if any.
				//if(property.Attributes != null)
				//	attrs.AddRange(property.Attributes);

				Attribute[] attrArray = (Attribute[])attrs.ToArray(typeof(Attribute));

				// Create a new property descriptor for the property item, and add
				// it to the list.
				PropertySpecDescriptor pd = new PropertySpecDescriptor(property,
					this, property.DisplayName, attrArray);
				props.Add(pd);
			}

			// Convert the list of PropertyDescriptors to a collection that the
			// ICustomTypeDescriptor can use, and return it.
			PropertyDescriptor[] propArray = (PropertyDescriptor[])props.ToArray(
				typeof(PropertyDescriptor));
			return new PropertyDescriptorCollection(propArray);
		}

		object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
		{
			return this;
		}
		#endregion
	}
}
