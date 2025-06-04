using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Drawing.Drawing2D;

using MindFusion.Drawing;
using MindFusion.Diagramming;
using MindFusion.Diagramming.WinForms;
using LinearGradientBrush = MindFusion.Drawing.LinearGradientBrush;

namespace KeyEdit.Controls
{
    public partial class LibNoiseTextureDesigner : UserControl
    {
        private struct Link
        {
            public int Origin;
            public int OriginRow;
            public int OriginAnchor;
            public int Dest;
            public int DestRow;
            public int DestAnchor;
        }

        // all of this fits in the tag property of an anchorpoint
        public struct Parameter
        {
            public bool IsInputParameter;
            public int RowID;
            public string Name;
            public string DataType;
            public string Tooltip;
            public string Description;
            public bool AssignmentRequired;
            public int Link;
            public bool EditInPlace;
            public object Value; // only set if EditInPlace = true;
            public bool IsRanged;
            public object MinValue;
            public object MaxValue;

            public Parameter(string name, string dataType, bool isInputParameter)
                : this(name, dataType, isInputParameter, false)
            {
            }

            public Parameter(string name, string dataType, bool isInputParameter, bool assignmentRequired)
            {
                IsInputParameter = isInputParameter;
                RowID = -1;
                Name = name;
                DataType = dataType;
                Tooltip = "";
                Description = "";

                AssignmentRequired = assignmentRequired;
                Link = -1;

                EditInPlace = false;
                Value = "";
                IsRanged = false;
                MinValue = "";
                MaxValue = "";
            }

            public Parameter(int link, int rowID, string fromString)
            {
                RowID = rowID;
                Link = link;
                string[] split = fromString.Split(',');

               
                Name = split[0];
                DataType = split[1];
                IsInputParameter = bool.Parse (split[2]);
                Tooltip = split[3];
                Description = split[4];
                AssignmentRequired = bool.Parse (split[5]);

                EditInPlace = bool.Parse (split[6]);
                
                
                if (EditInPlace)
                {
                    switch (DataType)
                    {
                        case "int":
                            Value = int.Parse(split[7]);
                            break;

                        case "float":
                            Value = float.Parse(split[7]);
                            break;

                        default:
                            throw new Exception("Datatype '" + DataType + "' not yet supported.");
                    }
                }
                else
                {
                    Value = "";
                }

                IsRanged = bool.Parse(split[8]);
                if (IsRanged )
                {
                    switch (DataType)
                    {
                        case "int":
                            MinValue = int.Parse(split[9]);
                            MaxValue = int.Parse(split[10]);
                            break;

                        case "float":
                            MinValue = float.Parse(split[9]);
                            MaxValue = float.Parse(split[10]);
                            break;

                        default:
                            throw new Exception("Datatype '" + DataType + "' not yet supported.");
                    }
                }
                else
                {
                    MinValue = "";
                    MaxValue = "";
                }

            }

            public override string  ToString()
            {
                return string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}", 
                    Name, DataType , IsInputParameter , Tooltip , Description, AssignmentRequired,  
                    EditInPlace , Value, IsRanged , MinValue, MaxValue);
            }
       }


        public LibNoiseTextureDesigner(string name)
        {
            InitializeComponent();
            Name = name;
            diagramView1.AllowInplaceEdit = true;
            diagramView.AllowUnanchoredLinks = false;
            diagramView.AllowLinksRepeat = false;
            diagramView.AllowSelfLoops = false;
            diagramView.ShowAnchors = MindFusion.Diagramming.ShowAnchors.Never;

            pictureBox.Image = Image.FromFile(System.IO.Path.Combine (AppMain.MOD_PATH, @"caesar\shaders\Planet\planet.jpg"));

            RatioStretch();
            Init();
        }

        private void Init()
        {
            //diagramView.ActionRecorded +=
            //    diagramView.ActionRecording += 
            //    diagramView.ActionRedone += 
            //    diagramView.ActionUndone += 
            //    diagramView.BeginLoad +=
            //    diagramView.BoundsChanged += 
            //    diagramView.CellClicked += 
            //    diagramView.CellDoubleClicked +=
            diagramView.CellTextEdited += OnCellTextEdited;
            diagramView.CellTextEditing += OnCellTextEditing;
            //    diagramView.Clicked += 
            //    diagramView.ContainerChildAdded += 
            //    diagramView.ContainerChildRemoved +=
            //    diagramView.ContainerFolded += 
            //    diagramView.ContainerUnfolded += 
            //    diagramView.DefaultShapeChanged +=
            //    diagramView.DeserializeTag += 
            //    diagramView.DestinationAnchorChanged += 
            //    diagramView.DirtyChanged +=
            //    diagramView.Disposed +=
            //    diagramView.DoubleClicked += 
            //    diagramView.DrawAdjustmentHandles +=
            //    diagramView.DrawAnchorPoint +=
            //    diagramView.DrawBackground +=
            //    diagramView.DrawCell += 
            //    diagramView.DrawForeground +=
            //    diagramView.DrawLink +=
            //    diagramView.DrawNode +=
            //    diagramView.EndLoad +=
            //    diagramView.ExpandButtonClicked+=
            //    diagramView.GroupDestroyed +=
            //    diagramView.HitTestAdjustmentHandles +=
            //    diagramView.InitializeLink +=
            //    diagramView.InitializeNode +=
            //    diagramView.ItemAdded +=
            //    diagramView.ItemRemoved +=
            //    diagramView.LinkActivated +=
            //    diagramView.LinkClicked +=
            //    diagramView.LinkCreateCancelled +=
            diagramView.LinkCreated += OnLinkCreated;
            diagramView.LinkCreating += OnLinkCreating;
            //    diagramView.LinkDeactivated +=
            //    diagramView.LinkDeleted +=
            //    diagramView.LinkDeleting +=
            //    diagramView.LinkDeselected +=
            //    diagramView.LinkDoubleClicked +=
            //    diagramView.LinkModified += OnLinkModified;
            //    diagramView.LinkModifyCancelled += ;
            diagramView.LinkModifying += OnLinkModifying;
            //    diagramView.LinkPasted +=
            //    diagramView.LinkRouted +=
            //    diagramView.LinkSelected +=
            //    diagramView.LinkSelecting +=
            //diagramView.LinkStartModifying += OnLinkModified;
            //    diagramView.LinkTextEdited +=
            //    diagramView.LinkTextEditing +=
            //    diagramView.MeasureUnitChanged +=
            //    diagramView.NodeActivated +=
            //    diagramView.NodeClicked +=
            //    diagramView.NodeCreateCancelled +=
            //    diagramView.NodeCreated +=
            //    diagramView.NodeCreating +=
            //    diagramView.NodeDeactivated +=
            //    diagramView.NodeDeleted +=
            //    diagramView.NodeDeleting +=
            //    diagramView.NodeDeselected +=
            //    diagramView.NodeDoubleClicked +=
            //    diagramView.NodeModified += 
            //    diagramView.NodeModifyCancelled +=
            //    diagramView.NodeModifying +=
            //    diagramView.NodePasted +=
            //    diagramView.NodeSelected +=
            //    diagramView.NodeSelecting +=
            //    diagramView.NodeStartModifying +=
            //    diagramView.NodeTextEdited +=
            //    diagramView.NodeTextEditing +=
            //    diagramView.OriginAnchorChanged +=
            //    diagramView.Repaint +=
            //    diagramView.SelectionChanged +=
            //    diagramView.SelectionMoved +=
            //    diagramView.SelectionMoving +=
            //    diagramView.SerializeTag +=
            //    diagramView.TableSectionCollapsed +=
            //    diagramView.TableSectionExpanded +=
            //    diagramView.TreeCollapsed +=
            //    diagramView.TreeExpanded +=
            //    diagramView.TreeItemTextEditing +=
            diagramView.ValidateAnchorPoint += OnValidateAnchorPoint;
            //    diagramView.ViewRemoved += 



        }

        //http://www.codeproject.com/KB/miscctrl/ratiostretchpicturebox1.aspx?fid=15270&fr=1#xx0xx
        // in this code, the author has his custom control inherited from a scrollablecontrol and so he gets the scrolling automatically
        // i could perhaps use a panel and place the picturebox onto it, or just implementt he custom control as he does.
        // 
        private void RatioStretch()
        {
            float pRatio = (float)this.splitContainer.Panel2.Width / this.splitContainer.Panel2.Height;
            float imRatio = (float)this.pictureBox.Image.Width / this.pictureBox.Image.Height;

            if (this.splitContainer.Panel2.Width >= this.pictureBox.Image.Width && this.splitContainer.Panel2.Height >= this.pictureBox.Image.Height)
            {
                this.pictureBox.Width = this.pictureBox.Image.Width;
                this.pictureBox.Height = this.pictureBox.Image.Height;
            }
            else if (this.splitContainer.Panel2.Width > this.pictureBox.Image.Width && this.splitContainer.Panel2.Height < this.pictureBox.Image.Height)
            {
                this.pictureBox.Height = this.splitContainer.Panel2.Height;
                this.pictureBox.Width = (int)(this.splitContainer.Panel2.Height * imRatio);
            }
            else if (this.splitContainer.Panel2.Width < this.pictureBox.Image.Width && this.splitContainer.Panel2.Height > this.pictureBox.Image.Height)
            {
                this.pictureBox.Width = this.splitContainer.Panel2.Width;
                this.pictureBox.Height = (int)(this.splitContainer.Panel2.Width / imRatio);
            }
            else if (this.splitContainer.Panel2.Width < this.pictureBox.Image.Width && this.splitContainer.Panel2.Height < this.pictureBox.Image.Height)
            {
                if (this.splitContainer.Panel2.Width >= this.splitContainer.Panel2.Height)
                {
                    //width image
                    if (this.pictureBox.Image.Width >= this.pictureBox.Image.Height && imRatio >= pRatio)
                    {
                        this.pictureBox.Width = this.splitContainer.Panel2.Width;
                        this.pictureBox.Height = (int)(this.splitContainer.Panel2.Width / imRatio);
                    }
                    else
                    {
                        this.pictureBox.Height = this.splitContainer.Panel2.Height;
                        this.pictureBox.Width = (int)(this.splitContainer.Panel2.Height * imRatio);
                    }
                }
                else
                {
                    //width image
                    if (this.pictureBox.Image.Width < this.pictureBox.Image.Height && imRatio < pRatio)
                    {
                        this.pictureBox.Height = this.splitContainer.Panel2.Height;
                        this.pictureBox.Width = (int)(this.splitContainer.Panel2.Height * imRatio);
                    }
                    else // height image
                    {
                        this.pictureBox.Width = this.splitContainer.Panel2.Width;
                        this.pictureBox.Height = (int)(this.splitContainer.Panel2.Width / imRatio);
                    }
                }
            }
            this.CenterImage();
        }

        private void CenterImage()
        {
            int top = (int)((this.splitContainer.Panel2.Height  - this.pictureBox.Height) / 2.0);
            int left = (int)((this.splitContainer.Panel2.Width  - this.pictureBox.Width) / 2.0);
            if (top < 0)
                top = 0;
            if (left < 0)
                left = 0;
          
            this.pictureBox.Top = top;
            this.pictureBox.Left = left;
        }

        public void CreateValueTypeNode(string valueTypeName, string valueTypeDescription, string[] labels, string[] values, string[] valueType)
        {
            // labels, values and valueTypes must all be same length
            if (labels.Length != values.Length) throw new ArgumentOutOfRangeException();
            if (values.Length != valueType.Length) throw new ArgumentOutOfRangeException();


        }

        public void CreateValueTypeNode(string valueTypeName, string valueTypeDescription, Parameter[] parameters)
        {
                    // create a table
            int rowCount = parameters.Length;
            int columnCount = 2;

            float rh = diagramView.TableRowHeight;
            float ch = diagramView.TableCaptionHeight;
            float totalh = ch + (rh * rowCount);
            float rowWidth = columnCount * 15;
            TableNode t = new TableNode(diagramView);
            t.Bounds = new RectangleF(0, 0, rowWidth, totalh + .1f);//(1, 1, rowWidth, totalh);
            t.Caption = valueTypeName;
            t.Tag = valueTypeDescription;
            diagramView.Nodes.Add(t);

           
            // set table properties
            t.RowCount = rowCount;
            t.ColumnCount = columnCount;

            t.Scrollable = false;
            
            t.ConnectionStyle = TableConnectionStyle.Rows;
            t.EnabledHandles = AdjustmentHandles.Move;
            t.CellFrameStyle = CellFrameStyle.Simple ;
            t.HandlesStyle = HandlesStyle.DashFrame;
            t.Columns[0].Width = rowWidth / 2;
            t.Columns[1].Width = rowWidth / 2;

            t.Style = TableStyle.Rectangle;
            t.CaptionColor = Color.White;

            // set connection points
            AnchorPoint ptout = new AnchorPoint(100, 50, false, true, Color.Red, 1);
            ArrayList al = new ArrayList();

            for (int i = 0; i < parameters.Length; i++)
            {
                al.Clear();
                if (!(String.IsNullOrEmpty (parameters[i].Name )))
                {
                    t[0, i].Text = parameters[i].Name;
                    //t[0,i].ToolTip =
                    //t[0,i].Tag = // the reason i'd rather use the anchorPoint.Tag and not this cell's tag is because the it's just annoying to have to read the cell's xml for the tag
                }

                if ( parameters[i].EditInPlace )
                {
                    t[1, i].Text = parameters[i].Value.ToString ();
                    //t[0,i].ToolTip =
                    //t[0,i].Tag = 
                    t[1, i].TextFormat.Alignment = StringAlignment.Far;
                    AnchorPoint anchorOut = (AnchorPoint)ptout.Clone();
                    anchorOut.Tag = parameters[i].ToString();
                    al.Add(anchorOut);
                }

                // note: the input and output anchor points are added to the same row at the same time
                t.Rows[i].AnchorPattern = new AnchorPattern(
                    (AnchorPoint[])al.ToArray(typeof(AnchorPoint)));
            }
        }


        public void CreateEntityNode(string entityName, string entityDescription, string[] inputs, string[] inputType, bool[] inputRequired, string[] outputs, string[] outputType)
        {
            if (inputType.Length != inputs.Length) throw new ArgumentOutOfRangeException();
            if (outputType.Length != outputs.Length) throw new ArgumentOutOfRangeException();

            Parameter[] inParameters = new Parameter [inputs.Length];
            for (int i = 0; i < inputs.Length; i ++)
            {
                inParameters[i] = new Parameter(inputs[i], inputType[i], true,inputRequired[i]);
            }

            Parameter[] outParameters = new Parameter[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
            {
                outParameters[i] = new Parameter(outputs[i],outputType[i] , false);
            }
           
            CreateEntityNode(entityName , entityDescription , inParameters , outParameters );
        }

        public void CreateEntityNode(string entityName, string entityDescription, Parameter[] inputs, Parameter[] outputs)
        {
            // create a table
            int rowCount = Math.Max(inputs.Length, outputs.Length); // 3;

            float rh = diagramView.TableRowHeight;
            float th = diagramView.TableCaptionHeight;
            float totalh = th + (rowCount + 1) * rh; // +1 for the info and help button rows
            float rowWidth = 50 + 2 * rh;
            TableNode t = new TableNode(diagramView);
            t.Bounds = new RectangleF(5, 5, rowWidth, totalh);
            t.Caption = entityName;
            t.Tag = entityDescription;
            diagramView.Nodes.Add(t);

     
            // set table properties
            LinearGradientBrush tbrush = new
                LinearGradientBrush(Color.LightBlue, Color.LightBlue, 90);

            ColorBlend blend = new ColorBlend(4);
            blend.Colors[0] = Color.LightBlue;
            blend.Colors[1] = Color.Black;
            blend.Colors[2] = Color.LightBlue;
            blend.Colors[3] = Color.LightBlue;
            blend.Positions[0] = 0;
            blend.Positions[1] = th / totalh;
            blend.Positions[2] = th / totalh;
            blend.Positions[3] = 1;
            tbrush.InterpolationColors = blend;

            t.Brush = tbrush;

            t.RowCount = rowCount;
            t.ColumnCount = 4;

            t.Scrollable = false;
            t.EnabledHandles = AdjustmentHandles.Move;
            t.CellFrameStyle = CellFrameStyle.None;
            t.HandlesStyle = HandlesStyle.DashFrame;
            t.Columns[0].Width = 5f; // colum 0 and 3 holds the image  // rh;
            t.Columns[3].Width = 5f; // colums 0 and 3 holds the image //  rh;
           
            t.Columns[1].Width = 35;
            t.Columns[2].Width = 15;
            t.Style = TableStyle.RoundedRectangle;
            t.CaptionColor = Color.White;

            // set connection points
            AnchorPoint ptin = new AnchorPoint(50, 50, true, false, Color.Red, 0);
            AnchorPoint ptout = new AnchorPoint(50, 50, false, true, Color.Red, 3);
            ArrayList al = new ArrayList();

            // normalize the input and output lengths so that when we add our anchor points for the row
            // we are adding both the input and output points for each row at the same time
            if (inputs.Length > outputs.Length)
            {
                Parameter[] temp = new Parameter[inputs.Length];
                for (int i = 0; i < outputs.Length; i++)
                    temp[i] = outputs[i];

                outputs = temp;
            }
            else if (outputs.Length > inputs.Length)
            {
                Parameter[] temp = new Parameter[outputs.Length];
                for (int i = 0; i < inputs.Length; i++)
                    temp[i] = outputs[i];

                inputs = temp;
            }

            for (int i = 0; i < inputs.Length; i++)
            {
                al.Clear();
                if (!(string.IsNullOrEmpty (inputs[i].DataType )))
                {
                  
                    t[0, i].ImageAlign = ImageAlign.Fit;
                    t[0, i].Image = images.Images[0]; // TODO: the image is derived from the datatype
                    t[1, i].Text = inputs[i].Name;
                    t[1,i].ToolTip =inputs[i].Tooltip;
                   
                    //t[0,i].Tag = // the reason i'd rather use the anchorPoint.Tag and not this cell's tag is because the it's just annoying to have to read the cell's xml for the tag
                    AnchorPoint anchorIn = (AnchorPoint)ptin.Clone();
                    anchorIn.Tag = inputs[i].ToString();
                    al.Add(anchorIn);
                }

                if (!(string.IsNullOrEmpty (outputs[i].DataType )))
                {
                    t[3, i].ImageAlign = ImageAlign.Fit;
                    t[3, i].Image = images.Images[1]; // TODO: the image is derived from the datatype
                    t[2, i].Text = outputs[i].Name;
                    t[2, i].ToolTip = outputs[i].Tooltip;
                    //t[0,i].Tag = 
                    t[2, i].TextFormat.Alignment = StringAlignment.Far;
                    AnchorPoint anchorOut = (AnchorPoint)ptout.Clone();
                    anchorOut.Tag = outputs[i].ToString();
                    al.Add(anchorOut);
                }

                // note: the input and output anchor points are added to the same row at the same time
                t.Rows[i].AnchorPattern = new AnchorPattern(
                    (AnchorPoint[])al.ToArray(typeof(AnchorPoint)));
            }
        }

        protected override void  OnResize(EventArgs e)
        {
 	        base.OnResize(e);
            
            if (this.pictureBox.Image != null)
                RatioStretch();
        }

        // note: Why have float, int and such types as nodes and not just directly as parameters?  The simply answer is
        // by having them nodes that have out parameters that can attach to in parameters, we have flexibility to have other
        // sources that yield float or int, etc out parameters as input sources other than the float and int and other dedicated node types.
        private bool Validate()
        {
            DiagramNode start = null;
            DiagramNode end = null;
            foreach (DiagramNode node in diagramView.Nodes)
            {
                //if (node.GetAllIncomingLinks().Count == 0)
                //{
                //    if (start != null) return false;
                //    start = node;
                //}
                if (node.GetAllOutgoingLinks().Count == 0)
                {
                    if (end != null) return false;
                    end = node;
                }
            }

            if (end == null) return false;
            if (start == end)
            {
                // start and end can be the same but only if it's the only node
                if (diagramView.Nodes.Count != 1) return false;
            }

            // Note: there is no single "start" node, but there is a single "end" node which is dervived from Libnoise.Model
            // eg. Sphere, Cylinder, Plane, Line

            // make sure no tablenodes have links that reconnect to anchors within themselves (something I already check for
            // during design time but which should be checked again here)

            // verify that all tablenodes have a path to the "end" node, otherwise there is a detached node or branch.

            // verify there is only one tableNode that has no output link and that is the "end" and the output type is "texture"

            return true;
        }

        public bool Generate()
        {
            if (Validate())
            {
                // save flow chart to a temporary file so that we can use our custom xml parser to parse it
                string path = @"E:\dev\c#\KeystoneGameBlocks\Data\flowchart.xml";
                Save(path);

                System.Xml.XmlDocument document = Keystone.IO.XmlHelper.OpenXmlDocument(path);
                System.Xml.XmlNode root = document.DocumentElement; // for our graph this is the "Diagram" node
                System.Xml.XmlNode nodes = Keystone.IO.XmlHelper.SelectNode(root, "Nodes", "", "");
                System.Xml.XmlNode links = Keystone.IO.XmlHelper.SelectNode(root, "Links", "", "");
                
                Hashtable entities = new Hashtable();
                Link[] instancedLinks = GetLinks(links.ChildNodes);

                bool result = Recurse(nodes.ChildNodes, instancedLinks, ref entities);

                // now if Recurse returned true, we should have all the entities instanced and now we just need to call
                // .Generate() on the NoiseMapModel node which i believe will always end up being the very last entity in the hashtable
                if (result)
                {

                    // dispose the previous image because according to MSDN, Image.FromFile() the file will remain locked and trying to overwrite it will fail.
                    // (plus the fact we should be disposing GDI objects anyway!.  It is odd that MS locks the file though with that call)
                    if (pictureBox.Image != null)
                        pictureBox.Image.Dispose();

                    // TODO: the index of the NoiseMapModel is not always the last entity.  We should just write a function
                    // getNoiseMapModel 
                    LibNoise.Models.Sphere sphere = (LibNoise.Models.Sphere) GetNoiseMapModel(entities);

                    System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
                    stopWatch.Start();
                    int[] colors = sphere.Generate();


                    bool hasAlpha = true; // used in the TextureFactory.CreateTexture() call.  cloud layers would have alpha but surface terrain + ocean layers would not.
                    string name = "libnoise";
                    AppMain._core.TextureFactory.SetTextureMode(MTV3D65.CONST_TV_TEXTUREMODE.TV_TEXTUREMODE_32BITS);
                    int textureIndex = AppMain._core.TextureFactory.CreateTexture(sphere.Width, sphere.Height, hasAlpha);
                    AppMain._core.TextureFactory.LockTexture(textureIndex, false);
                    AppMain._core.TextureFactory.SetPixelArray(textureIndex, 0, 0, sphere.Width, sphere.Height, colors);
                    AppMain._core.TextureFactory.UnlockTexture(textureIndex);
                    // TODO: use a temp file until the user has actually saved the file
                    string filename = AppMain._core.ModsPath + "\\caesar\\Shaders\\Planet\\libnoisegen.bmp";
                    AppMain._core.TextureFactory.SaveTexture(textureIndex, filename, MTV3D65.CONST_TV_IMAGEFORMAT.TV_IMAGE_BMP);
                    stopWatch.Stop();
                    System.Diagnostics.Trace.WriteLine(string.Format("Procedural texture '{0}' generated in {1} seconds", filename, stopWatch.Elapsed.TotalSeconds));

                    System.IO.FileStream fs = new System.IO.FileStream(filename, System.IO.FileMode.Open, System.IO.FileAccess.Read);
                    pictureBox.Image = System.Drawing.Image.FromStream(fs);
                    fs.Close();
                }
                return result;
            }
            return false;
        }

        private LibNoise.Models.NoiseMapModel GetNoiseMapModel(Hashtable entities)
        {
            foreach (object entity in entities.Values)
                if (entity is LibNoise.Models.NoiseMapModel) 
                    return (LibNoise.Models.NoiseMapModel)entity;

            return null;
        }

        public void Save(string path)
        {
            diagramView.SaveToXml(path);
            diagramView.LoadFromXml(path);
        }


        private bool Recurse(System.Xml.XmlNodeList nodes, Link[] links, ref Hashtable entities)
        {
            // now we're going to iterate thru the node's until we've instanced all of their libnoise counterparts
            bool found = false;
            foreach (System.Xml.XmlNode child in nodes)
            {
                int id = int.Parse(Keystone.IO.XmlHelper.GetAttributeValue (child, "Id"));

                // if this node already exists in the entity list, skip it
                if (entities.ContainsKey (id)) continue;
                
                
                // - first we need to get the id of the origins for all the links attached to our incoming anchor points
                Parameter[] inputs, outputs;
                if (!GetAnchorPointsInfo(child, links, out inputs, out outputs))
                    return false;
                // we're still here so this node has all of it's "in" parameter nodes in the entities list, we can instance this object
                // using our factory
                if (inputs != null && inputs.Length > 0)
                {
                    bool allInParametersInstanced = true;
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        // if edit in place  then assignmentRequired must be false
                        // I think we should simply update the in achor point Tag immediately upon editing the value.
                        //if (inputs[i].EditInPlace)
                        //    inputs[i].Value = GetEditInPlaceValue(child, inputs[i].RowID);

                        // System.Diagnostics.Trace.Assert(inputs[i].AssignmentRequired);
                        // -1 indicates the assignment is empty, so if its any value other than that AND the entity to which that assignment reference is not instanced we cant continue.
                        // we also check to see if a specific input
                        // requires an assignment to be made prior to being able to generate a texture (i.e is a condition of validation)
                        if (!string.IsNullOrEmpty(inputs[i].Name))
                        {
                            if ((inputs[i].AssignmentRequired && inputs[i].Link == -1) || (inputs[i].Link != -1 && !entities.ContainsKey(links[inputs[i].Link].Origin)))
                            {
                                allInParametersInstanced = false;
                                break;
                            }
                        }
                        else
                        {
                            if (outputs[i].AssignmentRequired && outputs[i].Link == -1)
                            {
                                allInParametersInstanced = false;
                                break;
                            }
                        }
                    }
                    //   we can instance this new entity and set the found = true
                    if (allInParametersInstanced)
                    {
                        // to instance it, we will pass the type, name, and the ordered array of in parameters as a list of object[]
                        // for any in parameter that is null, we reserve the null space in the ordered list and use default value
                        string name = Keystone.IO.XmlHelper.SelectNode(child, "Caption", "", "").InnerText;


                        // if value type which has means it has no linked inAssignments, they're all editInPlace then
                        // we only pass outputs
                        if (name == "Integer" || name == "Float")
                            entities.Add(id, CreateObject(entities, links, name, id, outputs ));
                        else
                        // else if a reference type
                            entities.Add(id, CreateObject(entities, links, name, id, inputs));

                        // since we are able to instance at least one  node, set the found = true
                        found = true;
                    }
                }
            }

            // if all nodes are now in the entity list, we're done
            if (entities.Count == nodes.Count )
                return true;
            else if (!found) 
                return false;  // if no new entity was instanced and we're not done, then this is an invalid graph and we can stop processing

            // else we recurse
            return Recurse(nodes, links,ref entities);
        }

        private bool GetAnchorPointsInfo(System.Xml.XmlNode node, Link[] instancedLinks, out Parameter[] inputs, out Parameter[] outputs)
        {
            System.Xml.XmlNode rows = Keystone.IO.XmlHelper.SelectNode(node, "Rows", "", "");
            int nodeID = int.Parse(Keystone.IO.XmlHelper.GetAttributeValue(node, "Id"));
            int rowID = 0;
            int rowCount = rows.ChildNodes.Count;
            inputs = new Parameter[rowCount];
            outputs = new Parameter[rowCount];
            
            
            foreach (System.Xml.XmlNode row in rows.ChildNodes)
            {
                System.Xml.XmlNode pattern = Keystone.IO.XmlHelper.SelectNode(row, "AnchorPattern", "", "");
                System.Xml.XmlNode points = Keystone.IO.XmlHelper.SelectNode(pattern, "Points", "", "");

                int pointID = 0;
                foreach (System.Xml.XmlNode point in points.ChildNodes)
                {
                    // go through the links array and find the link who's dest = node, who's row = rowID, and who's
                    // Anchor = pointID.  We only need to search for dest and since we only allow an input paramter to have
                    // a single link attached, there should only be one.  We could keep searching and return false if there is > 1
                    // for any link

                    for (int i = 0; i < instancedLinks.Length; i++)
                    {
                        Link l = instancedLinks[i];
                        // NOTE: we use rowID so if this anchor point does NOT have a link, that array position will contain null to preserve the ordering of the overall list of in and out parameters
                        Parameter p = new Parameter(i, rowID, GetTag(point));
                        if (p.IsInputParameter )
                        {
                            if (l.Dest == nodeID && l.DestRow == rowID && l.DestAnchor == pointID)
                                inputs[rowID] = p;
                        }
                        else 
                        {
                            if (l.Origin  == nodeID && l.OriginRow  == rowID && l.OriginAnchor  == pointID)
                                outputs[rowID] = p;
                        }
                    }
                    pointID++;
                }
                rowID++;
            }

            return true;
        }

        private string GetTag(System.Xml.XmlNode node)
        {
            System.Xml.XmlNode tag = Keystone.IO.XmlHelper.SelectNode(node, "Tag", "", "");
            return tag.InnerText;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nodeID"></param>
        /// <param name="inAssignments">The indices of the Links that are connected to this node's in parameters</param>
        /// <returns></returns>
        private object CreateObject(Hashtable entities,  Link[] links, string name, int nodeID, Parameter[] inputs)
        {
            object result;
            switch (name)
            {
                case "Scale Output":
                    LibNoise.Modifiers.ScaleOutput scaler = null;

                    for (int i = 0; i < inputs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(inputs[i].Name)) continue;

                        // based on the input field name, we'll know how to cast the entity or value type that is assigned to this input parameter
                        switch (inputs[i].Name)
                        {
                            case "Module":
                                LibNoise.IModule scalerSourceModule = (LibNoise.IModule)entities[links[inputs[0].Link].Origin];
                                scaler = new LibNoise.Modifiers.ScaleOutput(scalerSourceModule, 1);
                                break;
                            case "Scale":
                                scaler.Scale = (float)entities[links[inputs[i].Link].Origin];
                                break;

                            default:
                                break;
                        }
                    }
                    result = scaler;
                    break;

                case "Scale Bias Output":
                    LibNoise.Modifiers.ScaleBiasOutput biasScaler = null;
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(inputs[i].Name)) continue;

                        // based on the input field name, we'll know how to cast the entity or value type that is assigned to this input parameter
                        switch (inputs[i].Name)
                        {
                            case "Module":
                                LibNoise.IModule biasSourceModule = (LibNoise.IModule)entities[links[inputs[0].Link].Origin];
                                biasScaler = new LibNoise.Modifiers.ScaleBiasOutput(biasSourceModule);
                                break;
                            case "Scale":
                                biasScaler.Scale = (float)entities[links[inputs[i].Link].Origin];
                                break;

                            case "Bias":
                                biasScaler.Bias = (float)entities[links[inputs[i].Link].Origin];
                                break;

                            default:
                                break;
                        }
                    }
                    result = biasScaler;
                    break;

                case "Select":
                    LibNoise.Modifiers.Select selector = null;
                    for (int i = 0; i < inputs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(inputs[i].Name)) continue;

                        // based on the input field name, we'll know how to cast the entity or value type that is assigned to this input parameter
                        switch (inputs[i].Name)
                        {
                            case "ControlModule":
                                LibNoise.IModule selectControlModule = (LibNoise.IModule)entities[links[inputs[0].Link].Origin];
                                LibNoise.IModule selectSourceModule1 = (LibNoise.IModule)entities[links[inputs[1].Link].Origin];
                                LibNoise.IModule selectSourceModule2 = (LibNoise.IModule)entities[links[inputs[2].Link].Origin];
                                selector = new LibNoise.Modifiers.Select(selectControlModule, selectSourceModule1, selectSourceModule2);
                                break;

                            case "LowerBound":
                                // selector.LowerBound  = (float)entities[links[inputs[i].Link].Origin];
                                selector.SetBounds((float)entities[links[inputs[i].Link].Origin], (float)entities[links[inputs[i+1].Link].Origin]);
                                break;

                            case "UpperBound":
                               // selector.UpperBound  = (float)entities[links[inputs[i].Link].Origin];
                                break;

                             case "EdgeFalloff":
                                selector.EdgeFalloff   = (float)entities[links[inputs[i].Link].Origin];
                                break;

                            default:
                                break;
                        }
                    }
                    result = selector;
                    break;

                case "Integer":
                    // for value types such as this, inAssignments typically are in all editInPlace and so rather than using
                    // links array to find the origin and thus the entities index, we need to take the inputs[i].Value
                    result = int.Parse(inputs[0].Value.ToString()); 
                    break;

                case "Float":
                    result = float.Parse(inputs[0].Value.ToString());
                    break;
                    

                case "Fast Ridged Multifractal": // same as Fast Noise just no Persistence
                case "Ridged Multifractal":  // same as Fast Noise just no Persistence
                    LibNoise.INoiseBasis fractal = null;
                    if (name == "Ridged Multifractal")
                        fractal = new LibNoise.RidgedMultifractal();
                    else
                        fractal = new LibNoise.FastRidgedMultifractal();

                    for (int i = 0; i < inputs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(inputs[i].Name)) continue;

                        // based on the input field name, we'll know how to cast the entity or value type that is assigned to this input parameter
                        switch (inputs[i].Name)
                        {
                            case "Seed":
                                int seed = (int)entities[links[inputs[i].Link].Origin];
                                fractal.Seed = seed;
                                break;

                            case "Frequency":
                                float freq = (float)entities[links[inputs[i].Link].Origin];
                                fractal.Frequency = freq;
                                break;

                            case "Lacunarity":
                                float lacunarity = (float)entities[links[inputs[i].Link].Origin];
                                fractal.Lacunarity = lacunarity;
                                break;

                            case "OctaveCount":
                                int octaves = (int)entities[links[inputs[i].Link].Origin];
                                fractal.OctaveCount = octaves;
                                break;

                            case "NoiseQuality":
                                int noisequality = (int)entities[links[inputs[i].Link].Origin];
                                fractal.NoiseQuality = (LibNoise.NoiseQuality)noisequality;
                                break;

                            default:
                                break;
                        }
                    }
                    result = fractal;
                    break;

                case "Perlin":                     // same as Fast Noise
                case "Fast Billow":
                case "Billow":                    // same as Fast Noise
                case "Fast Noise":
                    LibNoise.IPersistenceNoiseBasis noise = null;

                    if (name == "Perlin")
                        noise = new LibNoise.Perlin();
                    else if (name == "Fast Billow")
                        noise = new LibNoise.FastBillow();
                    else if (name == "Billow")
                        noise = new LibNoise.Billow();
                    else
                        noise = new LibNoise.FastBillow();


                    for (int i = 0; i < inputs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(inputs[i].Name)) continue;

                        // based on the input field name, we'll know how to cast the entity or value type that is assigned to this input parameter
                        switch (inputs[i].Name)
                        {
                            case "Seed":
                                int seed = (int)entities[links[inputs[i].Link].Origin];
                                noise.Seed = seed;
                                break;

                            case "Frequency":
                                float freq = (float)entities[links[inputs[i].Link].Origin];
                                noise.Frequency = freq;
                                break;

                            case   "Lacunarity":
                                float lacunarity = (float)entities[links[inputs[i].Link].Origin];
                                noise.Lacunarity = lacunarity;
                                break;
                            
                            case  "OctaveCount":
                                int octaves = (int)entities[links[inputs[i].Link].Origin];
                                noise.OctaveCount  = octaves;
                                break;
                            
                            case "Persistence":
                                float persistence = (float)entities[links[inputs[i].Link].Origin];
                                noise.Persistence = persistence;
                                break;
                            
                            case "NoiseQuality":
                                int noisequality = (int)entities[links[inputs[i].Link].Origin];
                                noise.NoiseQuality = (LibNoise.NoiseQuality) noisequality;
                                break;

                            default:
                                break;
                        }
                    }
                    result = noise;
                    
                    break;

                case "Turbulence":
                case "Fast Turbulence":
                    result = null;
                    LibNoise.ITurbulence  turb = null;

                    for (int i = 0; i < inputs.Length; i++)
                    {
                        if (string.IsNullOrEmpty(inputs[i].Name)) continue;
                        switch (inputs[i].Name)
                        {
                            case "Seed":
                                turb.Seed = (int)entities[links[inputs[i].Link].Origin];
                                break;

                            case "Module":
                                LibNoise.IModule turbSourceModule = (LibNoise.IModule)entities[links[inputs[0].Link].Origin];
                                if (name == "Turbulence")
                                    turb = new LibNoise.Turbulence(turbSourceModule);
                                else
                                    turb = new LibNoise.FastTurbulence(turbSourceModule);
                                break;

                            case "Frequency":
                                turb.Frequency = (float)entities[links[inputs[i].Link].Origin];
                                break;

                            case "Roughness":
                                turb.Roughness = (int)entities[links[inputs[i].Link].Origin];
                                break;

                            case "Power":
                                turb.Power = (float)entities[links[inputs[i].Link].Origin];
                                break;

                            default:
                                break;
                        }
                    }

                    result = turb;
                    break;

                case "Sphere Mapper":
                    // the first assignment 0 cannot be null.
                    LibNoise.IModule module = (LibNoise.IModule) entities[links[inputs[0].Link].Origin];
                    LibNoise.Models.Sphere sphere = new LibNoise.Models.Sphere(module);
                    int textureHeight = 4096;// 1024;
                    int textureWidth = textureHeight * 2; // for sphere,s the width should be double the height to reduce warping

                    string palettePath = AppMain._core.ModsPath + "caesar\\Shaders\\Planet\\EarthLookupTable.png";
                    Bitmap lookupBitmap = new Bitmap(palettePath);
                    sphere.Palette = new LibNoise.Palette(lookupBitmap);

                    // we know the order for all assignments, if the item is not -1 (-1 means no link is set to it)
                    // then we must assign it 
                    // TODO: we need some note applied to a input param to indicate it's required and not optional
                    sphere.Width = textureWidth;
                    sphere.Height = textureHeight;
                    // if we wanted to zoom in on a particular spot, i think here we'd modify the extents?
                    sphere.SetCoordinateSystemExtents(-180, 180, -90, 90);
                    //sphere.SetCoordinateSystemExtents (minX, maxX, minY, maxY)
                    result = sphere;
                    break;

                default:
                    throw new ArgumentOutOfRangeException("Node '" + name + "' not supported.");
                    break;
            }
            
            return result;
        }

        private Link[] GetLinks(System.Xml.XmlNodeList links)
        {
            int count = links.Count ;
            Link[] instancedLinks = new Link[count];
            int i = 0;
            foreach (System.Xml.XmlNode node in links)
            {
                Link l = new Link();
                // we grab these origin, row and anchor values because we need all 3 to determine the actual origin in parameter
                l.Origin = int.Parse (Keystone.IO.XmlHelper.GetAttributeValue(Keystone.IO.XmlHelper.SelectNode(node, "Origin", "", ""), "Id"));  // since this points to an Id and Id only exists in entities after the entity is instanced, 
                System.Xml.XmlNode temp = Keystone.IO.XmlHelper.SelectNode(node, "OriginRow", "", "");
                l.OriginRow =int.Parse ( Keystone.IO.XmlHelper.SelectNode(node, "OriginRow", "", "").InnerText);
                l.OriginAnchor = int.Parse(Keystone.IO.XmlHelper.SelectNode(node, "OriginAnchor", "", "").InnerText);

                l.Dest = int.Parse (Keystone.IO.XmlHelper.GetAttributeValue(Keystone.IO.XmlHelper.SelectNode(node, "Destination", "", ""), "Id"));  // since this points to an Id and Id only exists in entities after the entity is instanced, 
                l.DestRow = int.Parse(Keystone.IO.XmlHelper.SelectNode(node, "DestinationRow", "", "").InnerText);
                l.DestAnchor = int.Parse(Keystone.IO.XmlHelper.SelectNode(node, "DestinationAnchor", "", "").InnerText);
                instancedLinks[i] = l;
                i++;
            }
            return instancedLinks;
        }


        #region Events
        private void OnCellTextEditing(object sender, CellValidationEventArgs args)
        {
            if (args.Table.Caption != "Integer" && args.Table.Caption != "Float")
            {
                args.Cancel = true;
                return;
            }
            if (args.Table.Rows[args.Row].AnchorPattern == null)
            {
                args.Cancel = true;
                return;
            }
            //string tag = args.Table.Rows[args.Row].AnchorPattern.Points[0].Tag.ToString();
            //Parameter p = new Parameter (1, args.Row , tag);
            //p.Value = args.Table[args.Column , args.Row ].Text;
            //args.Table.Rows[args.Row].AnchorPattern.Points[0].Tag = p.ToString();
        }

        private void OnCellTextEdited(object sender, EditCellTextEventArgs args)
        {
            string tag = args.Table.Rows[args.Row].AnchorPattern.Points[0].Tag.ToString();
            Parameter p = new Parameter(1, args.Row, tag);
            p.Value = args.Table[args.Column, args.Row].Text;
            args.Table.Rows[args.Row].AnchorPattern.Points[0].Tag = p.ToString();
        }

        private void OnLinkCreated(object sender, LinkEventArgs args)
        {
            // cannot link to self
            if (args.Link.Origin == args.Link.Destination)
            {
                diagramView.Links.Remove(args.Link);
                return;
            }
            // the input link and output link tag types must match
            TableNode  origin = (TableNode)args.Link.Origin;
            TableNode dest = (TableNode)args.Link.Destination;
            int originRow = args.Link.OriginConnection.Row;
            int destRow = args.Link.DestinationConnection.Row;

            // ensure diagramView.AllowUnanchoredLinks = false; in control Initialization
            if (args.Link.DestinationAnchor == -1)
            {
                diagramView.Links.Remove(args.Link);
                return;
            }

            // args.Link.OriginAnchor and args.Link.DestinationAnchor in our implementation will be -1 (none) 0 (inAnchor) 1 (outAnchor) for a given row number
            string originTag = (string)origin.Rows[originRow].AnchorPattern.Points[args.Link.OriginAnchor].Tag;
            string destTag = (string)dest.Rows[destRow].AnchorPattern.Points[args.Link.DestinationAnchor].Tag;
            string[] originSplit = originTag.Split(',');
            string[] destSplit = destTag.Split(',');
            if (originSplit[1] != destSplit[1]) // index = 1 holds the datatypes so here we verify the types are the same
                diagramView.Links.Remove(args.Link);
        }

        private void OnLinkCreating(object sender, LinkEventArgs args)
        {
            //if (args.Link.Origin == args.Link.Destination)
            //    throw new Exception();
        }

        // this event is raised during mouse move and again when mouse is released.  WHile moving, cancel = true
        // results in the cursor changing to "invalid" location icon.
        // but this is all so buggy.  I think best thing is to let users delete nodes if they make a mistake, but no modifying is
        // allowed.
        private void OnLinkModifying(object sender, LinkValidationEventArgs args)
        {
            args.Cancel = true;
            return;

            if (args.ChangingOrigin)
            {       
                args.Cancel = true;
                return;
            }  
            // cannot link to self
            if (args.Link.Origin == args.Link.Destination)
            {
                args.Cancel = true;
                return;
            }
            // the input link and output link tag types must match
            TableNode origin = (TableNode)args.Link.Origin;
            TableNode dest = (TableNode)args.Link.Destination;
            int originRow = args.Link.OriginConnection.Row;
            int destRow = args.Link.DestinationConnection.Row;

            // ensure diagramView.AllowUnanchoredLinks = false; in control Initialization
            if (args.Link.DestinationAnchor == -1)
            {
                args.Cancel = true;
                return;
            }

            // args.Link.OriginAnchor and args.Link.DestinationAnchor in our implementation will be -1 (none) 0 (inAnchor) 1 (outAnchor) for a given row number
            //if (origin.Rows[originRow].AnchorPattern.Points[args.Link.OriginAnchor].Tag != dest.Rows[destRow].AnchorPattern.Points[args.Link.DestinationAnchor].Tag)
            // args.Link.OriginAnchor and args.Link.DestinationAnchor in our implementation will be -1 (none) 0 (inAnchor) 1 (outAnchor) for a given row number
            string originTag = (string)origin.Rows[originRow].AnchorPattern.Points[args.Link.OriginAnchor].Tag;
            string destTag = (string)dest.Rows[destRow].AnchorPattern.Points[args.Link.DestinationAnchor].Tag;
            string[] originSplit = originTag.Split(',');
            string[] destSplit = destTag.Split(',');
            if (originSplit[1] != destSplit[1])
                args.Cancel = true; 
        }

        private void OnValidateAnchorPoint(object sender, LinkValidationEventArgs args)
        {
           // if (args.Origin == args.Destination) args.Cancel = true;
        }

        #endregion



    }


}
