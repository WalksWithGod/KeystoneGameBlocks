﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using Open.Diagramming;

namespace Open.Diagramming.Examples.Forms
{
    public partial class frmClassDiagram : Form
    {
        public frmClassDiagram()
        {
            InitializeComponent();

            diagram1.Model = LoadModel();
            diagram1.Controller.Refresh();
        }
        
        //Load a Model with the diagram elements
        private Model LoadModel()
        {
            Model model = new Model();

            Table table = new Table();

            //Set Element properties
            table.BackColor = Color.White;
            table.GradientColor = Color.FromArgb(96, SystemColors.Highlight);
            table.Location = new PointF(100, 50);
            table.Width = 140;
            table.Height = 200;
            table.Indent = 10;
            table.Heading = "Element";
            table.SubHeading = "Class";
            table.DrawExpand = true;

            //Add the fields group
            TableGroup fieldGroup = new TableGroup();
            fieldGroup.Text = "Fields";
            table.Groups.Add(fieldGroup);

            //Add the fields rows
            //Layer
            TableRow row = new TableRow();
            row.Text = "Layer";
            row.Image = new Open.Diagramming.Image("Resource.publicfield.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            fieldGroup.Rows.Add(row);

            //SuspendEvents
            row = new TableRow();
            row.Text = "SuspendEvents";
            row.Image = new Open.Diagramming.Image("Resource.protectedfield.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            fieldGroup.Rows.Add(row);

            //Add the methods group
            TableGroup methodGroup = new TableGroup();
            methodGroup.Text = "Methods";
            table.Groups.Add(methodGroup);

            //Add the methods rows
            //AddPath
            row = new TableRow();
            row.Text = "AddPath";
            row.Image = new Open.Diagramming.Image("Resource.publicmethod.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            methodGroup.Rows.Add(row);

            row = new TableRow();
            row.Image = new Open.Diagramming.Image("Resource.protectedmethod.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            row.Text = "SetLayer";
            methodGroup.Rows.Add(row);

            //Add Element to model
            model.Shapes.Add("Element", table);

            table = new Table();

            //Set SolidElement properties
            table.BackColor = Color.White;
            table.GradientColor = Color.FromArgb(96, SystemColors.Highlight);
            table.Location = new PointF(100, 250);
            table.Width = 140;
            table.Height = 500;
            table.Indent = 10;
            table.Heading = "SolidElement";
            table.SubHeading = "Class";
            table.DrawExpand = true;

            //Add the fields group
            fieldGroup = new TableGroup();
            fieldGroup.Text = "Fields";
            table.Groups.Add(fieldGroup);

            //Add the fields rows
            //BackColor
            row = new TableRow();
            row.Text = "BackColor";
            row.Image = new Open.Diagramming.Image("Resource.publicfield.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            fieldGroup.Rows.Add(row);

            //Add the methods group
            methodGroup = new TableGroup();
            methodGroup.Text = "Methods";
            table.Groups.Add(methodGroup);

            //Add the methods rows
            //ScalePath
            row = new TableRow();
            row.Text = "ScalePath";
            row.Image = new Open.Diagramming.Image("Resource.publicmethod.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            methodGroup.Rows.Add(row);

            //Add Element to model
            model.Shapes.Add("SolidElement", table);

            table = new Table();

            //Set Shape properties
            table.BackColor = Color.White;
            table.GradientColor = Color.FromArgb(96, SystemColors.Highlight);
            table.Location = new PointF(100, 410);
            table.Width = 140;
            table.Height = 500;
            table.Indent = 10;
            table.Heading = "Shape";
            table.SubHeading = "Class";
            table.DrawExpand = true;

            //Add the fields group
            fieldGroup = new TableGroup();
            fieldGroup.Text = "Fields";
            table.Groups.Add(fieldGroup);

            //Add the fields rows
            //AllowMove
            row = new TableRow();
            row.Text = "AllowMove";
            row.Image = new Open.Diagramming.Image("Resource.publicfield.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            fieldGroup.Rows.Add(row);

            //Add the methods group
            methodGroup = new TableGroup();
            methodGroup.Text = "Methods";
            table.Groups.Add(methodGroup);

            //Add the methods rows
            //Rotate
            row = new TableRow();
            row.Text = "Rotate";
            row.Image = new Open.Diagramming.Image("Resource.publicmethod.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            methodGroup.Rows.Add(row);

            //Add Element to model
            model.Shapes.Add("Shape", table);

            //Add a Layer class
            table = new Table();

            //Set Shape properties
            table.BackColor = Color.White;
            table.GradientColor = Color.FromArgb(96, SystemColors.Highlight);
            table.Location = new PointF(400, 100);
            table.Width = 140;
            table.Height = 500;
            table.Indent = 10;
            table.Heading = "Layer";
            table.SubHeading = "Class";
            table.DrawExpand = true;

            //Add the fields group
            fieldGroup = new TableGroup();
            fieldGroup.Text = "Fields";
            table.Groups.Add(fieldGroup);

            //Add the fields rows
            //DrawShadows
            row = new TableRow();
            row.Text = "DrawShadows";
            row.Image = new Open.Diagramming.Image("Resource.publicfield.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            fieldGroup.Rows.Add(row);

            //Elements
            row = new TableRow();
            row.Text = "Elements";
            row.Image = new Open.Diagramming.Image("Resource.publicfield.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            fieldGroup.Rows.Add(row);

            //Opacity
            row = new TableRow();
            row.Text = "Opacity";
            row.Image = new Open.Diagramming.Image("Resource.publicfield.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            fieldGroup.Rows.Add(row);

            //SuspendEvents
            row = new TableRow();
            row.Text = "SuspendEvents";
            row.Image = new Open.Diagramming.Image("Resource.protectedfield.gif", "Open.Diagramming.Examples.Forms.frmClassDiagram");
            fieldGroup.Rows.Add(row);

            //Add Layer to model
            model.Shapes.Add("Layer", table);

            //Add GradientMode enumeration shape
            table = new Table();

            //Set Shape properties
            table.BackColor = Color.White;
            table.GradientColor = Color.FromArgb(128, Color.GreenYellow);
            table.Location = new PointF(400, 300);
            table.Width = 140;
            table.Height = 500;
            table.Indent = 10;
            table.Heading = "GradientMode";
            table.SubHeading = "Enum";
            table.DrawExpand = true;

            //Add the fields rows
            //BackwardDiagonal
            row = new TableRow();
            row.Text = "BackwardDiagonal";
            table.Rows.Add(row);

            //ForwardDiagonal
            row = new TableRow();
            row.Text = "ForwardDiagonal";
            table.Rows.Add(row);

            //Vertical
            row = new TableRow();
            row.Text = "Vertical";
            table.Rows.Add(row);

            //Horizontal
            row = new TableRow();
            row.Text = "Horizontal";
            table.Rows.Add(row);

            //Add GradientMode to model
            model.Shapes.Add("GradientMode", table);

            //Connect
            //Create an arrow line marker
            Arrow arrow = new Arrow();
            arrow.DrawBackground = false;
            arrow.Inset = 8;

            //Add link between shape and solid
            Connector line = new Connector((Shape)model.Shapes["Shape"], (Shape)model.Shapes["SolidElement"]);
            line.End.Marker = arrow;
            model.Lines.Add(model.Lines.CreateKey(), line);

            //Add link between solid and element
            line = new Connector((Shape)model.Shapes["SolidElement"], (Shape)model.Shapes["Element"]);
            line.End.Marker = arrow;
            model.Lines.Add(model.Lines.CreateKey(), line);

            //Dependancies
            //Add dependancy between element and layer
            arrow = new Arrow();
            arrow.DrawBackground = false;
            arrow.Inset = 0;

            line = new Connector((Shape)model.Shapes["Element"], (Shape)model.Shapes["Layer"]);
            line.BorderStyle = System.Drawing.Drawing2D.DashStyle.Dash;
            line.End.Marker = arrow;

            model.Lines.Add(model.Lines.CreateKey(), line);

            return model;
        }
    }
}
