﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;

namespace KeyPlugins.ColorPicker
{
    public class ColorListBox : ListBox
    {
        int m_knownColorCount = 0;
        public ColorListBox()
        {
            DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            PropertyInfo[] propinfos = typeof(Color).GetProperties(BindingFlags.Public | BindingFlags.Static);
            foreach (PropertyInfo info in propinfos)
            {
                if (info.PropertyType == typeof(Color))
                {
                    Color c = (Color)info.GetValue(typeof(Color), null);
                    if (c.A == 0) // skip transparent
                        continue;
                    Items.Add(c);
                }
            }
            m_knownColorCount = Items.Count;
        }
        int ColorIndex(Color color)
        {
            // have to search all colors by value;
            int argb = color.ToArgb();
            for (int index = 0; index < Items.Count; index++)
            {
                Color c = (Color)Items[index];
                if (c.ToArgb() == argb)
                    return index;
            }
            return -1;
        }
        public void SelectColor(Color color)
        {
            int index = ColorIndex(color);
            if (index < 0)
                index = SetCustomColor(color);
            SelectedItem = Items[index];
        }
        void RemoveCustomColor()
        {
            if (Items.Count > m_knownColorCount)
                Items.RemoveAt(Items.Count - 1);
        }
        int SetCustomColor(Color col)
        {
            RemoveCustomColor();
            Items.Add(col);
            return Items.Count - 1;
        }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index > -1)
            {
                e.DrawBackground();
                Rectangle rect = e.Bounds;
                rect.Inflate(-2, -2);
                rect.Width = 50;
                Color textColor = Color.Empty;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    textColor = SystemColors.HighlightText;
                else
                    textColor = this.ForeColor;
                Color color = (Color)Items[e.Index];
                using (Brush brush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(brush, rect);
                }
                if ((e.State & DrawItemState.Selected) != DrawItemState.Selected)
                {
                    using (Pen pen = new Pen(e.ForeColor))
                    {
                        e.Graphics.DrawRectangle(pen, rect);
                    }
                }
                using (Brush brush = new SolidBrush(e.ForeColor))
                {
                    string name = color.Name + string.Format("({0})", e.Index);
                    if (color.IsKnownColor == false)
                        name = "<custom>";

                    StringFormat format = new StringFormat();
                    format.LineAlignment = StringAlignment.Center;
                    rect = e.Bounds;
                    rect.X += 60;
                    e.Graphics.DrawString(name, Font, brush, rect, format);
                }
                e.DrawFocusRectangle();
            }
        }
    }
    }
