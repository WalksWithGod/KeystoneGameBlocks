using System;
using System.Windows.Forms;
using Settings;

namespace KeyEdit
{
    public partial class ControlPanel : Form
    {
        public PropertyBagCollection Properties;


        public ControlPanel(PropertyBagCollection properties)
        {
            if (properties == null) throw new ArgumentNullException();
            InitializeComponent();

            Properties = properties;

            string[] categories = properties.GetCategoryNames();
            if (categories == null || categories.Length == 0) return;

            for (int i = 0; i < categories.Length; i++)
            {

                PropertySpec[] categorizedProperties = properties.PropertiesByCategory(categories[i]);
                if (categorizedProperties != null)
                {
                    TreeNode node = new TreeNode(categories[i]);
                    tree.Nodes.Add(node);
                    node.Tag = categorizedProperties;
                }
            }
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            grid.Select();
            try
            {
                // NOTE: If .SelectedObject assignment throws NullReferenceException it's because
                // the "type" parameter of one or more of the propertyspec's items in
                // the property bag is null.
                PropertySpec[] properties = (PropertySpec[])e.Node.Tag;
                PropertyBag tempBag = new PropertyBag();
                tempBag.Properties.AddRange(properties);
                // from the array of properties stored on this bag, create a bag to use as the 
                // selected object
                grid.SelectedObject = tempBag; // bag;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
    }
}