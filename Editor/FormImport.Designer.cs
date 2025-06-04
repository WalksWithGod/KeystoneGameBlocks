namespace KeyEdit
{
    partial class FormImport
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonCreateNew = new System.Windows.Forms.Button();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.buttonCreateSubFolder = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.labelDestPath = new System.Windows.Forms.Label();
            this.buttonUndo = new System.Windows.Forms.Button();
            this.checkBoxImportTextures = new System.Windows.Forms.CheckBox();
            this.checkBoxImportMaterials = new System.Windows.Forms.CheckBox();
            this.checkBoxConvertToTVM = new System.Windows.Forms.CheckBox();
            this.labelSrcPath = new System.Windows.Forms.Label();
            this.comboBoxMod = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.checkBoxImportAsActor = new System.Windows.Forms.CheckBox();
            this.textBoxFileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // buttonCreateNew
            // 
            this.buttonCreateNew.Location = new System.Drawing.Point(416, 105);
            this.buttonCreateNew.Name = "buttonCreateNew";
            this.buttonCreateNew.Size = new System.Drawing.Size(164, 25);
            this.buttonCreateNew.TabIndex = 3;
            this.buttonCreateNew.Text = "Create New Mod...";
            this.buttonCreateNew.UseVisualStyleBackColor = true;
            this.buttonCreateNew.Click += new System.EventHandler(this.buttonCreateNew_Click);
            // 
            // treeView1
            // 
            this.treeView1.Location = new System.Drawing.Point(12, 131);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(385, 178);
            this.treeView1.TabIndex = 5;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            // 
            // buttonCreateSubFolder
            // 
            this.buttonCreateSubFolder.Location = new System.Drawing.Point(416, 136);
            this.buttonCreateSubFolder.Name = "buttonCreateSubFolder";
            this.buttonCreateSubFolder.Size = new System.Drawing.Size(164, 25);
            this.buttonCreateSubFolder.TabIndex = 6;
            this.buttonCreateSubFolder.Text = "Add New Sub Folder...";
            this.buttonCreateSubFolder.UseVisualStyleBackColor = true;
            this.buttonCreateSubFolder.Click += new System.EventHandler(this.buttonCreateSubFolder_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Location = new System.Drawing.Point(261, 355);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(164, 25);
            this.buttonCancel.TabIndex = 7;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Location = new System.Drawing.Point(447, 355);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(164, 25);
            this.buttonOK.TabIndex = 8;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelDestPath
            // 
            this.labelDestPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDestPath.Location = new System.Drawing.Point(12, 312);
            this.labelDestPath.Name = "labelDestPath";
            this.labelDestPath.Size = new System.Drawing.Size(599, 40);
            this.labelDestPath.TabIndex = 10;
            this.labelDestPath.Text = "Dest Path:  Data\\Mods\\Common\\";
            // 
            // buttonUndo
            // 
            this.buttonUndo.Location = new System.Drawing.Point(416, 167);
            this.buttonUndo.Name = "buttonUndo";
            this.buttonUndo.Size = new System.Drawing.Size(164, 25);
            this.buttonUndo.TabIndex = 11;
            this.buttonUndo.Text = "Undo Previous Action";
            this.buttonUndo.UseVisualStyleBackColor = true;
            this.buttonUndo.Click += new System.EventHandler(this.buttonUndo_Click);
            // 
            // checkBoxImportTextures
            // 
            this.checkBoxImportTextures.AutoSize = true;
            this.checkBoxImportTextures.Checked = true;
            this.checkBoxImportTextures.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxImportTextures.Location = new System.Drawing.Point(416, 212);
            this.checkBoxImportTextures.Name = "checkBoxImportTextures";
            this.checkBoxImportTextures.Size = new System.Drawing.Size(153, 17);
            this.checkBoxImportTextures.TabIndex = 12;
            this.checkBoxImportTextures.Text = "Import Embedded Textures";
            this.checkBoxImportTextures.UseVisualStyleBackColor = true;
            // 
            // checkBoxImportMaterials
            // 
            this.checkBoxImportMaterials.AutoSize = true;
            this.checkBoxImportMaterials.Checked = true;
            this.checkBoxImportMaterials.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxImportMaterials.Location = new System.Drawing.Point(416, 235);
            this.checkBoxImportMaterials.Name = "checkBoxImportMaterials";
            this.checkBoxImportMaterials.Size = new System.Drawing.Size(154, 17);
            this.checkBoxImportMaterials.TabIndex = 13;
            this.checkBoxImportMaterials.Text = "Import Embedded Materials";
            this.checkBoxImportMaterials.UseVisualStyleBackColor = true;
            // 
            // checkBoxConvertToTVM
            // 
            this.checkBoxConvertToTVM.AutoSize = true;
            this.checkBoxConvertToTVM.Checked = true;
            this.checkBoxConvertToTVM.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxConvertToTVM.Location = new System.Drawing.Point(416, 279);
            this.checkBoxConvertToTVM.Name = "checkBoxConvertToTVM";
            this.checkBoxConvertToTVM.Size = new System.Drawing.Size(184, 17);
            this.checkBoxConvertToTVM.TabIndex = 14;
            this.checkBoxConvertToTVM.Text = "Convert .X && .OBJ to .TVM\\ .TVA";
            this.checkBoxConvertToTVM.UseVisualStyleBackColor = true;
            // 
            // labelSrcPath
            // 
            this.labelSrcPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelSrcPath.Location = new System.Drawing.Point(9, 7);
            this.labelSrcPath.Name = "labelSrcPath";
            this.labelSrcPath.Size = new System.Drawing.Size(602, 36);
            this.labelSrcPath.TabIndex = 15;
            this.labelSrcPath.Text = "Src Path:  Data\\Mods\\Common\\";
            // 
            // comboBoxMod
            // 
            this.comboBoxMod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMod.FormattingEnabled = true;
            this.comboBoxMod.Location = new System.Drawing.Point(12, 104);
            this.comboBoxMod.Name = "comboBoxMod";
            this.comboBoxMod.Size = new System.Drawing.Size(385, 21);
            this.comboBoxMod.TabIndex = 16;
            this.comboBoxMod.SelectedIndexChanged += new System.EventHandler(this.comboBoxMod_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(9, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(76, 16);
            this.label3.TabIndex = 17;
            this.label3.Text = "Select Mod";
            // 
            // checkBoxImportAsActor
            // 
            this.checkBoxImportAsActor.AutoSize = true;
            this.checkBoxImportAsActor.Checked = true;
            this.checkBoxImportAsActor.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxImportAsActor.Location = new System.Drawing.Point(416, 258);
            this.checkBoxImportAsActor.Name = "checkBoxImportAsActor";
            this.checkBoxImportAsActor.Size = new System.Drawing.Size(163, 17);
            this.checkBoxImportAsActor.TabIndex = 18;
            this.checkBoxImportAsActor.Text = "Import .X as Actor (not Mesh)";
            this.checkBoxImportAsActor.UseVisualStyleBackColor = true;
            // 
            // textBoxFileName
            // 
            this.textBoxFileName.Location = new System.Drawing.Point(12, 62);
            this.textBoxFileName.Name = "textBoxFileName";
            this.textBoxFileName.Size = new System.Drawing.Size(385, 20);
            this.textBoxFileName.TabIndex = 19;
            this.textBoxFileName.TextChanged += new System.EventHandler(this.textBoxFileName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 16);
            this.label1.TabIndex = 20;
            this.label1.Text = "Filename";
            // 
            // FormImport
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(623, 390);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBoxFileName);
            this.Controls.Add(this.checkBoxImportAsActor);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBoxMod);
            this.Controls.Add(this.labelSrcPath);
            this.Controls.Add(this.checkBoxConvertToTVM);
            this.Controls.Add(this.checkBoxImportMaterials);
            this.Controls.Add(this.checkBoxImportTextures);
            this.Controls.Add(this.buttonUndo);
            this.Controls.Add(this.labelDestPath);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonCreateSubFolder);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.buttonCreateNew);
            this.Name = "FormImport";
            this.Text = "Import New Geometry To Mod  Data Library";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonCreateNew;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.Button buttonCreateSubFolder;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Label labelDestPath;
        private System.Windows.Forms.Button buttonUndo;
        private System.Windows.Forms.CheckBox checkBoxImportTextures;
        private System.Windows.Forms.CheckBox checkBoxImportMaterials;
        private System.Windows.Forms.CheckBox checkBoxConvertToTVM;
        private System.Windows.Forms.Label labelSrcPath;
        private System.Windows.Forms.ComboBox comboBoxMod;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox checkBoxImportAsActor;
        private System.Windows.Forms.TextBox textBoxFileName;
        private System.Windows.Forms.Label label1;
    }
}