
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using KeyPlugins;
using Keystone.Types;

namespace KeyPlugins
{
    partial class BaseEntityPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appearanceID">Appearance Node ID or GroupAttribute ID</param>
        private void CreateAppearanceCards(string appearanceID)
        {
            ClearAppearanceCardsPanel(superTabControlPanel2);
        
            cboLightingMode.Items.Clear();
            cboLightingMode.Items.Add("NONE");
            cboLightingMode.Items.Add("NORMAL");
            cboLightingMode.Items.Add("MANAGED");
            cboLightingMode.Items.Add("BUMP_TANGENT_SPACE");
            cboLightingMode.Items.Add("BUMP_OFFSET");
            cboLightingMode.Items.Add("PER-VERTEX PRT");

            object lighting = mHost.Node_GetProperty(appearanceID, "lightingmode");

            // remove the event hanlder to prevent the SelectedValueChanged even from firing when we are just
            // iniitializing the combobox
            this.cboLightingMode.SelectedValueChanged -= new System.EventHandler(this.cboLightingMode_SelectedIndexChanged);

            if (lighting != null)
            {
                // for particle emitters, there is no lightingmode 
                int currentLightingMode = (int)lighting;
                string selected = GetSimplifiedLightingModeName(currentLightingMode);
                cboLightingMode.SelectedIndex = currentLightingMode;
            }
            this.cboLightingMode.SelectedValueChanged += new System.EventHandler(this.cboLightingMode_SelectedIndexChanged);

            string[] childIDs;
            string[] childTypes;

            mHost.Node_GetChildrenInfo(appearanceID, null, out childIDs, out childTypes);

            if (childIDs != null && childIDs.Length > 0)
            {
                for (int i = 0; i < childIDs.Length; i++)
                {
                    string nodeType = childTypes[i];
                    string nodeID = childIDs[i];

                    switch (nodeType)
                    {
                        // TODO: When loading a .X, .TVA, .TVM we need to convert
                        // the material to use the shader effect params.
                        // The cool part is since we will always load a default shader
                        // when importing which mimics tv's shader, we can directly
                        // assign the material values to those in the shader.

                        // ok, so no more Material node, but still Texture nodes...
                        // how does that work?  The Texture layer has to be set on the geometry
                        // because we'll be using the layers semantics... so that means
                        // appearance must update the texture on the mesh3d for shared meshes
                        // that are shared geometry but different appearance.
                        // So how is that done?
                        // Actually same as always. The only thing different is whenever
                        // a shader is loaded, we determine if all textures added match up
                        // with a semantic in the shader and if not, we remove those textures.
                        case "Material":
                            MaterialAttributeCard matEdit = new MaterialAttributeCard(null, nodeID, appearanceID, BrowseReplacementResource, EditResource);
                            //matEdit.Text = nodeType;

                            matEdit.Ambient = (Keystone.Types.Color)mHost.Node_GetProperty(nodeID, "ambient");
                            matEdit.Diffuse = (Keystone.Types.Color)mHost.Node_GetProperty(nodeID, "diffuse");
                            matEdit.Emissive = (Keystone.Types.Color)mHost.Node_GetProperty(nodeID, "emissive");
                            matEdit.Specular = (Keystone.Types.Color)mHost.Node_GetProperty(nodeID, "specular");
                            
                            // TODO: I must re-arrange this so opacity and specular arent always 
                            //       too difficult to access
                            matEdit.Opacity = (byte)(255 * (float)mHost.Node_GetProperty(nodeID, "opacity"));
                            matEdit.SpecularPower = (float)mHost.Node_GetProperty(nodeID, "power");

                            // assign event handlers
                            matEdit.AmbientChanged += OnAmbientChanged;
                            matEdit.DiffuseChanged += OnDiffuseChanged;
                            matEdit.EmissiveChanged += OnEmissiveChanged;
                            matEdit.SpecularChanged += OnSpecularChanged;
                            matEdit.OpacityChanged += OnOpacityChanged;
                            matEdit.SpecularPowerChanged += OnSpecPowerChanged;
                            matEdit.ControlClosed += OnMaterialRemoved;

                            superTabControlPanel2.Controls.Add(matEdit);
                            break;
                        case "ProceduralShader":
                        case "Shader":
                            string descriptor = (string)mHost.Node_GetProperty(nodeID, "shaderpath");
                            ShaderEditCard shaderCard = new ShaderEditCard(descriptor, nodeID, appearanceID, BrowseReplacementResource, EditResource);
                            shaderCard.Text = nodeType;
                            // populate the parameters to the shader card's grid
                            int parameterCount = (int)mHost.Node_GetProperty(nodeID, "parmatercount");

                            // TODO: techniques, passes, can be assigned here.  They are stored in the
                            // non shareable AppearanceGroup node.  Furthermore
                            // maybe here in the AppearanceNode is where we set more detailed defines
                            // such as parallax, and such?  
                            
                            // similar to domain object we can grab all custom shader parameters
                            Settings.PropertySpec[] shaderparams = mHost.Appearance_GetShaderParameters(appearanceID);
                            shaderCard.SetGridProperties(shaderparams); // ok if result is null
                            

                            //if (parameters != null)
                            //    for (int j = 0; j < parameters.Length; j++)
                            //    {
                            //        // add an edit control
                            //        switch (parameters[j].TypeName.ToUpper())
                            //        {
                            //            case "TEXTURE0":
                            //            case "TEXTURE1":
                            //                break;
                            //            case "VECTOR3D":
                            //                break;
                            //            case "MATRIX":
                            //                break;
                            //            case "SINGLE":
                            //                break;
                            //            case "INT32":
                            //                break;
                            //            default:
                            //                break;
                            //        }
                            //    }
                            

                            // assign event handlers
                            shaderCard.OnResourceChanged += OnShaderChanged;  // resource itself
                            shaderCard.ControlClosed += OnShaderRemoved;
                            shaderCard.OnPropertyValueChanged = OnShaderParameterChanged;

                            superTabControlPanel2.Controls.Add(shaderCard);
                            break;
                        // Remember that Diffuse..CubeMap are Layers, not Textures anymore.
                        // This was done so that Layers represent any transforms that should be applied to Texture resources
                        // and Textures themselves can be simple dumb resources fully shareable with no issues
                        case "TextureCycle":
                            // this doesn't use a typical texture edit card because you can't independantly 
                            // modify scale/translation/rotation of textures...  that would be for the texture cycle itself
                            // these are the raw textures... 
                            // furtehr, there are potentially dozens of textures in the cycle.... a grid makes more sense
                            // than cards... ugh.. 
                            break;
                        case "SplatAlpha":
                        case "Diffuse":
                        case "Specular":
                        case "NormalMap":
                        case "Emissive":
                        case "VolumeTexture":
                        case "DUDVTexture":
                        case "CubeMap":
                            
                            //case "Texture":
                            string resourceDescriptor = (string)mHost.Node_GetProperty(nodeID, "texture");
                            TextureEditCard texCard = new TextureEditCard(resourceDescriptor, nodeID, appearanceID, this.mModFolderPath, BrowseReplacementResource, EditResource);
                            //texCard.Text = nodeType;


                            // populate the properties
                            texCard.Scale = (Vector2f)mHost.Node_GetProperty(nodeID, "tscale");
                            texCard.Translation = (Vector2f)mHost.Node_GetProperty(nodeID, "toffset");
                            texCard.Rotation = (float)mHost.Node_GetProperty(nodeID, "trotation");

                            // assign event handlers
                            texCard.AlphaTestChanged += OnAlphaTestChanged ;
                            texCard.AlphaTestDepthWriteChanged += OnAlphaTestDepthWriteChanged;
                            texCard.AlphaTestRefValueChanged += OnAlphaTestReferenceValueChanged;
                            texCard.TextureChanged += OnTextureChanged;
                            texCard.TextureScaled += OnTextureScaled;
                            texCard.TextureTranslated += OnTextureTranslated;
                            texCard.TextureRotated += OnTextureRotated;
                            texCard.ControlClosed += OnTextureRemoved;

                            superTabControlPanel2.Controls.Add(texCard);
                            break;
                    }
                }

                
                AutoArrangePanelCards(superTabControlPanel2);
            }
        }

        /// <summary>
        /// To avoid "Error creating window handle" error, our Notecards must be properly
        /// removed.
        /// </summary>
        /// <param name="panel"></param>
        protected void ClearAppearanceCardsPanel(DevComponents.DotNetBar.SuperTabControlPanel panel)
        {
            if (panel.Controls != null && panel.Controls.Count > 0)
            {
                foreach (Control c in panel.Controls)
                {
                    if (c is NotecardBase == false) continue;
                    c.Dispose(); // override dispose to remove events?
                    c.Visible = false;
                }
            }
            //panel.Controls.Clear();
        }

        private void PopulateOverallAppearanceMenu(ContextMenuStrip menu, string appearanceGroupID, string parentNodeID, Point location)
        {
            PopulateGroupAppearanceMenu(menu, appearanceGroupID, parentNodeID, location);

            menu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem menuItem = new ToolStripMenuItem("Delete");
            menuItem.Name = appearanceGroupID;
            menuItem.Tag = parentNodeID;
            menuItem.Click += new EventHandler(Delete_Click); // Delete_Click() is protected function in BasePluginCtl.cs
            menu.Items.Add(menuItem);

            menu.Show(this.treeModel, location);
        }


        private void PopulateGroupAppearanceMenu(ContextMenuStrip menu, string appearanceGroupID, string parentNodeID, Point location)
        {
            // appearanceGroupID is either GroupAttribute or the overall DefaultAppearance 
            // Add Material <-- grey out if there already exists a material

            ToolStripMenuItem menuItem = new ToolStripMenuItem("Add Material");
            menuItem.Name = appearanceGroupID;
            menuItem.Tag = parentNodeID;
            menuItem.Click += new EventHandler(menuAddMaterial_Click);
            menu.Items.Add(menuItem);

            menu.Items.Add(new ToolStripSeparator());

            menuItem = new ToolStripMenuItem("Add Diffuse");
            menuItem.Name = appearanceGroupID;
            menuItem.Tag = parentNodeID;
            menuItem.Click += new EventHandler(menuAddDiffuse_Click);
            menu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("Add Normal Map");
            menuItem.Name = appearanceGroupID;
            menuItem.Tag = parentNodeID;
            menuItem.Click += new EventHandler(menuAddNormalMap_Click);
            menu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("Add Specular Map");
            menuItem.Name = appearanceGroupID;
            menuItem.Tag = parentNodeID;
            menuItem.Click += new EventHandler(menuAddSpecularMap_Click);
            menu.Items.Add(menuItem);

            menuItem = new ToolStripMenuItem("Add Emissive Map");
            menuItem.Name = appearanceGroupID;
            menuItem.Tag = parentNodeID;
            menuItem.Click += new EventHandler(menuAddEmissiveMap_Click);
            menu.Items.Add(menuItem);

            menu.Items.Add(new ToolStripSeparator());
            
            menuItem = new ToolStripMenuItem("Add Texture Cycle");
            menuItem.Name = appearanceGroupID;
            menuItem.Tag = parentNodeID;
            menuItem.Click += new EventHandler(menuAddTextureCycle_Click);
            menu.Items.Add(menuItem);

            menu.Items.Add(new ToolStripSeparator());

            menuItem = new ToolStripMenuItem("Add Shader");
            menuItem.Name = appearanceGroupID;
            menuItem.Tag = parentNodeID;
            menuItem.Click += new EventHandler(menuAddShader_Click);
            menu.Items.Add(menuItem);
        }

        private void menuAddMaterial_Click(object sender, EventArgs e)
        {
            string materialID = null;
            DialogResult result = KeyPlugins.StaticControls.InputBox("Enter name of Material:", "enter new material name", ref materialID);

            if (string.IsNullOrEmpty(materialID)) return;

            CreateNode ("Material", materialID, ((ToolStripMenuItem)sender).Name.ToString());
        }

        private void menuAddDiffuse_Click(object sender, EventArgs e)
        {
            CreateNode("Diffuse", ((ToolStripMenuItem)sender).Name.ToString());
        }

        private void menuAddNormalMap_Click(object sender, EventArgs e)
        {
            CreateNode("NormalMap", ((ToolStripMenuItem)sender).Name.ToString());
        }

        private void menuAddSpecularMap_Click(object sender, EventArgs e)
        {
            CreateNode("Specular", ((ToolStripMenuItem)sender).Name.ToString());
        }

        private void menuAddEmissiveMap_Click(object sender, EventArgs e)
        {
            CreateNode("Emissive", ((ToolStripMenuItem)sender).Name.ToString());
        }

        private void menuAddTextureCycle_Click(object sender, EventArgs e)
        {
            CreateNode("TextureCycle", ((ToolStripMenuItem)sender).Name.ToString());
        }

        /// <summary>
        /// Pops up the Asset Browser and allows user to select a shader. 
        /// Can also first create a new shader.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void menuAddShader_Click(object sender, EventArgs e)
        {
            string fileDialogFilter = "Shaders|*.fx";
            BrowseNewResource("ProceduralShader", ((ToolStripMenuItem)sender).Name.ToString(), fileDialogFilter);
        }

        #region ShaderEditCard
        // user can drag drop script to change, as well as edit the contents
        // and check for change and then reload it
        private void OnShaderChanged(object sender, EventArgs e)
        {
            if (e is ResourceEventArgs)
            {
                // TODO: disallow this.  No more drag and drop onto existing
                // because it would require us in reality to delete the first shader
                // and replace it.  
                // Because a Shader cannot have it's _id field changed because it's 
                // a resource id.
                ResourceEventArgs targs = (ResourceEventArgs)e;
                mHost.Node_ReplaceResource(targs.ChildID, (string)targs.Value, "Generic", targs.ParentID);
            }
        }

        private void OnShaderRemoved(object sender, EventArgs e)
        {
            if (sender is ShaderEditCard)
            {
                ShaderEditCard card = (ShaderEditCard)sender;

                foreach (Control c in superTabControlPanel2.Controls)
                {
                    if (c == card)
                    {
                        card.ControlClosed -= OnShaderRemoved;

                        mHost.Node_Remove(card.ShaderID, card.ParentID);

                        superTabControlPanel2.Controls.Remove(c);
                        c.Dispose(); // override dispose to remove events?
                        break;
                    }
                }
            }
        }

        private void OnShaderParameterChanged(string nodeID, Settings.PropertySpec spec)
        {
            if (mHost != null)
                mHost.Appearance_ChangeShaderParameterValue(nodeID, spec.Name, spec.TypeName, spec.DefaultValue);
        }

        //private void OnShaderParameterChanged(object sender, EventArgs e)
        //{

        //    if (sender is ShaderEditCard)
        //    {
        //        ShaderEditCard card = sender as ShaderEditCard;
        //        Settings.PropertySpecEventArgs args = (Settings.PropertySpecEventArgs)e;

        //        if (mHost != null)
        //            mHost.Appearance_ChangeShaderParameterValue(card.AppearanceID, args.Property.Name, args.Property.TypeName, args.Value);
        //    }
        //}
        #endregion

        #region MaterialEditCard
        private void OnAmbientChanged(object sender, EventArgs e)
        {
            if (e == null) return;
            if (sender is MaterialAttributeCard)
            {
                string nodeID = ((MaterialAttributeCard)sender).TargetID;
                Keystone.Types.Color color = ((MaterialAttributeCard)sender).Ambient;
                mHost.Node_ChangeProperty(nodeID, "ambient", typeof(Keystone.Types.Color), color);
            }

        }

        private void OnDiffuseChanged(object sender, EventArgs e)
        {
            if (e == null) return;
            if (sender is MaterialAttributeCard)
            {
                string nodeID = ((MaterialAttributeCard)sender).TargetID;
                Keystone.Types.Color color = ((MaterialAttributeCard)sender).Diffuse;
                mHost.Node_ChangeProperty(nodeID, "diffuse", typeof(Keystone.Types.Color), color);
            }
        }

        private void OnEmissiveChanged(object sender, EventArgs e)
        {
            if (e == null) return;
            if (sender is MaterialAttributeCard)
            {
                string nodeID = ((MaterialAttributeCard)sender).TargetID;
                Keystone.Types.Color color = ((MaterialAttributeCard)sender).Emissive;
                mHost.Node_ChangeProperty(nodeID, "emissive", typeof(Keystone.Types.Color), color);
            }
        }

        private void OnSpecularChanged(object sender, EventArgs e)
        {
            if (e == null) return;
            if (sender is MaterialAttributeCard)
            {
                string nodeID = ((MaterialAttributeCard)sender).TargetID;
                Keystone.Types.Color color = ((MaterialAttributeCard)sender).Specular;
                mHost.Node_ChangeProperty(nodeID, "specular", typeof(Keystone.Types.Color), color);
            }
        }

        private void OnOpacityChanged(object sender, EventArgs e)
        {
            if (e == null) return;
            if (sender is MaterialAttributeCard)
            {
                string nodeID = ((MaterialAttributeCard)sender).TargetID;
                float opacity = ((MaterialAttributeCard)sender).Opacity / 255f;
                mHost.Node_ChangeProperty(nodeID, "opacity", typeof(float), opacity);
            }
        }

        private void OnSpecPowerChanged(object sender, EventArgs e)
        {
            if (e == null) return;
            if (sender is MaterialAttributeCard)
            {
                string nodeID = ((MaterialAttributeCard)sender).TargetID;
                float power = ((MaterialAttributeCard)sender).SpecularPower;
                mHost.Node_ChangeProperty(nodeID, "power", typeof(float), power);
            }
        }

        private void OnMaterialRemoved(object sender, EventArgs e)
        {
            if (sender is MaterialAttributeCard)
            {
                MaterialAttributeCard card = (MaterialAttributeCard)sender;

                foreach (Control c in superTabControlPanel2.Controls)
                {
                    if (c == card)
                    {
                        card.AmbientChanged -= OnAmbientChanged;
                        card.DiffuseChanged -= OnDiffuseChanged;
                        card.EmissiveChanged -= OnEmissiveChanged;
                        card.SpecularChanged -= OnSpecularChanged;
                        card.OpacityChanged -= OnOpacityChanged;
                        card.SpecularPowerChanged -= OnSpecPowerChanged;
                        card.ControlClosed -= OnMaterialRemoved;

                        mHost.Node_Remove(card.TargetID, card.ParentID);

                        superTabControlPanel2.Controls.Remove(c);
                        c.Dispose(); // override dispose to remove events?
                        break;
                    }
                }
            }
        }
        #endregion

        #region TextureEditCard
        private void OnAlphaTestChanged (object sender, EventArgs e)
        {
            if (e == null) return;
            if (e is ResourceEventArgs)
            {
                ResourceEventArgs targs = (ResourceEventArgs)e;
                mHost.Node_ChangeProperty(targs.ParentID, "alphatest", typeof(bool), targs.Value);
            }
        }
        
        private void OnAlphaTestReferenceValueChanged (object sender, EventArgs e)
        {
        }
        
        private void OnAlphaTestDepthWriteChanged (object sender, EventArgs e)
        {
        }
                
        private void OnTextureChanged(object sender, EventArgs e)
        {
            // if a new texture has been dragged onto an existing texture edit card's picturebox
            // change the texture
            if (e is ResourceEventArgs)
            {
                ResourceEventArgs targs = (ResourceEventArgs)e;

                // Notify the PluginHost that we wish to change the underlying TEXTURE RESOURCE
                // for the current entity's geometry group/layer

                // NOTE: Since the groupAttribute or Appearance doesnt' change, neither will
                // any previous scaling/texturemods
                mHost.Node_ReplaceResource(targs.ChildID, (string)targs.Value, "Texture", targs.ParentID);
            }
        }

        private void OnTextureScaled(object sender, EventArgs e)
        {
            if (e == null) return;
            if (e is ResourceEventArgs)
            {
                ResourceEventArgs targs = (ResourceEventArgs)e;
                mHost.Node_ChangeProperty(targs.ParentID, "tscale", typeof(Vector2f), targs.Value);
            }
        }

        private void OnTextureTranslated(object sender, EventArgs e)
        {
            if (e == null) return;
            if (e is ResourceEventArgs)
            {
                ResourceEventArgs targs = (ResourceEventArgs)e;
                mHost.Node_ChangeProperty(targs.ParentID, "toffset", typeof(Vector2f), targs.Value);

            }
        }

        private void OnTextureRotated(object sender, EventArgs e)
        {
            if (e == null) return;
            if (e is ResourceEventArgs)
            {
                ResourceEventArgs targs = (ResourceEventArgs)e;
                mHost.Node_ChangeProperty(targs.ParentID, "trotation", typeof(float), targs.Value);
            }
        }

        private void OnTextureRemoved(object sender, EventArgs e)
        {
            if (sender is TextureEditCard)
            {
                TextureEditCard card = (TextureEditCard)sender;

                foreach (Control c in superTabControlPanel2.Controls)
                {
                    if (c == card)
                    {
                        card.TextureChanged -= OnTextureChanged;
                        card.TextureScaled -= OnTextureScaled;
                        card.TextureTranslated -= OnTextureTranslated;
                        card.TextureRotated -= OnTextureRotated;
                        card.ControlClosed -= OnTextureRemoved;

                        mHost.Node_Remove(card.LayerID, card.ParentID);

                        superTabControlPanel2.Controls.Remove(c);
                        c.Dispose(); // override dispose to remove events?
                        break;
                    }
                }
            }
        }
        #endregion

    }
}
