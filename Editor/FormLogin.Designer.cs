namespace KeyEdit
{
    partial class FormLogin
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
        	this.buttonLogin = new System.Windows.Forms.Button();
        	this.buttonScreenSaver = new System.Windows.Forms.Button();
        	this.textBox2 = new System.Windows.Forms.TextBox();
        	this.textBox1 = new System.Windows.Forms.TextBox();
        	this.label1 = new System.Windows.Forms.Label();
        	this.labelPassword = new System.Windows.Forms.Label();
        	this.checkBoxRememberPassword = new System.Windows.Forms.CheckBox();
        	this.checkBoxRememberLogin = new System.Windows.Forms.CheckBox();
        	this.buttonForgotPassword = new System.Windows.Forms.Button();
        	this.buttonNewAccount = new System.Windows.Forms.Button();
        	this.labelLogin = new System.Windows.Forms.Label();
        	this.SuspendLayout();
        	// 
        	// buttonLogin
        	// 
        	this.buttonLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.buttonLogin.Location = new System.Drawing.Point(93, 168);
        	this.buttonLogin.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.buttonLogin.Name = "buttonLogin";
        	this.buttonLogin.Size = new System.Drawing.Size(308, 61);
        	this.buttonLogin.TabIndex = 25;
        	this.buttonLogin.Text = "Login";
        	this.buttonLogin.UseVisualStyleBackColor = true;
        	// 
        	// buttonScreenSaver
        	// 
        	this.buttonScreenSaver.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.buttonScreenSaver.Location = new System.Drawing.Point(113, 393);
        	this.buttonScreenSaver.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.buttonScreenSaver.Name = "buttonScreenSaver";
        	this.buttonScreenSaver.Size = new System.Drawing.Size(267, 61);
        	this.buttonScreenSaver.TabIndex = 24;
        	this.buttonScreenSaver.Text = "Screen Saver";
        	this.buttonScreenSaver.UseVisualStyleBackColor = true;
        	// 
        	// textBox2
        	// 
        	this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.textBox2.Location = new System.Drawing.Point(180, 57);
        	this.textBox2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.textBox2.Name = "textBox2";
        	this.textBox2.PasswordChar = '*';
        	this.textBox2.Size = new System.Drawing.Size(221, 26);
        	this.textBox2.TabIndex = 23;
        	this.textBox2.Text = "password";
        	// 
        	// textBox1
        	// 
        	this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.textBox1.Location = new System.Drawing.Point(180, 23);
        	this.textBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.textBox1.Name = "textBox1";
        	this.textBox1.Size = new System.Drawing.Size(221, 26);
        	this.textBox1.TabIndex = 22;
        	this.textBox1.Text = "Hypnotron";
        	// 
        	// label1
        	// 
        	this.label1.AutoSize = true;
        	this.label1.BackColor = System.Drawing.Color.Transparent;
        	this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.label1.ForeColor = System.Drawing.Color.Teal;
        	this.label1.Location = new System.Drawing.Point(100, 458);
        	this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
        	this.label1.Name = "label1";
        	this.label1.Size = new System.Drawing.Size(370, 80);
        	this.label1.TabIndex = 21;
        	this.label1.Text = "Authentication required.  Please submit credentials.\r\n***Support will never ask y" +
	"ou for your password or \r\n    personal information.  Do not give it out.***\r\n\r\n";
        	// 
        	// labelPassword
        	// 
        	this.labelPassword.AutoSize = true;
        	this.labelPassword.BackColor = System.Drawing.Color.Transparent;
        	this.labelPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelPassword.ForeColor = System.Drawing.Color.Teal;
        	this.labelPassword.Location = new System.Drawing.Point(90, 65);
        	this.labelPassword.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
        	this.labelPassword.Name = "labelPassword";
        	this.labelPassword.Size = new System.Drawing.Size(82, 20);
        	this.labelPassword.TabIndex = 20;
        	this.labelPassword.Text = "Password:";
        	// 
        	// checkBoxRememberPassword
        	// 
        	this.checkBoxRememberPassword.AutoSize = true;
        	this.checkBoxRememberPassword.BackColor = System.Drawing.Color.Transparent;
        	this.checkBoxRememberPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.checkBoxRememberPassword.ForeColor = System.Drawing.Color.Teal;
        	this.checkBoxRememberPassword.Location = new System.Drawing.Point(93, 138);
        	this.checkBoxRememberPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.checkBoxRememberPassword.Name = "checkBoxRememberPassword";
        	this.checkBoxRememberPassword.Size = new System.Drawing.Size(180, 24);
        	this.checkBoxRememberPassword.TabIndex = 19;
        	this.checkBoxRememberPassword.Text = "Remember Password";
        	this.checkBoxRememberPassword.UseVisualStyleBackColor = false;
        	// 
        	// checkBoxRememberLogin
        	// 
        	this.checkBoxRememberLogin.AutoSize = true;
        	this.checkBoxRememberLogin.BackColor = System.Drawing.Color.Transparent;
        	this.checkBoxRememberLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.checkBoxRememberLogin.ForeColor = System.Drawing.Color.Teal;
        	this.checkBoxRememberLogin.Location = new System.Drawing.Point(93, 108);
        	this.checkBoxRememberLogin.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.checkBoxRememberLogin.Name = "checkBoxRememberLogin";
        	this.checkBoxRememberLogin.Size = new System.Drawing.Size(150, 24);
        	this.checkBoxRememberLogin.TabIndex = 18;
        	this.checkBoxRememberLogin.Text = "Remember Login";
        	this.checkBoxRememberLogin.UseVisualStyleBackColor = false;
        	// 
        	// buttonForgotPassword
        	// 
        	this.buttonForgotPassword.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.buttonForgotPassword.Location = new System.Drawing.Point(113, 315);
        	this.buttonForgotPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.buttonForgotPassword.Name = "buttonForgotPassword";
        	this.buttonForgotPassword.Size = new System.Drawing.Size(267, 61);
        	this.buttonForgotPassword.TabIndex = 17;
        	this.buttonForgotPassword.Text = "Forgot Password";
        	this.buttonForgotPassword.UseVisualStyleBackColor = true;
        	// 
        	// buttonNewAccount
        	// 
        	this.buttonNewAccount.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.buttonNewAccount.Location = new System.Drawing.Point(113, 246);
        	this.buttonNewAccount.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.buttonNewAccount.Name = "buttonNewAccount";
        	this.buttonNewAccount.Size = new System.Drawing.Size(267, 61);
        	this.buttonNewAccount.TabIndex = 16;
        	this.buttonNewAccount.Text = "Create New Account";
        	this.buttonNewAccount.UseVisualStyleBackColor = true;
        	// 
        	// labelLogin
        	// 
        	this.labelLogin.AutoSize = true;
        	this.labelLogin.BackColor = System.Drawing.Color.Transparent;
        	this.labelLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.labelLogin.ForeColor = System.Drawing.Color.Teal;
        	this.labelLogin.Location = new System.Drawing.Point(90, 31);
        	this.labelLogin.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
        	this.labelLogin.Name = "labelLogin";
        	this.labelLogin.Size = new System.Drawing.Size(52, 20);
        	this.labelLogin.TabIndex = 15;
        	this.labelLogin.Text = "Login:";
        	// 
        	// FormLogin
        	// 
        	this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
        	this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        	this.ClientSize = new System.Drawing.Size(492, 522);
        	this.Controls.Add(this.buttonLogin);
        	this.Controls.Add(this.buttonScreenSaver);
        	this.Controls.Add(this.textBox2);
        	this.Controls.Add(this.textBox1);
        	this.Controls.Add(this.label1);
        	this.Controls.Add(this.labelPassword);
        	this.Controls.Add(this.checkBoxRememberPassword);
        	this.Controls.Add(this.checkBoxRememberLogin);
        	this.Controls.Add(this.buttonForgotPassword);
        	this.Controls.Add(this.buttonNewAccount);
        	this.Controls.Add(this.labelLogin);
        	this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
        	this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
        	this.Name = "FormLogin";
        	this.Text = "Login";
        	this.ResumeLayout(false);
        	this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonLogin;
        private System.Windows.Forms.Button buttonScreenSaver;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labelPassword;
        private System.Windows.Forms.CheckBox checkBoxRememberPassword;
        private System.Windows.Forms.CheckBox checkBoxRememberLogin;
        private System.Windows.Forms.Button buttonForgotPassword;
        private System.Windows.Forms.Button buttonNewAccount;
        private System.Windows.Forms.Label labelLogin;
    }
}