using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

public partial class SplashScreen : Form
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
        this.progressBar1 = new System.Windows.Forms.ProgressBar();
        this.label1 = new System.Windows.Forms.Label();
        this.SuspendLayout();
        // 
        // progressBar1
        // 
        this.progressBar1.Location = new System.Drawing.Point(9, 25);
        this.progressBar1.Name = "progressBar1";
        this.progressBar1.Size = new System.Drawing.Size(364, 17);
        this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
        this.progressBar1.TabIndex = 1;
        // 
        // label1
        // 
        this.label1.BackColor = System.Drawing.Color.Transparent;
        this.label1.Location = new System.Drawing.Point(6, 9);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(70, 13);
        this.label1.TabIndex = 2;
        this.label1.Text = "Please wait...";
        // 
        // SplashScreen
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
        this.ClientSize = new System.Drawing.Size(385, 50);
        this.Controls.Add(this.label1);
        this.Controls.Add(this.progressBar1);
        this.DoubleBuffered = true;
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
        this.Name = "SplashScreen";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "CS-Script";
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Label label1;

    public SplashScreen()
    {
        InitializeComponent();
    }
    public SplashScreen(string title, string message)
    {
        InitializeComponent();
        Text = title;
        label1.Text = message;
    }
    private static SplashScreen instance;

    public static void ShowSplash(string title, string message)
    {
        Thread t = new Thread(
            delegate(object obj)
            {
                instance = new SplashScreen(title, message);
                instance.ShowDialog();
            });
        t.IsBackground = true; 
        t.Start();
    }
    public static void ShowSplash()
    {
        Thread t = new Thread(
            delegate(object obj)
            {
                instance = new SplashScreen();
                instance.ShowDialog();
            });
        t.IsBackground = true;
        t.Start(); 
    }
    public static void HideSplash()
    {
        try
        {
            instance.Invoke((MethodInvoker)delegate { Application.ExitThread(); });
        }
        catch { }
    }

}
