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
using System.Collections;
using System.Collections.Generic;

namespace Settings
{
    /// <summary>
    /// An derived class of PropertyBag that includes a store of property values, in
    /// addition to firing events when property values are requested or set.
    /// </summary>
    public class PropertyTable : PropertyBag
    {
        protected Dictionary<string, object> mValues;

        /// <summary>
        /// Initializes a new instance of the PropertyTable class.
        /// </summary>
        public PropertyTable() : base()
        {
            mValues = new Dictionary<string,object> () ; 
        }

        /// <summary>
        /// Gets or sets the value of the property with the specified name.
        /// Since we're using a Hashtable, recall that hashtable[key] = something;
        /// will add that element using that key if it doesn't already exist, else
        /// it will update it.  That is why there are no Add/Remove methods in this Property Table class.
        /// <p>In C#, this property is the indexer of the PropertyTable class.</p>
        /// </summary>
        public object this[string key]
        {
            // TODO: the old HashTable mValues would add the entry on "Get" if it did not exist.  
            // our new Dictionary<string, object> which allows a strongly type key, does not so must
            // be initialized first.
            get { return mValues[key]; }
            set { mValues[key] = value; }
        }

        /// <summary>
        /// This member overrides PropertyBag.OnGetValue.
        /// </summary>
        protected override void OnGetValue(PropertySpecEventArgs e)
        {
            e.Value = mValues[e.Property.Name];
            base.OnGetValue(e);
        }

        /// <summary>
        /// This member overrides PropertyBag.OnSetValue.
        /// </summary>
        protected override void OnSetValue(PropertySpecEventArgs e)
        {
            mValues[e.Property.Name] = e.Value;
            base.OnSetValue(e);
        }
    }
}
