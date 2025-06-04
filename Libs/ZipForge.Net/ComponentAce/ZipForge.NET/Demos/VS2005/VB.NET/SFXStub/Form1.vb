Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data
Imports System.IO
Imports ComponentAce.Compression.ZipForge
Imports ComponentAce.Compression.Archiver


Namespace SFXStub
	''' <summary>
	''' Summary description for Form1.
	''' </summary>
	Public Class Form1
		Inherits System.Windows.Forms.Form
		''' <summary>
		''' Required designer variable.
		''' </summary>
		Private components As System.ComponentModel.Container = Nothing
		Private WithEvents button1 As Button
		Private WithEvents button2 As Button
		Private WithEvents button3 As Button
		Private folderBrowserDialog1 As FolderBrowserDialog
		Private label1 As Label
		Private checkBox1 As System.Windows.Forms.CheckBox
		Private textBox1 As TextBox


		Public Sub New()
			'
			' Required for Windows Form Designer support
			'
			InitializeComponent()

			'
			' TODO: Add any constructor code after InitializeComponent call
			'
		End Sub

		''' <summary>
		''' Clean up any resources being used.
		''' </summary>
		Protected Overrides Overloads Sub Dispose(ByVal disposing As Boolean)
			If disposing Then
				If Not components Is Nothing Then
					components.Dispose()
				End If
			End If
			MyBase.Dispose(disposing)
		End Sub

		#Region "Windows Form Designer generated code"
		''' <summary>
		''' Required method for Designer support - do not modify
		''' the contents of this method with the code editor.
		''' </summary>
		Private Sub InitializeComponent()
			Me.label1 = New System.Windows.Forms.Label()
			Me.textBox1 = New System.Windows.Forms.TextBox()
			Me.folderBrowserDialog1 = New System.Windows.Forms.FolderBrowserDialog()
			Me.button1 = New System.Windows.Forms.Button()
			Me.button2 = New System.Windows.Forms.Button()
			Me.button3 = New System.Windows.Forms.Button()
			Me.checkBox1 = New System.Windows.Forms.CheckBox()
			Me.SuspendLayout()
			' 
			' label1
			' 
			Me.label1.Location = New System.Drawing.Point(14, 8)
			Me.label1.Name = "label1"
			Me.label1.Size = New System.Drawing.Size(320, 16)
			Me.label1.TabIndex = 0
			Me.label1.Text = "Select destination folder"
			' 
			' textBox1
			' 
			Me.textBox1.Location = New System.Drawing.Point(16, 32)
			Me.textBox1.Name = "textBox1"
			Me.textBox1.Size = New System.Drawing.Size(320, 20)
			Me.textBox1.TabIndex = 1
			Me.textBox1.Text = ""
			' 
			' button1
			' 
			Me.button1.Location = New System.Drawing.Point(344, 32)
			Me.button1.Name = "button1"
			Me.button1.TabIndex = 2
			Me.button1.Text = "Browse..."
'			Me.button1.Click += New System.EventHandler(Me.button1_Click);
			' 
			' button2
			' 
			Me.button2.Location = New System.Drawing.Point(128, 96)
			Me.button2.Name = "button2"
			Me.button2.TabIndex = 3
			Me.button2.Text = "Extract"
'			Me.button2.Click += New System.EventHandler(Me.button2_Click);
			' 
			' button3
			' 
			Me.button3.Location = New System.Drawing.Point(224, 96)
			Me.button3.Name = "button3"
			Me.button3.TabIndex = 4
			Me.button3.Text = "Exit"
'			Me.button3.Click += New System.EventHandler(Me.button3_Click);
			' 
			' checkBox1
			' 
			Me.checkBox1.Location = New System.Drawing.Point(16, 64)
			Me.checkBox1.Name = "checkBox1"
			Me.checkBox1.Size = New System.Drawing.Size(176, 16)
			Me.checkBox1.TabIndex = 5
			Me.checkBox1.Text = "Overwrite existing files"
			' 
			' Form1
			' 
			Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
			Me.ClientSize = New System.Drawing.Size(424, 133)
			Me.Controls.Add(Me.checkBox1)
			Me.Controls.Add(Me.button3)
			Me.Controls.Add(Me.button2)
			Me.Controls.Add(Me.button1)
			Me.Controls.Add(Me.textBox1)
			Me.Controls.Add(Me.label1)
			Me.Name = "Form1"
			Me.Text = "ZipForge.NET SFX  stub demo (C) ComponentAce 2007"
			Me.ResumeLayout(False)

		End Sub
		#End Region

		''' <summary>
		''' The main entry point for the application.
		''' </summary>
		<STAThread> _
		Shared Sub Main()
			Application.Run(New Form1())
		End Sub

		Private Sub button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles button1.Click
			If Me.folderBrowserDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
				Me.textBox1.Text = Me.folderBrowserDialog1.SelectedPath
			End If

		End Sub

		Private Sub button2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles button2.Click
            Dim ZipForge1 As ZipForge = New ZipForge
			ZipForge1.FileName = Application.ExecutablePath
			ZipForge1.OpenArchive(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
			ZipForge1.BaseDir = Me.textBox1.Text
			If checkBox1.Checked Then
                ZipForge1.Options.Overwrite = OverwriteMode.Always
			Else
                ZipForge1.Options.Overwrite = OverwriteMode.Never
			End If
			ZipForge1.ExtractFiles("*.*")
			ZipForge1.CloseArchive()
			System.Windows.Forms.MessageBox.Show("All files were extracted successfully")

		End Sub

		Private Sub button3_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles button3.Click
			Close()
		End Sub

        Private Sub ZipForge1_OnFileProgress(ByVal sender As Object, ByVal fileName As String, ByVal progress As Double, ByVal operation As ProcessOperation, ByVal progressPhase As ProgressPhase, ByRef cancel As Boolean)

        End Sub
    End Class
End Namespace
