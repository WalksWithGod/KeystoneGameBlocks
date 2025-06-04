using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace KeyEdit
{
    public partial class FormPleaseWait : Form, Keystone.Commands.ICommandProgress 
    {
        bool mIsAborting = false;

        public FormPleaseWait()
        {
            InitializeComponent();
        }


        #region ICommandProgress Members
        public void Begin(int minimum, int maximum)
        {
        }

        /// <summary>
        /// I want to be able to show the dialog first, then execute the command, and 
        /// have the main app blocked from accepting key or mouse input while this dialog
        /// is active.  
        /// </summary>
        public void Begin()
        {
           
        }

        public void SetRange(int minimum, int maximum)
        {
        }

        public void SetText(string text)
        {
        }

        public void StepTo(int val)
        {
        }

        public void Increment(int val)
        {
        }

        public bool IsAborting
        {
            get { return mIsAborting; }
        }

        public void End()
        {
            mIsAborting =true;
            this.Hide();
            this.Close();
        }
        #endregion
    }
}
