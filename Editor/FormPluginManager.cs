using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using KeyPlugins;

namespace KeyEdit
{
    public partial class FormPluginManager : Form
    {
        public FormPluginManager()
        {
            InitializeComponent();


            foreach (AvailablePlugin plugin in AppMain.PluginService.AvailablePlugins)
            {
                listBox1.Items.Add(plugin.Instance.Name);
            }

        }
    }
}
