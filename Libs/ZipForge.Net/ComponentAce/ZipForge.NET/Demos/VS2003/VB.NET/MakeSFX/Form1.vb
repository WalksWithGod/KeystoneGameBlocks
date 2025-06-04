Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data
Imports ComponentAce.Compression.ZipForge
Imports ComponentAce.Compression.Archiver


Namespace MakeSFX
	''' <summary>
	''' Summary description for Form1.
	''' </summary>
	Public Class Form1
		Inherits System.Windows.Forms.Form
		Friend SaveFileDialog1 As System.Windows.Forms.SaveFileDialog
		Friend WithEvents Button3 As System.Windows.Forms.Button
		Friend WithEvents Button2 As System.Windows.Forms.Button
		Friend WithEvents Button1 As System.Windows.Forms.Button
		Friend TextBox2 As System.Windows.Forms.TextBox
		Friend TextBox1 As System.Windows.Forms.TextBox
		Friend Label1 As System.Windows.Forms.Label
		Friend OpenFileDialog1 As System.Windows.Forms.OpenFileDialog
		Friend Label2 As System.Windows.Forms.Label
		Friend OpenFileDialog2 As System.Windows.Forms.OpenFileDialog
		''' <summary>
		''' Required designer variable.
		''' </summary>
		Private components As System.ComponentModel.Container = Nothing

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
			Me.SaveFileDialog1 = New System.Windows.Forms.SaveFileDialog()
			Me.Button3 = New System.Windows.Forms.Button()
			Me.Button2 = New System.Windows.Forms.Button()
			Me.Button1 = New System.Windows.Forms.Button()
			Me.TextBox2 = New System.Windows.Forms.TextBox()
			Me.TextBox1 = New System.Windows.Forms.TextBox()
			Me.Label1 = New System.Windows.Forms.Label()
			Me.OpenFileDialog1 = New System.Windows.Forms.OpenFileDialog()
			Me.Label2 = New System.Windows.Forms.Label()
			Me.OpenFileDialog2 = New System.Windows.Forms.OpenFileDialog()
			Me.SuspendLayout()
			' 
			' SaveFileDialog1
			' 
			Me.SaveFileDialog1.Filter = "Applications | *.exe"
			' 
			' Button3
			' 
			Me.Button3.Location = New System.Drawing.Point(176, 96)
			Me.Button3.Name = "Button3"
			Me.Button3.Size = New System.Drawing.Size(160, 24)
			Me.Button3.TabIndex = 13
			Me.Button3.Text = "Make SFX..."
'			Me.Button3.Click += New System.EventHandler(Me.Button3_Click);
			' 
			' Button2
			' 
			Me.Button2.Location = New System.Drawing.Point(408, 56)
			Me.Button2.Name = "Button2"
			Me.Button2.Size = New System.Drawing.Size(64, 21)
			Me.Button2.TabIndex = 12
			Me.Button2.Text = "Browse..."
'			Me.Button2.Click += New System.EventHandler(Me.Button2_Click);
			' 
			' Button1
			' 
			Me.Button1.Location = New System.Drawing.Point(408, 24)
			Me.Button1.Name = "Button1"
			Me.Button1.Size = New System.Drawing.Size(65, 21)
			Me.Button1.TabIndex = 11
			Me.Button1.Text = "Browse..."
'			Me.Button1.Click += New System.EventHandler(Me.Button1_Click);
			' 
			' TextBox2
			' 
			Me.TextBox2.Location = New System.Drawing.Point(128, 56)
			Me.TextBox2.Name = "TextBox2"
			Me.TextBox2.Size = New System.Drawing.Size(272, 20)
			Me.TextBox2.TabIndex = 10
			Me.TextBox2.Text = ""
			' 
			' TextBox1
			' 
			Me.TextBox1.Location = New System.Drawing.Point(128, 24)
			Me.TextBox1.Name = "TextBox1"
			Me.TextBox1.Size = New System.Drawing.Size(272, 20)
			Me.TextBox1.TabIndex = 9
			Me.TextBox1.Text = ""
			' 
			' Label1
			' 
			Me.Label1.Location = New System.Drawing.Point(16, 24)
			Me.Label1.Name = "Label1"
			Me.Label1.TabIndex = 7
			Me.Label1.Text = "Archive file name"
			' 
			' OpenFileDialog1
			' 
            Me.OpenFileDialog1.DefaultExt = "*.zip"
            Me.OpenFileDialog1.Filter = "ZipForge archives | *.zip"
			' 
			' Label2
			' 
			Me.Label2.Location = New System.Drawing.Point(16, 64)
			Me.Label2.Name = "Label2"
			Me.Label2.TabIndex = 8
			Me.Label2.Text = "SFX stub file name"
			' 
			' OpenFileDialog2
			' 
			Me.OpenFileDialog2.DefaultExt = "*.exe"
			Me.OpenFileDialog2.Filter = "Applications | *.exe"
			' 
			' Form1
			' 
			Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
			Me.ClientSize = New System.Drawing.Size(504, 125)
			Me.Controls.Add(Me.TextBox2)
			Me.Controls.Add(Me.TextBox1)
			Me.Controls.Add(Me.Label1)
			Me.Controls.Add(Me.Label2)
			Me.Controls.Add(Me.Button3)
			Me.Controls.Add(Me.Button2)
			Me.Controls.Add(Me.Button1)
			Me.Name = "Form1"
			Me.Text = "MakeSFX demo (C) ComponentAce 2007"
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

		Private Sub Button1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button1.Click
			If OpenFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
				TextBox1.Text = OpenFileDialog1.FileName
			End If


		End Sub

		Private Sub Button2_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button2.Click
			If OpenFileDialog2.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
				TextBox2.Text = OpenFileDialog2.FileName
			End If
		End Sub

		Private Sub Button3_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles Button3.Click
			If SaveFileDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
                Dim ZipForge1 As ZipForge = New ZipForge
				ZipForge1.FileName = TextBox1.Text
				ZipForge1.SFXStub = TextBox2.Text
				ZipForge1.MakeSFX(SaveFileDialog1.FileName)
			End If

		End Sub
	End Class
End Namespace
