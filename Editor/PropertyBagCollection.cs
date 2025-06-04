using System;
using System.Collections.Generic;
using System.ComponentModel;
using MTV3D65;
using Settings;

namespace KeyEdit
{

    /// <summary>
    /// A custom property table. Recall that PropertyTable itself inherits from PropertyBag and extends it by
    /// adding a value store for holding the non default values of properties.
    /// </summary>
    public class PropertyBagCollection : Settings.PropertyTable
    {

        public PropertyBagCollection(Initialization ini) : base()
        {
            if (ini == null) throw new ArgumentNullException();

            this.GetValue += Property_GetValue;
            this.SetValue += Property_SetValue;

            foreach (IniSection sec in ini.sections)
            {
                foreach (KeyValuePair kvp in sec.keys)
                {
                    PropertySpec propertySpec;
                    
                    // if its a tv3d constant, we need to pass it as a Type and not a fully qualified name
                    string typeName = kvp.Type ;
                    object value = kvp.Value;

                    // + indicated an enum. We use assembly qualifed name such as
                    // AssemblyQualifiedName: "Keystone.Cameras.Viewport+ProjectionType, Keystone, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
                    if (kvp.Type.Contains("+")) 
                    {
                        //for enumerations, we need to convert the string form of the integral value to the actual type or else the default value will not show up in the grid
                        Type enumType = Type.GetType(typeName, false, false);
                        value = EnumHelper.EnumeratedMemberValue(enumType, kvp.Value);
                    }
                    if (kvp.Type.Contains("CONST_TV"))
                    {
                        // get the MTV3D65 qualified assembly name because we need to append this to every
                        // kvp.Type that uses a CONST_TV_* or else the PropertySpec will not be able to convert the typename
                        string fullName = typeof(CONST_TV_ACTORMODE).AssemblyQualifiedName;
                        fullName = fullName.Substring(fullName.IndexOf(","));
                        typeName = "MTV3D65." + kvp.Type + fullName;
                        //for enumerations, we need to convert the string form of the integral value to the actual type or else the default value will not show up in the grid
                        Type enumType = Type.GetType(typeName, false, false);
                        Enum instance = (Enum) Enum.ToObject(enumType , Convert.ToInt32(value));
                        value = instance; 
                    }
                    if (kvp.Type == "ADAPTERS")
                        propertySpec = new PropertySpec(kvp.Name, typeof(string), sec.name, kvp.Description, value, "", typeof(AdapterListConverter));
                    else if (kvp.Type == "DISPLAYMODES")
                        //special case for display modes. 
                        // for each adapter in our DeviceCaps, add a propertyspec and another to contain the available modes for that type
                        // the kvp.Value contains the adapter name, so set that as the current one in the DeviceCaps
                        // and whenever the user changes that adapter, update the DeviceCaps in Property_SetValue()
                        propertySpec = new PropertySpec(kvp.Name, typeof(string), sec.name, kvp.Description, value, "", typeof(DisplayModeConverter));
                    else if (kvp.Type == "SOUNDCARDS")
                        propertySpec = new PropertySpec(kvp.Name, typeof(string), sec.name, kvp.Description, value, "", typeof(SoundCardListConverter));
                    else if (kvp.Type == "SPEAKERS")
                        propertySpec = new PropertySpec(kvp.Name, typeof(string), sec.name, kvp.Description, value, "", typeof(SpeakerListConverter));
                    else
                        propertySpec = new PropertySpec(kvp.Name, typeName, sec.name, kvp.Description, value, "", kvp.Converter);
                    
                    //kvp.UIEditor e.g. like for a special color picker or calender, etc.

                    mValues.Add(sec.name + kvp.Name, value);                    
                    propertySpec.DisplayName = kvp.Display; 

                    this.Properties.Add(propertySpec);
                }
            }


            
            //debug 
            // -show boxes, tvdebug log settings+path, app debug log path
            //controls - should allow user to switch current mode
            //     system, edit, 3rd person -- this should show the keybinds listing (system should be readonly)
            //     mouse sensitivity, keyboard text/chat repeat delay (note: the keyboard device handler should 
            //          be responsible for generating the repeat keys to the Observers.
            //engine
            //  adapter settings
            //  devicecaps -- just shows all the caps
            //  shadermode, etc, fpuPrecision, smoothtime
            //network
            //   server info
            //simulation
            //   time (timeStep 1 sec = 1minute, timeofDay, month/year)
            //environment
            //   weather - rain
            //   water
            //   sky
            //   fog
            //controls
            //audio
            //video - basic things like resolution, fullscreen, etc
            //    advanced - VSync, xAA, wireframe, 
        }

        public Settings.PropertySpec[] PropertiesByCategory(string category)
        {
            if (mSpecs == null || mSpecs.Count == 0) return null;
            List<PropertySpec> specs = new List<PropertySpec>();
            for (int i = 0; i < mSpecs.Count ;i++)
                if (mSpecs[i].Category == category)
                    specs.Add (mSpecs[i]);

            return specs.ToArray ();
        }

        public string[] GetCategoryNames()
        {
            if (mSpecs == null || mSpecs.Count == 0) return null;

            // a null or empty string for category is a valid category name
            System.Collections.Hashtable categories = new System.Collections.Hashtable();
            for (int i = 0; i < mSpecs.Count; i++)
                if (categories.Contains(mSpecs[i].Category) == false)
                    categories.Add(mSpecs[i].Category, null);

            List<string> results = new List<string>();
            foreach (object key in categories.Keys)
                results.Add ((string)key);

            return results.ToArray();
        }

        // called when property spec is accessed for it's value
        private void Property_GetValue(object sender, PropertySpecEventArgs e)
        {
            e.Value = mValues[e.Property.Category + e.Property.Name];
        }

        // TODO: why on earth do i not pass these Property_GetValue/Property_SetValue methods into this PropertyBagCollection
        // rather than implement them here?  Simple fix but boy how stupid for me to not
        // just implement these handlers in AppMain.cs or FormMain 
        private void Property_SetValue(object sender, PropertySpecEventArgs e)
        {
            if (e.Property.Name == "adapter")
                AppMain._core.DeviceCaps.CurrentAdapterName = (string)e.Value;
            else if (e.Property.Name == "soundcards")
                AppMain._core.AudioManager.CurrentSoundCard = new Guid((string)e.Value);
            else if (e.Property.Name == "speakers")
                AppMain._core.AudioManager.CurrentSpeaker = (string)e.Value; 
                
            // TODO: How do i set the default value for some of these list items?  Normally
            // i set the "value" attribute in the XML, but for these sorts where it produces from a variable
            // list, there's no way to know a default value in advance. If we could instead pick the index=0 item...
            
            //TODO: Not sure how to determine from here whether the current displaymode
            // is available on the current adapter in scenarios where they change the adapter
            // but leave the display mode setting unchanged from the previous adapter setting. 
            // I mean i can "set" it in the Graphics Manager
            // from here and then have it do the verification, but it still doesnt solve the problem
            // of getting the user to select a valid one in the grid.
            mValues[e.Property.Category + e.Property.Name] = e.Value;
        }

      
        public class DisplayModeConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, 
                //but allow free-form entry
                return true;
            }

            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {
                // this call would have to always get the list from the current selected Adapter
                // so in our Get/Set values, we simply need to set that in the DeviceCaps. 
                // sometimes these sorts of global hacks are necessary.  But we keep them to absolute minimum
                return new StandardValuesCollection(AppMain._core.DeviceCaps.DisplayModes);
            }
        }

        public class AdapterListConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, 
                //but allow free-form entry
                return true;
            }

            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {
                // this call would have to always get the list from the current selected Adapter
                // so in our Get/Set values, we simply need to set that in the DeviceCaps. 
                // sometimes these sorts of global hacks are necessary.  But we keep them to absolute minimum
                return new StandardValuesCollection(AppMain._core.DeviceCaps.AvailableAdapters);
            }
        }

        public class SoundCardListConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, 
                //but allow free-form entry
                return true;
            }

            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(AppMain._core.AudioManager.AvailableSoundCards);
            }
        }

        public class SpeakerListConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                //true means show a combobox
                return true;
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                //true will limit to list. false will show the list, 
                //but allow free-form entry
                return true;
            }

            public override System.ComponentModel.TypeConverter.StandardValuesCollection
                   GetStandardValues(ITypeDescriptorContext context)
            {

                return new StandardValuesCollection(AppMain._core.AudioManager.AvailableSpeakers);
            }
        }
        
        public Initialization ToInitializationObject()
        {
            if (mSpecs == null || mSpecs.Count == 0) return null;

            Initialization ini = new Initialization();

            // we need to get a list of all categories within our mSpecs
            foreach (PropertySpec spec in mSpecs)
            {
                KeyValuePair kvp = new KeyValuePair();
                kvp.Name = spec.Name;
                kvp.Type = spec.TypeName;
                if (kvp.Type.Contains("MTV3D65"))
                {
                    // strip the fully qualified part of the type for CONST_TV.  its simple by just grabbing everything left of the first comma.
                    // if we dont strip it, then everytime the tv3d dll version changes, we'll need to go back and edit them all.
                    string qualifiedname = kvp.Type;
                    int firstperiod = kvp.Type.IndexOf(".");
                    qualifiedname = kvp.Type.Substring(firstperiod + 1);
                    firstperiod = qualifiedname.IndexOf(",");
                    qualifiedname = qualifiedname.Substring(0, firstperiod);
                    kvp.Type = qualifiedname;
                    kvp.Value = ((int)mValues[spec.Category + spec.Name]).ToString(); //   //spec.DefaultValue.ToString();
                }
                else
                {
                    kvp.Value = mValues[spec.Category + spec.Name].ToString(); //   //spec.DefaultValue.ToString();
                }
                kvp.Description = spec.Description;
                kvp.Converter = spec.ConverterTypeName;
                kvp.Display = spec.DisplayName;

                //kvp.UITypeEditor
                ini.settingWrite(spec.Category, kvp);
            }

            // obsolete - used before this PropertyBagCollection switched to be derived inherited type of PropertyTable
            //foreach (string key in mSpecs)
            //{
            //    ini.sectionAdd(key);
            //    PropertyBag b = PropertyBags[key];
            //    foreach (PropertySpec spec in b.Properties)
            //    {
            //        KeyValuePair kvp = new KeyValuePair();
            //        kvp.Name = spec.Name;
            //        kvp.Type = spec.TypeName;
            //        if (kvp.Type.Contains("MTV3D65"))
            //        {
            //            // strip the fully qualified part of the type for CONST_TV.  its simple by just grabbing everything left of the first comma.
            //            // if we dont strip it, then everytime the tv3d dll version changes, we'll need to go back and edit them all.
            //            string qualifiedname = kvp.Type;
            //            int firstperiod = kvp.Type.IndexOf(".");
            //            qualifiedname = kvp.Type.Substring(firstperiod + 1);
            //            firstperiod = qualifiedname.IndexOf(",");
            //            qualifiedname = qualifiedname.Substring(0, firstperiod);
            //            kvp.Type = qualifiedname;
            //            kvp.Value = ((int)mValues[spec.Category + spec.Name]).ToString(); //   //spec.DefaultValue.ToString();
            //        }
            //        else
            //        {
            //            kvp.Value = mValues[spec.Category + spec.Name].ToString(); //   //spec.DefaultValue.ToString();
            //        }
            //        kvp.Description = spec.Description;
            //        kvp.Converter = spec.ConverterTypeName;
            //        kvp.Display = spec.DisplayName;

            //        //kvp.UITypeEditor
            //        ini.settingWrite(key, kvp);
            //    }
            //}
            return ini;
        }
    }
}