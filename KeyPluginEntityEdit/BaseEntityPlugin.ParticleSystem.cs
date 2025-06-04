using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;
using Keystone.Types;
using System.Reflection;
using System.Resources;
using System.IO;
using System.Linq;

namespace KeyPlugins
{
    partial class BaseEntityPlugin
    {
        string mParticleSystemID;

        private void CreateAttractorPanel(string modelID, string particleSystemID, string attracotrTreeNodeName)
        {
            mHost.PluginChangesSuspended = true;

            // create the property card (which is laid out identicly to the emitter card)
            EmitterCard attractorCard = CreateEmitterCard(particleSystemID, attracotrTreeNodeName);
            attractorCard.OnPropertyValueChanged += OnAttractorPropertyChanged;
            superTabControlPanel12.Controls.Add(attractorCard);

            const int GEOMETRY_PARAM = 1; // empty or 0 is for Emitters and 1 is for Attractors
            Settings.PropertySpec[] attractorProperties = (Settings.PropertySpec[])mHost.Geometry_GetGroupProperties(particleSystemID, attractorCard.mEmitterIndex, GEOMETRY_PARAM);

            attractorCard.SetGridProperties(attractorProperties);
            mHost.PluginChangesSuspended = false;
        }

        private void CreateEmitterPanel(string modelID, string particleSystemID, string emitterTreeNodeName)
        {
            mHost.PluginChangesSuspended = true;

            // NOTE: A DefaultAppearance or GroupAttribute should exist because they should be created during Emitter.Add and removed during Emitter.Delete
            // todo: UGH, GroupAttributes may need to re-order based on adding and removing of emitters 
            // todo; need right menu option to move an emitter up or down.
            // todo: attractors should always be below all emitters
            // todo: need to remove keyframes for any emitter that is deleted
            int emitterCount = (int)mHost.Node_GetProperty(particleSystemID, "emittercount");
            string appearanceID = mHost.Node_GetChildOfType(modelID, "DefaultAppearance");
            // mHost.Node_MoveChildOrder


            // todo: this should show up when clicking the actual ParticleSystem node in the model tree, not when clicking one of the emitter #N nodes.
            Settings.PropertySpec[] systemProperties = mHost.Node_GetProperties(particleSystemID);
            lblGroupCount.Text = "Emitters: " + ((int)systemProperties[0].DefaultValue).ToString();
            lblVertices.Text = "Attractors: " + ((int)systemProperties[1].DefaultValue).ToString();

            // Create the emitterCard and assign properties
            EmitterCard emitterCard = CreateEmitterCard(particleSystemID, emitterTreeNodeName);
            emitterCard.OnPropertyValueChanged += OnEmitterPropertyChanged;
            // emitterCard.OnResourceChanged // I think this is only for the overall particle system and not needed here
            superTabControlPanel11.Controls.Add(emitterCard);

            Settings.PropertySpec[] emitterProperties = (Settings.PropertySpec[])mHost.Geometry_GetGroupProperties(particleSystemID, emitterCard.mEmitterIndex);

            // todo: the emitter type should be listed in the Model Tree along with it's "name". Also it should be removed from the emitterProperties we assign to the emittercard
            // todo: we want to swap the the "int" types for things like BlendingMode and EMITTER_SHAPE
            //       - we need dropdown comboboxes to store the list of allowed values.  I think we have this working for script's customproperties ?in fact i think in userconstants or userfunctions i have the arrays defined there for things like materials, craftsmenship, etc
            //       - we can use strings, but then when changing the value we need to convert it back to an int
            int length = emitterProperties.Length - 4; // the emitteruseage, particleuseage, emitterkeyframes and particlekeyframes are the four last entires we don't want added to the propertyGrid

            Settings.PropertySpec[] p = new Settings.PropertySpec[length];

            for (int i = 0; i < length; i++)
            {
                p[i] = emitterProperties[i];
                ReplaceParticleEnumType(p[i]);
            }

            emitterCard.SetGridProperties(p); // p holds all emitter properties EXCEPT the keyframes for emitter and particle


            InitializeUseageMenu(toolStripMenuEmitterUseage, (int)emitterProperties[emitterProperties.Length - 4].DefaultValue);
            InitializeUseageMenu(toolStripMenuParticleUseage, (int)emitterProperties[emitterProperties.Length - 3].DefaultValue);

            // NOTE: we don't add the keyframe properties to the Emitter property card.  Those will get added to the SourceGrids controls.
            Keystone.KeyFrames.EmitterKeyframe[] emitterKeyFrames = (Keystone.KeyFrames.EmitterKeyframe[])emitterProperties[emitterProperties.Length - 2].DefaultValue;
            Keystone.KeyFrames.ParticleKeyframe[] particleKeyFrames = (Keystone.KeyFrames.ParticleKeyframe[])emitterProperties[emitterProperties.Length - 1].DefaultValue;

            InitializeEmitterGrid(emitterKeyFrames);
            InitializeParticleGrid(particleKeyFrames);

            gridEmitter.BorderStyle = BorderStyle.Fixed3D;
            gridParticle.BorderStyle = BorderStyle.Fixed3D;



            // todo: when adding a particleSystem emitter and emitterCount == 0, add a DefaultAppearance to the Model


            // todo: when adding/deleting existing emitters, i think internally, tv doesn't update their indices

            // todo: need a save as button to do a binary save of the particle system on command
            //       All duplicates that use this resource should be updated. The main reason is, the mSystem 
            //       probably needs to be recreated since the tvindex likely will change.

            mHost.PluginChangesSuspended = false;
        }

        //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_POINT // = 0;
        //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHEREVOLUME // =1;
        //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXVOLUME // = 2;
        //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_SPHERESURFACE // = 3;
        //MTV3D65.CONST_TV_EMITTERSHAPE.TV_EMITTERSHAPE_BOXSURFACE // = 4;
        // NOTE: the enum values match the CONST_TV_EMITTERSHAPE values
        private enum EmitterShape : int
        {
            Point = 0,
            Sphere,
            Box,
            SphereSurface,
            BoxSurface
        }
        
        // NOTE: there is no 0
        //CONST_TV_BLENDEX.TV_BLENDEX_ZERO = 1
        //CONST_TV_BLENDEX.TV_BLENDEX_ONE = 2
        //CONST_TV_BLENDEX.TV_BLENDEX_SRCCOLOR = 3
        //CONST_TV_BLENDEX.TV_BLENDEX_INVSRCCOLOR = 4
        //CONST_TV_BLENDEX.TV_BLENDEX_SRCALPHA = 5
        //CONST_TV_BLENDEX.TV_BLENDEX_INVSRCALPHA = 6
        //CONST_TV_BLENDEX.TV_BLENDEX_DESTALPHA = 7
        //CONST_TV_BLENDEX.TV_BLENDEX_INVDESTALPHA = 8
        private enum EmitterBlendEx : int
        {
            ZERO = 1,
            ONE = 2,
            SRCCOLOR = 3,
            INVSRCCOLOR = 4,
            SRCALPHA = 5,
            INVSRCALPHA = 6,
            DESTALPHA = 7,
            INVDESTALPHA = 8
        }

        //CONST_TV_BLENDINGMODE.TV_BLEND_NO = 0
        //CONST_TV_BLENDINGMODE.TV_BLEND_ALPHA = 1
        //CONST_TV_BLENDINGMODE.TV_BLEND_ADD = 2
        //CONST_TV_BLENDINGMODE.TV_BLEND_COLOR = 3
        //CONST_TV_BLENDINGMODE.TV_BLEND_ADDALPHA = 4
        //CONST_TV_BLENDINGMODE.TV_BLEND_MULTIPLY = 5
        private enum EmitterBlendingMode : int
        {
            NO = 0,
            ALPHA = 1,
            ADD = 2,
            COLOR = 3,
            ADDALPHA = 4,
            MULTIPLY = 5
        }

        //CONST_TV_PARTICLECHANGE.TV_CHANGE_ALPHA = 1
        //CONST_TV_PARTICLECHANGE.TV_CHANGE_COLOR = 2
        //CONST_TV_PARTICLECHANGE.TV_CHANGE_NO = 3
        // NOTE: There is no 0
        private enum EmitterChange : int
        {
            ALPHA = 1,
            COLOR = 2,
            NO = 3
        }

        private void ReplaceParticleEnumType(Settings.PropertySpec spec)
        {
            switch (spec.Name)
            {
                // NOTE: we don't need to specify a converter since the TypeName is set correctly
                // spec.ConverterTypeName = typeof(EmitterShapceConverter).AssemblyQualifiedName; 
                case "shape":
                    // the value is an int, but we want to display a combobox of enumerated names 
                    // but then, when the user makes a selection, for the result to be sent to the 
                    // mHost as an int.  This is necessary because we don't want for our plugins
                    // to require a MTV3D65 dll reference.
                    spec.DefaultValue = (EmitterShape)spec.DefaultValue;
                    spec.TypeName = typeof(EmitterShape).AssemblyQualifiedName;
                    break;
                case "blendingmode": // NOTE: for minimesh the blendingmode must be set directly on the minimesh via the GroupAtttribute
                    spec.DefaultValue = (EmitterBlendingMode)spec.DefaultValue;
                    spec.TypeName = typeof(EmitterBlendingMode).AssemblyQualifiedName;
                    break;
                case "blendexsrc":
                case "blendexdest":
                    spec.DefaultValue = (EmitterBlendEx)spec.DefaultValue;
                    spec.TypeName = typeof(EmitterBlendEx).AssemblyQualifiedName;
                    break;
                case "change":
                    spec.DefaultValue = (EmitterChange)spec.DefaultValue;
                    spec.TypeName = typeof(EmitterChange).AssemblyQualifiedName;
                    break;
                
                default:
                    break;
            }
        }

        private Settings.PropertySpec RestoreParticleEnumType(Settings.PropertySpec spec)
        {
            switch (spec.Name)
            {
                case "shape": 
                case "blendingmode":
                case "blendexsrc":
                case "blendexdest":
                case "change":
                    // NOTE: we MUST create a new PropertySpec because the existing one will still be used by the PropertyGrid!
                    //       If we don't do this, a NullReferenceException will occur in AppMain main loop
                    Settings.PropertySpec restored = new Settings.PropertySpec(spec.Name, typeof(int).Name);
                    restored.DefaultValue = (int)spec.DefaultValue;
                    // Assignging to the original spec below is WRONG.
                    //spec.DefaultValue = (int)spec.DefaultValue;
                    //spec.TypeName = typeof(int).Name;
                    return restored;
                default:
                    return spec;
            }
        }

        EmitterCard CreateEmitterCard(string particleSystemID, string treeNodeName)
        {
            // if the emittercard already exists, just remove it and we'll create a new one
            foreach (Control c in superTabControlPanel11.Controls)
            {
                if (c is NotecardBase == false) continue;
                // c.SelectedObject = null;
                c.Dispose(); // override dispose to remove events?
                c.Visible = false;
            }

            string[] s = treeNodeName.Split('#');
            int emitterIndex = int.Parse(s[1]); //  we grab the emitterIndex from the emitterTreeNodeName which is in the form "Emitter #N"
            mGroupIndex = emitterIndex; // todo: this could be wrong.  we need the emitter.Index which may not match its array position in ParticleSystem.mEmitters

            mParticleSystemID = particleSystemID;
            EmitterCard emitterCard = new EmitterCard(mParticleSystemID, emitterIndex);
            emitterCard.Text = treeNodeName;

            // todo: we need the path to the .tvp binary and ability to browse for a new one
            // todo: need ability to save the ParticleSystem.Save(path) via the save as dialog.
            // todo: need toolbars for adding/removing keyframes to each SourceGrid
            // - useage needs a dropdown mnenu where we can check mark all the useages we want for the keyframes to use
            // https://www.codeproject.com/Articles/13793/A-UITypeEditor-for-easy-editing-of-flag-enum-prope

            
            emitterCard.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            emitterCard.Top = 5;
            emitterCard.Left = 5;
            const int bottomPadding = 6;
            double height = superTabControlPanel11.Size.Height;
            Size size = new Size(emitterCard.Width, superTabControlPanel11.Height - bottomPadding);
            emitterCard.Size = size;

            return emitterCard;

        }

        private void menuAddParticleSystem_Click(object sender, EventArgs e)
        {
            string modelID = ((ToolStripMenuItem)sender).Name;

            if (ChildOfTypeAlreadyExists(modelID, new string[] { "Mesh3d", "Actor3d", "Billboard", "ParticleSystem" }))
            {
                MessageBox.Show("A child of type 'Geometry' already exists under this parent.  You must delete the existing 'Geometry' node first before replacing it with another type of 'Geometry' node.");
                return;
            }

            if (KeyPlugins.StaticControls.YesNoBox("Browse for existing particle system?", "Browse?") == DialogResult.No)
            {
                
                // todo: open an input box to name this ParticleSystem so it can be saved
                // create a default empty particle system
                string validName = "";
                if (KeyPlugins.StaticControls.InputBox("Enter a name for this new ParticleSystem", "Name:", ref validName) == DialogResult.OK)
                {
                    char[] invalidChars = System.IO.Path.GetInvalidPathChars();
                    for (int i = 0; i < invalidChars.Length; i++)
                        if (validName.Contains(invalidChars[i].ToString()))
                        {
                            MessageBox.Show("Invalid character in filename");
                            return;
                        }


                    if (!validName.ToLower().EndsWith(".tvp"))
                    {
                        validName += ".tvp";
                    }

                    var assembly = Assembly.GetExecutingAssembly();
                    string resourceName = assembly.GetManifestResourceNames()
                        .Single(str => str.EndsWith("default.tvp"));

                    string resourcePath = "caesar\\particles\\" + validName;
                    string filePath = System.IO.Path.Combine(mHost.Engine_GetModPath(), resourcePath);

                    // check if the filePath already exists and prompt for overwrite
                    if (File.Exists(filePath))
                    {
                        DialogResult result = MessageBox.Show("File with that name already exists.  Do you wish to overwrite the existing file?", "Overwrite existing file?", MessageBoxButtons.YesNoCancel);
                        if (result != DialogResult.Yes) return;
                    }

                    // copy the default empty particleSystem found in project resources to the selected filename
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(file);
                    }

                    mHost.Geometry_Add(mTargetNodeID, modelID, resourcePath, true, false); // NOTE: the Model might alreayd have loaded Geometry, we should check and inform user they must delete existing geometry.
                }
            }
            else
            {

                string filter = "Particle Systems|*.tvp";
                System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog();
                open.Filter = filter;
                open.InitialDirectory = this.mModFolderPath;

                if (open.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;

                string resourcePath = open.FileName;
                int modPathLength = this.mModFolderPath.Length + 1;
                // use relative path to "mods" folder so that code works on any install folder location
                resourcePath = resourcePath.Remove(0, modPathLength);

                // todo: should we validate the file is of correct type/version before making call to mHost?

                // todo: we need to delete model.Appearance and then recreate the layout for the loaded ParticleSystem
                // todo: texturepath, minimesh path, shader path need to result in creation of those NoteCards under their respective GroupAttributes
                // todo: we need to refresh the tree after Plugin Notification
                // TODO: we need to remove any existing DefaultAppearance and GroupAttributes since those are auto-managed for ALL geometry.
                //       TODO: perhaps this should be done when deleting any existing Geometry and not waiting for a new Geometry to be added in it's place.
                // todo: verify we can't add more than 1 geometry per Model.  A messagebox should appear to delete any existing Geometry first.
                // TODO: actually, this is more like IMPORTING an actor or mesh and the same auto-managing of GrouopAttributes applies
                //        Whenever we add geometry to a Model, it should behaves as if its importing that geometry and creating all the GroupAttributes when applicable
                // TODO; we need to clear existing models of all GroupAttributes
                // todo: Geometry_Add() needs to work for all geometry types (mesh, billboard, actor, particle system)
                // NOTE: Geometry_Add() assumes all geometry, shaders, textures and materials exist under the modpath already.  This is not "Importing" (or copying to new folders) anything.
                //       The key is creating the DefaultAppearance and its child GroupAttributes so that we have 1:1 relationship with geometry Groups or Emitters
                //       Hmm, but really the code is identical to regular Import except we want to use the existing Entity and Model and not credate new ones and we want to use the \\Particles dirrectory for textures and we don't need to save the prefab immediately either
                // mHost.Node_Create("ParticleSystem", modelID, resourcePath, filter);
                mHost.Geometry_Add(mTargetNodeID, modelID, resourcePath, true, false); // NOTE: the Model might alreayd have loaded Geometry, we should check and inform user they must delete existing geometry.
            }

            //string fileDialogFilter = "Particle Systems|*.tvp";
            //BrowseNewResource("ParticleSystem", ((ToolStripMenuItem)sender).Name, fileDialogFilter);

            base.SelectTarget(null, mTargetNodeID, "ModeledEntity");
            buttonModel.RaiseClick();
        }

        private void menuAddEmitterPointSprite(object sender, EventArgs e)
        {
            string groupName = "";
            if (KeyPlugins.StaticControls.InputBox("Emiter name", "Name:", ref groupName) != DialogResult.OK) return;

            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!(sender is ToolStripMenuItem)) return;

            string geometryID = (string)item.Name;
            string modelID = (string)item.Tag;
            // todo: i need to either popup a dialog where user can set the type, or i need seperate nested emitter menu items for each type
            int groupType = 0; // 0 == pointsprite, 1 == billboard, 2 == minimesh

            // TODO: we need to refresh the tree after getting a NodeCreated event
            mHost.Geometry_CreateGroup(modelID, geometryID, groupName, groupType);
            
            base.SelectTarget(null, mTargetNodeID, "ModeledEntity");
            buttonModel.RaiseClick();
        }


        private void menuAddEmitterBillboard(object sender, EventArgs e)
        {
            string groupName = "";
            if (KeyPlugins.StaticControls.InputBox("Emiter name", "Name:", ref groupName) != DialogResult.OK) return;

            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!(sender is ToolStripMenuItem)) return;

            string geometryID = (string)item.Name;
            string  modelID = (string)item.Tag;
            // todo: i need to either popup a dialog where user can set the type, or i need seperate nested emitter menu items for each type
            int groupType = 1; // 0 == pointsprite, 1 == billboard, 2 == minimesh

            // TODO: we need to refresh the tree after getting a NodeCreated event
            mHost.Geometry_CreateGroup(modelID, geometryID, groupName, groupType);

            base.SelectTarget(null, mTargetNodeID, "ModeledEntity");
            buttonModel.RaiseClick();
        }

        private void menuAddEmitterMinimesh(object sender, EventArgs e)
        {
            string groupName = "";
            if (KeyPlugins.StaticControls.InputBox("Emiter name", "Name:", ref groupName) != DialogResult.OK) return;

            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!(sender is ToolStripMenuItem)) return;

            string geometryID = (string)item.Name;
            string modelID = (string)item.Tag;
            // todo: i need to either popup a dialog where user can set the type, or i need seperate nested emitter menu items for each type
            int groupType = 2; // 0 == pointsprite, 1 == billboard, 2 == minimesh
            int groupClass = 0; // 0 == emitter, 1 == attractor

            string fileDialogFilter = "Meshess|*.obj;*.x;*.tvm";

            System.Windows.Forms.FileDialog browser = new System.Windows.Forms.OpenFileDialog();
            browser.InitialDirectory = this.mModFolderPath;
            browser.RestoreDirectory = false;
            browser.Filter = fileDialogFilter; 
            System.Windows.Forms.DialogResult result = browser.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
                return;
            
            // resourcePath = new KeyCommon.IO.ResourceDescriptor(browser.ModName.ToLower(), browser.ModDBSelectedEntry.ToLower()).ToString();
            string resourcePath = browser.FileName;
            int modPathLength = this.mModFolderPath.Length + 1;
            // use relative path to "mods" folder so that code works on any install folder location
            resourcePath = resourcePath.Remove(0, modPathLength);
            
            if (string.IsNullOrEmpty(resourcePath))
                return;
            
            // TODO: we need to refresh the tree after getting a NodeCreated event
            // todo: i need to be ale to pass in the geometry path and a bool for whether it should load textures - otherwise, i manipulate the properties and assign the the model path and then create the mninimewsh
            //      actually what i coudl do, is in the groupName property use a comma delimited string to include the selected model path and particle count and whether to load texture
            mHost.Geometry_CreateGroup(modelID, geometryID, groupName, groupType, groupClass, resourcePath);
            
            base.SelectTarget(null, mTargetNodeID, "ModeledEntity");
            buttonModel.RaiseClick();
        }

        private void menuiAddAttractor_Click(object sender, EventArgs e)
        {
            string groupName = "";
            if (KeyPlugins.StaticControls.InputBox("Emiter name", "Name:", ref groupName) != DialogResult.OK) return;

            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!(sender is ToolStripMenuItem)) return;

            string geometryID = (string)item.Name;
            string modelID = (string)item.Tag;
            mHost.Geometry_CreateGroup(modelID, geometryID, groupName, 1);
            // todo: this will fail on certain nodes in our plugin that aren't Nodes such as Emitters and Attractors
            System.Diagnostics.Debug.WriteLine("Geometry_ChangeGroupProperty() for Deleting Emitter command sent...");
        }

        private void Save_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!(sender is ToolStripMenuItem)) return;

            // NOTE: the geometryID is like all Geometry, the same as the relative resource path
            string geometryID = (string)item.Name;
            string modelID = (string)item.Tag;

            // pop-up a save dialog with starting location as the current .tvp file and file filter set to .tvp files
            string fileDialogFilter = "Particle Systems|*.tvp";
            SaveGeometryAs(geometryID, modelID, fileDialogFilter);

        }

        private void Delete_Emitter_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!(sender is ToolStripMenuItem)) return;

            string[] s = item.Name.Split(new char[] { ',' });
            string geometryID = s[0];
            s = s[1].Split(new char[] { '#' });
            mGroupIndex = int.Parse(s[1]);

            string modelID = (string)item.Tag;
            // TODO: this should remove the related GroupAttribute
            mHost.Geometry_RemoveGroup(modelID, geometryID, mGroupIndex, 0);
            // todo: this will fail on certain nodes in our plugin that aren't Nodes such as Emitters and Attractors
            System.Diagnostics.Debug.WriteLine("Geometry_ChangeGroupProperty() for Deleting Emitter command sent...");
        }

        private void Delete_Attractor_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (ToolStripMenuItem)sender;
            if (!(sender is ToolStripMenuItem)) return;

            string[] s = item.Name.Split(new char[] { ',' });
            string geometryID = s[0];
            s = s[1].Split(new char[] { '#' });
            mGroupIndex = int.Parse(s[1]);

            string modelID = (string)item.Tag;
            mHost.Geometry_RemoveGroup(modelID, geometryID, mGroupIndex, 1);
            // todo: this will fail on certain nodes in our plugin that aren't Nodes such as Emitters and Attractors
            System.Diagnostics.Debug.WriteLine("Geometry_ChangeGroupProperty() for Deleting Attractor command sent...");
        }

        private void OnEmitterPropertyChanged(string nodeID, Settings.PropertySpec spec)
        {
            System.Diagnostics.Debug.WriteLine("Emitter Property Changed: " + spec.Name);

            // HACK - see ReplaceParticleEnumType()
            Settings.PropertySpec restored = RestoreParticleEnumType(spec);

            // mHost.Node_ChangeProperty(nodeID, spec.Name, spec.GetType(), spec.DefaultValue); // spec.GetType() does not work
            if (!mHost.PluginChangesSuspended)
                mHost.Geometry_ChangeGroupProperty(nodeID, mGroupIndex, restored.Name, restored.TypeName, restored.DefaultValue);
        }

        private void OnAttractorPropertyChanged(string nodeID, Settings.PropertySpec spec)
        {
            System.Diagnostics.Debug.WriteLine("Attractor Property Changed: " + spec.Name);
            const int CLASS_ATTRACTOR = 1;

            // mHost.Node_ChangeProperty(nodeID, spec.Name, spec.GetType(), spec.DefaultValue); // spec.GetType() does not work
            if (!mHost.PluginChangesSuspended)
                mHost.Geometry_ChangeGroupProperty(nodeID, mGroupIndex, spec.Name, spec.TypeName, spec.DefaultValue, CLASS_ATTRACTOR);
        }

        // https://www.codeproject.com/Articles/29824/ColorPicker-ColorPicker-with-a-compact-footprint-V
        // https://www.codeproject.com/Articles/19382/Not-just-another-color-picker
        // NOTE: the order of the menu items is important as they correlate with the CONST_TV_ useage flags for Particles and Emitters
        private void InitializeUseageMenu(ToolStripMenuItem parent, int flags)
        {
            int i = 0;
            foreach (ToolStripMenuItem item in parent.DropDown.Items)
            {
                int flag = 1 << i;
                item.Checked = (flags & flag) != 0;
                i++;
            }

        }


        #region Keyframe Grids
        CommonEmitterKeyFrameController mEmitterController;
        CommonParticleKeyFrameController mParticleController;

        const int ROW_HEADER = 0;
        const int EMITTER_KEYFRAMES_NUM_COLUMNS = 9;

        const int EMITTER_COLUMN_KEY = 0;
        const int EMITTER_COLUMN_POSITION = 1;
        const int EMITTER_COLUMN_DIR = 2;
        const int EMITTER_COLUMN_BOXSIZE = 3;
        const int EMITTER_COLUMN_RADIUS = 4;
        const int EMITTER_COLUMN_LIFETIME = 5;
        const int EMITTER_COLUMN_POWER = 6;
        const int EMITTER_COLUMN_SPEED = 7;
        const int EMITTER_COLUMN_COLOR = 8;


        const int PARTICLE_KEYFRAMES_NUM_COLUMNS = 4;

        const int PARTICLE_COLUMN_KEY = 0;
        const int PARTICLE_COLUMN_COLOR = 1;
        const int PARTICLE_COLUMN_SIZE = 2;
        const int PARTICLE_COLUMN_ROTATION = 3;

        bool mEmitterGridInitialized = false;
        bool mParticleGridInitialized = false;

        private void InitializeEmitterGrid(Keystone.KeyFrames.EmitterKeyframe[] keyframes)
        {
            mEmitterController = new CommonEmitterKeyFrameController(mHost, mParticleSystemID, mGroupIndex, "emitterkeyframes");

            // init editors
            mEditorString = new SourceGrid.Cells.Editors.TextBox(typeof(string));
            mEditorFloat = new SourceGrid.Cells.Editors.TextBox(typeof(float));
            mEditorColor = new SourceGrid.Cells.Editors.TextBox(typeof(Keystone.Types.Color));

            //mEditorVector = SourceGrid.Cells.Editors.Factory.Create(typeof(Keystone.Types.Vector3d));
            mEditorVector3f = new SourceGrid.Cells.Editors.TextBox(typeof(Vector3f));
            // init controllers 
            // IMPORTANT!!! These controllers should be persisted and shared for each
            // row. Trying to add a controller that is not persisted here at a module level 
            // (eg by doing grid[i,j].AddController(new FloatPropertyController(mHost, "speed"));
            // will result in a Null Exception that won't get thrown for some reason and instead just
            // hang the entire GUI even though the 3d render thread will keep going.

            gridEmitter.LinkedControls.Clear();

            gridEmitter.Redim(0, 0);
            gridEmitter.Redim(1, EMITTER_KEYFRAMES_NUM_COLUMNS); // just a single header for now
            gridEmitter.SelectionMode = SourceGrid.GridSelectionMode.Row;
            //gridAnimations.MinimumWidth = 50
            //gridAnimations.AutoSizeCells() ' <-- this makes the heights of each row insanely huge?  Why?

            for (int i = 0; i < EMITTER_KEYFRAMES_NUM_COLUMNS; i++)
                gridEmitter.Columns[i].AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSize | SourceGrid.AutoSizeMode.EnableStretch;


            gridEmitter.AutoStretchColumnsToFitWidth = true;
            //gridEmitter.AutoStretchRowsToFitHeight = true;
            gridEmitter.Columns.StretchToFit(); // <-- this evenly stretches all columns width's to fill the full width of the grid's client area
            //gridEmitter.Rows.StretchToFit();  // <-- this evenly stretches all rows heights to fill the full height of the grid's client area


            // configure the header row
            SourceGrid.Cells.Views.ColumnHeader viewColumnHeader = new SourceGrid.Cells.Views.ColumnHeader();
            DevAge.Drawing.VisualElements.ColumnHeader backHeader = new DevAge.Drawing.VisualElements.ColumnHeader();
            backHeader.BackColor = System.Drawing.Color.FromArgb(255, 71, 122, 212);
            backHeader.Border = DevAge.Drawing.RectangleBorder.NoBorder;

            viewColumnHeader.Background = backHeader;
            viewColumnHeader.ForeColor = System.Drawing.Color.White;
            viewColumnHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 12);
            viewColumnHeader.TextAlignment = DevAge.Drawing.ContentAlignment.MiddleCenter;
            //mCaptionModel = New SourceGrid.Cells.Views.Cell();
            //mCaptionModel.BackColor = gridClients.BackColor;


            // build the headers

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_KEY] = new SourceGrid.Cells.Cell("key");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_KEY].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_KEY].ColumnSpan = 1;

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_COLOR] = new SourceGrid.Cells.Cell("color");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_COLOR].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_COLOR].ColumnSpan = 1;

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_BOXSIZE] = new SourceGrid.Cells.Cell("boxsize");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_BOXSIZE].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_BOXSIZE].ColumnSpan = 1;

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_RADIUS] = new SourceGrid.Cells.Cell("radius");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_RADIUS].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_RADIUS].ColumnSpan = 1;

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_LIFETIME] = new SourceGrid.Cells.Cell("lifetime");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_LIFETIME].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_LIFETIME].ColumnSpan = 1;

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_POWER] = new SourceGrid.Cells.Cell("power");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_POWER].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_POWER].ColumnSpan = 1;

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_SPEED] = new SourceGrid.Cells.Cell("speed");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_SPEED].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_SPEED].ColumnSpan = 1;

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_DIR] = new SourceGrid.Cells.Cell("direction");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_DIR].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_DIR].ColumnSpan = 1;

            gridEmitter[ROW_HEADER, EMITTER_COLUMN_POSITION] = new SourceGrid.Cells.Cell("position");
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_POSITION].View = viewColumnHeader;
            gridEmitter[ROW_HEADER, EMITTER_COLUMN_POSITION].ColumnSpan = 1;

            if (keyframes != null && keyframes.Length > 0)
            {
                for (int i = 0; i < keyframes.Length; i++)
                {
                    AddAEmitterGridRow();
                    gridEmitter[i + 1, EMITTER_COLUMN_KEY].Value = keyframes[i].Key;
                    gridEmitter[i + 1, EMITTER_COLUMN_DIR].Value = keyframes[i].MainDirection;
                    gridEmitter[i + 1, EMITTER_COLUMN_POSITION].Value = keyframes[i].LocalPosition;
                    gridEmitter[i + 1, EMITTER_COLUMN_BOXSIZE].Value = keyframes[i].BoxSize;
                    gridEmitter[i + 1, EMITTER_COLUMN_COLOR].Value = keyframes[i].Color;
                    gridEmitter[i + 1, EMITTER_COLUMN_LIFETIME].Value = keyframes[i].Lifetime;
                    gridEmitter[i + 1, EMITTER_COLUMN_RADIUS].Value = keyframes[i].Radius;
                    gridEmitter[i + 1, EMITTER_COLUMN_POWER].Value = keyframes[i].Power;
                    gridEmitter[i + 1, EMITTER_COLUMN_SPEED].Value = keyframes[i].Speed;

                }
            }

            mEmitterGridInitialized = true;
        }
        
        private void InitializeParticleGrid(Keystone.KeyFrames.ParticleKeyframe[] keyframes)
        {
            // init controllers 
            // IMPORTANT!!! These controllers should be persisted and shared for each
            // row. Trying to add a controller that is not persisted here at a module level 
            // (eg by doing grid[i,j].AddController(new FloatPropertyController(mHost, "speed"));
            // will result in a Null Exception that won't get thrown for some reason and instead just
            // hang the entire GUI even though the 3d render thread will keep going.
            mParticleController = new CommonParticleKeyFrameController(mHost, mParticleSystemID, mGroupIndex, "particlekeyframes");
            gridParticle.LinkedControls.Clear();

            gridParticle.Redim(1, PARTICLE_KEYFRAMES_NUM_COLUMNS); // just a single header for now
            gridParticle.SelectionMode = SourceGrid.GridSelectionMode.Row;
            //gridParticle.MinimumWidth = 50
            //gridParticle.AutoSizeCells() ' <-- this makes the heights of each row insanely huge?  Why?

            for (int i = 0; i < PARTICLE_KEYFRAMES_NUM_COLUMNS; i++)
                gridParticle.Columns[i].AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSize | SourceGrid.AutoSizeMode.EnableStretch;


            gridParticle.AutoStretchColumnsToFitWidth = true;
            //gridParticle.AutoStretchRowsToFitHeight = true;
            gridParticle.Columns.StretchToFit(); // <-- this evenly stretches all columns width's to fill the full width of the grid's client area
            //gridParticle.Rows.StretchToFit();  // <-- this evenly stretches all rows heights to fill the full height of the grid's client area


            // configure the header row
            SourceGrid.Cells.Views.ColumnHeader viewColumnHeader = new SourceGrid.Cells.Views.ColumnHeader();
            DevAge.Drawing.VisualElements.ColumnHeader backHeader = new DevAge.Drawing.VisualElements.ColumnHeader();
            backHeader.BackColor = System.Drawing.Color.FromArgb(255, 71, 122, 212);
            backHeader.Border = DevAge.Drawing.RectangleBorder.NoBorder;

            viewColumnHeader.Background = backHeader;
            viewColumnHeader.ForeColor = System.Drawing.Color.White;
            viewColumnHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 12);
            viewColumnHeader.TextAlignment = DevAge.Drawing.ContentAlignment.MiddleCenter;
            //mCaptionModel = New SourceGrid.Cells.Views.Cell();
            //mCaptionModel.BackColor = gridClients.BackColor;


            // build the headers
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_KEY] = new SourceGrid.Cells.Cell("key");
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_KEY].View = viewColumnHeader;
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_KEY].ColumnSpan = 1;

            gridParticle[ROW_HEADER, PARTICLE_COLUMN_COLOR] = new SourceGrid.Cells.Cell("color");
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_COLOR].View = viewColumnHeader;
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_COLOR].ColumnSpan = 1;

            gridParticle[ROW_HEADER, PARTICLE_COLUMN_SIZE] = new SourceGrid.Cells.Cell("size");
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_SIZE].View = viewColumnHeader;
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_SIZE].ColumnSpan = 1;

            gridParticle[ROW_HEADER, PARTICLE_COLUMN_ROTATION] = new SourceGrid.Cells.Cell("rotation");
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_ROTATION].View = viewColumnHeader;
            gridParticle[ROW_HEADER, PARTICLE_COLUMN_ROTATION].ColumnSpan = 1;

            if (keyframes != null && keyframes.Length > 0)
            {
                for (int i = 0; i < keyframes.Length; i++)
                {
                    AddAParticleGridRow();
                    gridParticle[i + 1, PARTICLE_COLUMN_KEY].Value = keyframes[i].Key;
                    gridParticle[i + 1, PARTICLE_COLUMN_COLOR].Value = keyframes[i].Color;
                    gridParticle[i + 1, PARTICLE_COLUMN_ROTATION].Value = keyframes[i].Rotation;
                    gridParticle[i + 1, PARTICLE_COLUMN_SIZE].Value = keyframes[i].Size;

                }
            }

            mParticleGridInitialized = true;
        }

        private int AddAEmitterGridRow()
        {
            gridEmitter.SuspendLayout();

            int prevCount = gridEmitter.RowsCount;
            int newCount = prevCount + 1;
            gridEmitter.Redim(newCount, EMITTER_KEYFRAMES_NUM_COLUMNS);

            int rowIndex = newCount - 1; // 0 based means our actual row is one less than count

            gridEmitter[rowIndex, EMITTER_COLUMN_KEY] = new SourceGrid.Cells.Cell(0f);
            gridEmitter[rowIndex, EMITTER_COLUMN_KEY].Editor = mEditorFloat;
            gridEmitter[rowIndex, EMITTER_COLUMN_KEY].AddController(mEmitterController);

            gridEmitter[rowIndex, EMITTER_COLUMN_POSITION] = new SourceGrid.Cells.Cell(Vector3f.Zero());
            gridEmitter[rowIndex, EMITTER_COLUMN_POSITION].Editor = mEditorVector3f;
            gridEmitter[rowIndex, EMITTER_COLUMN_POSITION].AddController(mEmitterController);

            gridEmitter[rowIndex, EMITTER_COLUMN_DIR] = new SourceGrid.Cells.Cell(new Vector3f(0f, 1f, 0f));
            gridEmitter[rowIndex, EMITTER_COLUMN_DIR].Editor = mEditorVector3f;
            gridEmitter[rowIndex, EMITTER_COLUMN_DIR].AddController(mEmitterController);

            gridEmitter[rowIndex, EMITTER_COLUMN_COLOR] = new SourceGrid.Cells.Cell(Keystone.Types.Color.White);
            gridEmitter[rowIndex, EMITTER_COLUMN_COLOR].Editor = null; // mEditorColor; // mEditorNumericFloat; // mEditorFloat;
            gridEmitter[rowIndex, EMITTER_COLUMN_COLOR].AddController(mEmitterController);

            SourceGrid.Cells.Cell colorCell = (SourceGrid.Cells.Cell)gridEmitter[rowIndex, EMITTER_COLUMN_COLOR];
            System.Diagnostics.Debug.Assert(colorCell.Row.Index == rowIndex);

            // since the grid automatically resizes the linked picturebox to fit the cell, we handle SizeChanged and override Paint to draw custom picturebox with an inner rectangle representing the chosen color 
            var pictureBox = new PictureBox();
            pictureBox.Name = "color" + rowIndex.ToString();
            pictureBox.Tag = colorCell; // gridEmitter[rowIndex, EMITTER_COLUMN_COLOR];
            pictureBox.MouseClick += OnGridColor_Clicked;
            pictureBox.SizeChanged += OnGridColor_Resized;
            pictureBox.Paint += OnGridColor_Paint;
            // was unable to get a custom TypeEditor to pop-up, so we use a picturebox with LinkedControls method instead and handle the picturebox mouseclick to show our colorpicker
            // LinkedControls.Clear() must be used when re-building the grid
            gridEmitter.LinkedControls.Add(new SourceGrid.LinkedControlValue(pictureBox, new SourceGrid.Position(rowIndex, EMITTER_COLUMN_COLOR)));


            // TODO: when we add a new row and don't change any of the keyframe properties, i dont think we're sending the properties to the ParticleSystem


            gridEmitter[rowIndex, EMITTER_COLUMN_BOXSIZE] = new SourceGrid.Cells.Cell(new Vector3f(16f, 16f, 16f));
            gridEmitter[rowIndex, EMITTER_COLUMN_BOXSIZE].Editor = mEditorVector3f;
            gridEmitter[rowIndex, EMITTER_COLUMN_BOXSIZE].AddController(mEmitterController);

            // NOTE: DO NOT try to add controllers as the following shows. This will result in mysterious hangs freezes!
            // You must instead assign controllers that are persisted in module level variables.
            //gridEmitter[rowIndex, EMITTER_COLUMN_SPEED].AddController(new FloatPropertyController(mHost, "speed"));

            gridEmitter[rowIndex, EMITTER_COLUMN_RADIUS] = new SourceGrid.Cells.Cell(16f);
            gridEmitter[rowIndex, EMITTER_COLUMN_RADIUS].Editor = mEditorFloat; // mEditorNumericFloat; // mEditorFloat;
            gridEmitter[rowIndex, EMITTER_COLUMN_RADIUS].AddController(mEmitterController);

            gridEmitter[rowIndex, EMITTER_COLUMN_LIFETIME] = new SourceGrid.Cells.Cell(1f);
            gridEmitter[rowIndex, EMITTER_COLUMN_LIFETIME].Editor = mEditorFloat; // mEditorNumericFloat; // mEditorFloat;
            gridEmitter[rowIndex, EMITTER_COLUMN_LIFETIME].AddController(mEmitterController);

            gridEmitter[rowIndex, EMITTER_COLUMN_POWER] = new SourceGrid.Cells.Cell(10f);
            gridEmitter[rowIndex, EMITTER_COLUMN_POWER].Editor = mEditorFloat; // mEditorNumericFloat; // mEditorFloat;
            gridEmitter[rowIndex, EMITTER_COLUMN_POWER].AddController(mEmitterController);

            gridEmitter[rowIndex, EMITTER_COLUMN_SPEED] = new SourceGrid.Cells.Cell(10f);
            gridEmitter[rowIndex, EMITTER_COLUMN_SPEED].Editor = mEditorFloat; // mEditorNumericFloat; // mEditorFloat;
            gridEmitter[rowIndex, EMITTER_COLUMN_SPEED].AddController(mEmitterController);


            // gridEmitter.Columns(GRID_CLIENT_NUMBER).AutoSizeMode = SourceGrid.AutoSizeMode.MinimumSize Or SourceGrid.AutoSizeMode.Default
            //gridEmitter.Columns(GRID_STATE).AutoSizeMode = SourceGrid.AutoSizeMode.EnableAutoSizeView

            //gridEmitter.Columns(GRID_FILE_PROGRESS).Width = 50
            // gridEmitter.Columns(GRID_FILE_PROGRESS).AutoSizeMode = SourceGrid.AutoSizeMode.MinimumSize Or SourceGrid.AutoSizeMode.Default
            //gridEmitter[currentRow, GRID_FILE_PROGRESS] = New SourceGrid.Cells.Cell("control cell");
            //var progressBar = new ProgressBar();
            //progressBar.Name = "progressbar" + currentRow.ToString();
            //gridEmitter.LinkedControls.Add(New SourceGrid.LinkedControlValue(progressBar, new SourceGrid.Position(currentRow, GRID_FILE_PROGRESS)));


            // HACK - to get the color pictureboxc to show up in the correct position on the current row we add a fake row then delete it 
            gridEmitter.Redim(newCount + 1, EMITTER_KEYFRAMES_NUM_COLUMNS);
            gridEmitter.Redim(newCount, EMITTER_KEYFRAMES_NUM_COLUMNS);
            gridEmitter.ResumeLayout(true);

            return rowIndex;
        }

        private int AddAParticleGridRow()
        {
            gridParticle.SuspendLayout();

            int prevCount = gridParticle.RowsCount;
            int newCount = prevCount + 1;
            gridParticle.Redim(newCount, PARTICLE_KEYFRAMES_NUM_COLUMNS);

            int rowIndex = newCount - 1; // 0 based means our actual row is one less than count

            gridParticle[rowIndex, PARTICLE_COLUMN_KEY] = new SourceGrid.Cells.Cell(0f);
            gridParticle[rowIndex, PARTICLE_COLUMN_KEY].Editor = mEditorFloat;
            gridParticle[rowIndex, PARTICLE_COLUMN_KEY].AddController(mParticleController);


            gridParticle[rowIndex, PARTICLE_COLUMN_COLOR] = new SourceGrid.Cells.Cell(Keystone.Types.Color.White);
            gridParticle[rowIndex, PARTICLE_COLUMN_COLOR].Editor = null; // mEditorColor; // mEditorNumericFloat; // mEditorFloat;
            gridParticle[rowIndex, PARTICLE_COLUMN_COLOR].AddController(mParticleController);

            SourceGrid.Cells.Cell colorCell = (SourceGrid.Cells.Cell)gridParticle[rowIndex, PARTICLE_COLUMN_COLOR];
            System.Diagnostics.Debug.Assert(colorCell.Row.Index == rowIndex);

            // since the grid automatically resizes the linked picturebox to fit the cell, we handle SizeChanged and override Paint to draw custom picturebox with an inner rectangle representing the chosen color 
            var pictureBox = new PictureBox();
            pictureBox.Name = "color" + rowIndex.ToString();
            pictureBox.Tag = colorCell; // gridEmitter[rowIndex, EMITTER_COLUMN_COLOR];
            pictureBox.MouseClick += OnGridColor_Clicked;
            pictureBox.SizeChanged += OnGridColor_Resized;
            pictureBox.Paint += OnGridColor_Paint;
            // was unable to get a custom TypeEditor to pop-up, so we use a picturebox with LinkedControls method instead and handle the picturebox mouseclick to show our colorpicker
            // LinkedControls.Clear() must be used when re-building the grid
            gridParticle.LinkedControls.Add(new SourceGrid.LinkedControlValue(pictureBox, new SourceGrid.Position(rowIndex, PARTICLE_COLUMN_COLOR)));
            

            gridParticle[rowIndex, PARTICLE_COLUMN_SIZE] = new SourceGrid.Cells.Cell(new Vector3f(16f, 16f, 16f));
            gridParticle[rowIndex, PARTICLE_COLUMN_SIZE].Editor = mEditorVector3f;
            gridParticle[rowIndex, PARTICLE_COLUMN_SIZE].AddController(mParticleController);
            // NOTE: DO NOT try to add controllers as the following shows. This will result in mysterious hangs freezes!
            // You must instead assign controllers that are persisted in module level variables.
            //gridParticle[rowIndex, EMITTER_COLUMN_DIR].AddController(new mEditorVector(mHost, "size"));

            gridParticle[rowIndex, PARTICLE_COLUMN_ROTATION] = new SourceGrid.Cells.Cell(Vector3f.Zero(), typeof(Keystone.Types.Vector3f));
            gridParticle[rowIndex, PARTICLE_COLUMN_ROTATION].Editor = mEditorVector3f; // mEditorNumericFloat; // mEditorFloat;
            gridParticle[rowIndex, PARTICLE_COLUMN_ROTATION].AddController(mParticleController);

            // HACK to get the color column's filled rects to show up in the correct spot
            gridParticle.Redim(newCount + 1, PARTICLE_KEYFRAMES_NUM_COLUMNS);
            gridParticle.Redim(newCount, PARTICLE_KEYFRAMES_NUM_COLUMNS);

            return rowIndex;
        }

        // NOTE: when clicking the Add Button, this change the default color from white to Red which triggers mHost.Geometry_ChangeProperty call in the controller
        private void UpdateEmitterRowValues(int rowIndex)
        {
            gridEmitter[rowIndex, EMITTER_COLUMN_COLOR].Value = (object)Keystone.Types.Color.Red;
        }

        // NOTE: when clicking the Add Button, this changes the default color from White to Red which triggers mHost.Geometry_ChangeProperty call in the controller
        private void UpdateParticleRowValues(int rowIndex)
        {
            gridParticle[rowIndex, PARTICLE_COLUMN_COLOR].Value = (object)Keystone.Types.Color.Red;
        }

        private int mGroupIndex = -1;
       
        private void buttonAddEmitterKeyframe_Click(object sender, EventArgs e)
        {
            // TODO: we should initialize the row with the existing EmitterKeyFrame values first
            int rowIndex = AddAEmitterGridRow();
            UpdateEmitterRowValues(rowIndex);
        }

        private void buttonAddParticleKeyframe_Click(object sender, EventArgs e)
        {
            // TODO: we should initialize the row with the existing ParticleKeyframes values first

            int rowIndex = AddAParticleGridRow();
            UpdateParticleRowValues(rowIndex);
        }

        private void buttonRemoveEmitterKeyframe_Click(object sender, EventArgs e)
        {

        }

        private void buttonRefreshEmittterKeyframes_Click(object sender, EventArgs e)
        {

        }

        private void buttonMoveEmitterKeyframeUp_Click(object sender, EventArgs e)
        {

        }

        private void buttonMoveEmitterKeyframeDown_Click(object sender, EventArgs e)
        {

        }




        private void buttonRemoveParticleKeyframe_Click(object sender, EventArgs e)
        {

        }

        private void buttonRefreshParticleKeyframes_Click(object sender, EventArgs e)
        {

        }

        private void buttonMoveParticleKeyframeUp_Click(object sender, EventArgs e)
        {

        }

        private void buttonMoveParticleKeyframeDown_Click(object sender, EventArgs e)
        {

        }

        // particle
        private void menuParticleUseageChanged(object sender, EventArgs e)
        {
            // update the useage and send it to the host

            int useage = 0;
            int i = 0;
            foreach (ToolStripMenuItem item in toolStripMenuParticleUseage.DropDown.Items)
            {
                int flag = 1 << i;
               
                if (item.Checked)
                    useage |= flag;
                else
                    useage &= ~flag;
                i++;
            }
            
            int emitterIndex = 0; // todo: this needs to be dynam8ic
            if (!mHost.PluginChangesSuspended )
                mHost.Geometry_ChangeGroupProperty(mParticleSystemID, emitterIndex, "particlekeyuseage", typeof(int).Name, useage);
        }


        // emitter
        private void menuEmitterUseageChanged(object sender, EventArgs e)
        {
            // update the useage and send it to the host
            int useage = 0;
            int i = 0;
            foreach (ToolStripMenuItem item in toolStripMenuEmitterUseage.DropDown.Items)
            {
                int flag = 1 << i;
               
                if (item.Checked)
                    useage |= flag;
                else
                    useage &= ~flag;
                i++;
            }

            int emitterIndex = 0; // todo: this needs to be dynam8ic
            if (!mHost.PluginChangesSuspended)
                mHost.Geometry_ChangeGroupProperty(mParticleSystemID, emitterIndex, "emitterkeyuseage", typeof(int).Name, useage);
        }


        private void OnGridColor_Clicked(object sender, MouseEventArgs e)
        {
            PictureBox pbox = (PictureBox)sender;
            SourceGrid.Cells.Cell cell = (SourceGrid.Cells.Cell)pbox.Tag;

            Point screen = pbox.PointToScreen(e.Location);
            Point parentSpace = superTabControlPanel11.Parent.Parent.PointToClient(screen);

            KeyPluginEntityEdit.ColorPickerDialog dialog = new KeyPluginEntityEdit.ColorPickerDialog();
            dialog.Left = screen.X;
            dialog.Top = screen.Y;

            DialogResult result = dialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                Keystone.Types.Color c = new Keystone.Types.Color(dialog.SelectedColor.R, dialog.SelectedColor.G, dialog.SelectedColor.B, dialog.SelectedColor.A);
                if (cell.Grid == gridEmitter)
                    cell.Grid[cell.Row.Index, EMITTER_COLUMN_COLOR].Value = c;
                else
                    cell.Grid[cell.Row.Index, PARTICLE_COLUMN_COLOR].Value = c;

                //gridEmitter[cell.Row.Index, EMITTER_COLUMN_COLOR].Value = c;
            }
        }

        private void OnGridColor_Resized(object sender, EventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            pictureBox.BackColor = System.Drawing.Color.Red;
            Graphics g = pictureBox.CreateGraphics();
            System.Drawing.Rectangle innerRect = new Rectangle(3, 3, pictureBox.Width - 6, pictureBox.Height - 6);
            g.DrawRectangle(_pen, innerRect);
            g.Dispose();

            SourceGrid.Cells.Cell c = (SourceGrid.Cells.Cell)pictureBox.Tag;
        }

        Pen _pen = Pens.Black;
        private void OnGridColor_Paint(object sender, PaintEventArgs e)
        {
            PictureBox pictureBox = (PictureBox)sender;
            SourceGrid.Cells.Cell c = (SourceGrid.Cells.Cell)pictureBox.Tag;
            Keystone.Types.Color value = (Keystone.Types.Color)c.Value;
            System.Drawing.Color color = System.Drawing.Color.FromArgb(value.ToInt32());


            pictureBox.BackColor = System.Drawing.Color.White;

            System.Drawing.Rectangle innerRect = new Rectangle(3, 3, pictureBox.Width - 6, pictureBox.Height - 6);

            //Draw the outer rectangle with the color of _pen

            e.Graphics.DrawRectangle(_pen, innerRect);

            using (Brush brush = new SolidBrush(color))
            {
                //Fill the rectangle with the color of _brush

                e.Graphics.FillRectangle(brush, innerRect);
            }
        }

        private void ClearEmitterAndParticleGrids()
        {
            if (mEmitterGridInitialized)
            {
                if (gridEmitter.RowsCount > 1) // first row is header
                    for (int i = 1; i < gridEmitter.RowsCount; i++)
                        for (int j = 0; j < EMITTER_KEYFRAMES_NUM_COLUMNS; j++)
                            gridEmitter[i, j] = null;

                if (gridEmitter.RowsCount > 1) // first row is header
                    // delete all rows from bottom up but NOT the header which is row 0
                    for (int j = gridEmitter.RowsCount - 1; j >= 1; j--)
                        gridEmitter.Rows.Remove(j);
            }

            if (mParticleGridInitialized)
            {
                if (gridParticle.RowsCount > 1) // first row is header
                    for (int i = 1; i < gridParticle.RowsCount; i++)
                        for (int j = 0; j < PARTICLE_KEYFRAMES_NUM_COLUMNS; j++)
                            gridParticle[i, j] = null;

                if (gridParticle.RowsCount > 1) // first row is header
                    // delete all rows from bottom up but NOT the header which is row 0
                    for (int j = gridParticle.RowsCount - 1; j >= 1; j--)
                        gridParticle.Rows.Remove(j);
            }
        }

        private void ReleaseEmitterAndParticleGrids()
        {
            try
            {
                gridEmitter.Dispose();
                gridParticle.Dispose();
                mEmitterGridInitialized = false;
                mParticleGridInitialized = false;
            }
            catch { }
        }
        #endregion
    }
}