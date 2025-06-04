using System;
using System.Windows.Forms;

namespace KeyEdit
{
    public partial class FormNetwork : Form
    {
        public string Username;
        public string Password;
        public string ServerAddress;
        public int ServerPort = -1;
  
        public FormNetwork()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            txtUsername.Text = Username;
            txtKey.Text = Password ;
            txtKeyServerAddress.Text = ServerAddress;
            txtPort.Text =  ServerPort.ToString ();
        }
        private void buttonOK_Click(object sender, EventArgs e)
        {
            Username = txtUsername.Text;
            Password = txtKey.Text;
            ServerAddress = txtKeyServerAddress.Text ;
            int.TryParse (txtPort.Text, out ServerPort);

            this.DialogResult = DialogResult.OK;
           
        }
    }
}
