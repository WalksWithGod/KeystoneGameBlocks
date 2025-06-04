Imports Microsoft.VisualBasic
Imports System
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Windows.Forms
Imports System.Data
Imports ComponentAce.Compression.ZipForge
Imports ComponentAce.Compression.Archiver


Namespace Spanning
	''' <summary>
	''' Summary description for Form1.
	''' </summary>
	Public Class Form1
		Inherits System.Windows.Forms.Form
		Private tabControl1 As System.Windows.Forms.TabControl
		Private tabPage1 As System.Windows.Forms.TabPage
		Private label1 As System.Windows.Forms.Label
		Private label2 As System.Windows.Forms.Label
		Private tabPage2 As System.Windows.Forms.TabPage
		Private label3 As System.Windows.Forms.Label
		Private label4 As System.Windows.Forms.Label
		Private WithEvents button7 As System.Windows.Forms.Button
		Private WithEvents createSplittingButton As System.Windows.Forms.Button
		Private WithEvents createSpanningButton As System.Windows.Forms.Button
		Private WithEvents btnNewArchiveFile As System.Windows.Forms.Button
		Private WithEvents btnSourceFiles As System.Windows.Forms.Button
		Private tbNewArchiveFile As System.Windows.Forms.TextBox
		Private tbSourceFiles As System.Windows.Forms.TextBox
		Private WithEvents btnExtractTo As System.Windows.Forms.Button
		Private WithEvents btnArchiveFile As System.Windows.Forms.Button
		Private tbExtractTo As System.Windows.Forms.TextBox
		Private tbArchiveFile As System.Windows.Forms.TextBox
		Private ofdSourceFiles As System.Windows.Forms.OpenFileDialog
		Private sfdNewArchiveFile As System.Windows.Forms.SaveFileDialog
		Private pbProgress As System.Windows.Forms.ProgressBar
		Private ofdArchiveFile As System.Windows.Forms.OpenFileDialog
		Private fbdExtractTo As System.Windows.Forms.FolderBrowserDialog
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
            Me.tabControl1 = New System.Windows.Forms.TabControl
            Me.tabPage1 = New System.Windows.Forms.TabPage
            Me.createSplittingButton = New System.Windows.Forms.Button
            Me.createSpanningButton = New System.Windows.Forms.Button
            Me.btnNewArchiveFile = New System.Windows.Forms.Button
            Me.btnSourceFiles = New System.Windows.Forms.Button
            Me.label2 = New System.Windows.Forms.Label
            Me.label1 = New System.Windows.Forms.Label
            Me.tbNewArchiveFile = New System.Windows.Forms.TextBox
            Me.tbSourceFiles = New System.Windows.Forms.TextBox
            Me.tabPage2 = New System.Windows.Forms.TabPage
            Me.button7 = New System.Windows.Forms.Button
            Me.btnExtractTo = New System.Windows.Forms.Button
            Me.btnArchiveFile = New System.Windows.Forms.Button
            Me.label3 = New System.Windows.Forms.Label
            Me.label4 = New System.Windows.Forms.Label
            Me.tbExtractTo = New System.Windows.Forms.TextBox
            Me.tbArchiveFile = New System.Windows.Forms.TextBox
            Me.pbProgress = New System.Windows.Forms.ProgressBar
            Me.ofdSourceFiles = New System.Windows.Forms.OpenFileDialog
            Me.sfdNewArchiveFile = New System.Windows.Forms.SaveFileDialog
            Me.ofdArchiveFile = New System.Windows.Forms.OpenFileDialog
            Me.fbdExtractTo = New System.Windows.Forms.FolderBrowserDialog
            Me.tabControl1.SuspendLayout()
            Me.tabPage1.SuspendLayout()
            Me.tabPage2.SuspendLayout()
            Me.SuspendLayout()
            '
            'tabControl1
            '
            Me.tabControl1.Controls.Add(Me.tabPage1)
            Me.tabControl1.Controls.Add(Me.tabPage2)
            Me.tabControl1.Dock = System.Windows.Forms.DockStyle.Top
            Me.tabControl1.Location = New System.Drawing.Point(0, 0)
            Me.tabControl1.Name = "tabControl1"
            Me.tabControl1.SelectedIndex = 0
            Me.tabControl1.Size = New System.Drawing.Size(416, 208)
            Me.tabControl1.TabIndex = 0
            '
            'tabPage1
            '
            Me.tabPage1.Controls.Add(Me.createSplittingButton)
            Me.tabPage1.Controls.Add(Me.createSpanningButton)
            Me.tabPage1.Controls.Add(Me.btnNewArchiveFile)
            Me.tabPage1.Controls.Add(Me.btnSourceFiles)
            Me.tabPage1.Controls.Add(Me.label2)
            Me.tabPage1.Controls.Add(Me.label1)
            Me.tabPage1.Controls.Add(Me.tbNewArchiveFile)
            Me.tabPage1.Controls.Add(Me.tbSourceFiles)
            Me.tabPage1.Location = New System.Drawing.Point(4, 22)
            Me.tabPage1.Name = "tabPage1"
            Me.tabPage1.Size = New System.Drawing.Size(408, 182)
            Me.tabPage1.TabIndex = 0
            Me.tabPage1.Text = "Create archive"
            '
            'createSplittingButton
            '
            Me.createSplittingButton.Location = New System.Drawing.Point(40, 120)
            Me.createSplittingButton.Name = "createSplittingButton"
            Me.createSplittingButton.Size = New System.Drawing.Size(144, 23)
            Me.createSplittingButton.TabIndex = 7
            Me.createSplittingButton.Text = "Create splitting archive"
            '
            'createSpanningButton
            '
            Me.createSpanningButton.Location = New System.Drawing.Point(192, 120)
            Me.createSpanningButton.Name = "createSpanningButton"
            Me.createSpanningButton.Size = New System.Drawing.Size(152, 23)
            Me.createSpanningButton.TabIndex = 6
            Me.createSpanningButton.Text = "Create spanning archive"
            '
            'btnNewArchiveFile
            '
            Me.btnNewArchiveFile.Location = New System.Drawing.Point(312, 72)
            Me.btnNewArchiveFile.Name = "btnNewArchiveFile"
            Me.btnNewArchiveFile.TabIndex = 5
            Me.btnNewArchiveFile.Text = "Browse..."
            '
            'btnSourceFiles
            '
            Me.btnSourceFiles.Location = New System.Drawing.Point(312, 24)
            Me.btnSourceFiles.Name = "btnSourceFiles"
            Me.btnSourceFiles.TabIndex = 4
            Me.btnSourceFiles.Text = "Browse..."
            '
            'label2
            '
            Me.label2.AutoSize = True
            Me.label2.Location = New System.Drawing.Point(8, 80)
            Me.label2.Name = "label2"
            Me.label2.Size = New System.Drawing.Size(60, 16)
            Me.label2.TabIndex = 3
            Me.label2.Text = "Archive file"
            '
            'label1
            '
            Me.label1.AutoSize = True
            Me.label1.Location = New System.Drawing.Point(8, 32)
            Me.label1.Name = "label1"
            Me.label1.Size = New System.Drawing.Size(63, 16)
            Me.label1.TabIndex = 2
            Me.label1.Text = "Source files"
            '
            'tbNewArchiveFile
            '
            Me.tbNewArchiveFile.Location = New System.Drawing.Point(80, 72)
            Me.tbNewArchiveFile.Name = "tbNewArchiveFile"
            Me.tbNewArchiveFile.Size = New System.Drawing.Size(216, 20)
            Me.tbNewArchiveFile.TabIndex = 1
            Me.tbNewArchiveFile.Text = ""
            '
            'tbSourceFiles
            '
            Me.tbSourceFiles.Location = New System.Drawing.Point(80, 24)
            Me.tbSourceFiles.Name = "tbSourceFiles"
            Me.tbSourceFiles.Size = New System.Drawing.Size(216, 20)
            Me.tbSourceFiles.TabIndex = 0
            Me.tbSourceFiles.Text = ""
            '
            'tabPage2
            '
            Me.tabPage2.Controls.Add(Me.button7)
            Me.tabPage2.Controls.Add(Me.btnExtractTo)
            Me.tabPage2.Controls.Add(Me.btnArchiveFile)
            Me.tabPage2.Controls.Add(Me.label3)
            Me.tabPage2.Controls.Add(Me.label4)
            Me.tabPage2.Controls.Add(Me.tbExtractTo)
            Me.tabPage2.Controls.Add(Me.tbArchiveFile)
            Me.tabPage2.Location = New System.Drawing.Point(4, 22)
            Me.tabPage2.Name = "tabPage2"
            Me.tabPage2.Size = New System.Drawing.Size(408, 182)
            Me.tabPage2.TabIndex = 1
            Me.tabPage2.Text = "Extract files from archive"
            '
            'button7
            '
            Me.button7.Location = New System.Drawing.Point(128, 120)
            Me.button7.Name = "button7"
            Me.button7.Size = New System.Drawing.Size(128, 24)
            Me.button7.TabIndex = 12
            Me.button7.Text = "Extract files"
            '
            'btnExtractTo
            '
            Me.btnExtractTo.Location = New System.Drawing.Point(312, 72)
            Me.btnExtractTo.Name = "btnExtractTo"
            Me.btnExtractTo.TabIndex = 11
            Me.btnExtractTo.Text = "Browse..."
            '
            'btnArchiveFile
            '
            Me.btnArchiveFile.Location = New System.Drawing.Point(312, 24)
            Me.btnArchiveFile.Name = "btnArchiveFile"
            Me.btnArchiveFile.TabIndex = 10
            Me.btnArchiveFile.Text = "Browse..."
            '
            'label3
            '
            Me.label3.AutoSize = True
            Me.label3.Location = New System.Drawing.Point(8, 80)
            Me.label3.Name = "label3"
            Me.label3.Size = New System.Drawing.Size(52, 16)
            Me.label3.TabIndex = 9
            Me.label3.Text = "Extract to"
            '
            'label4
            '
            Me.label4.AutoSize = True
            Me.label4.Location = New System.Drawing.Point(8, 32)
            Me.label4.Name = "label4"
            Me.label4.Size = New System.Drawing.Size(60, 16)
            Me.label4.TabIndex = 8
            Me.label4.Text = "Archive file"
            '
            'tbExtractTo
            '
            Me.tbExtractTo.Location = New System.Drawing.Point(80, 72)
            Me.tbExtractTo.Name = "tbExtractTo"
            Me.tbExtractTo.Size = New System.Drawing.Size(216, 20)
            Me.tbExtractTo.TabIndex = 7
            Me.tbExtractTo.Text = ""
            '
            'tbArchiveFile
            '
            Me.tbArchiveFile.Location = New System.Drawing.Point(80, 24)
            Me.tbArchiveFile.Name = "tbArchiveFile"
            Me.tbArchiveFile.Size = New System.Drawing.Size(216, 20)
            Me.tbArchiveFile.TabIndex = 6
            Me.tbArchiveFile.Text = ""
            '
            'pbProgress
            '
            Me.pbProgress.Dock = System.Windows.Forms.DockStyle.Bottom
            Me.pbProgress.Location = New System.Drawing.Point(0, 205)
            Me.pbProgress.Name = "pbProgress"
            Me.pbProgress.Size = New System.Drawing.Size(416, 16)
            Me.pbProgress.TabIndex = 1
            '
            'sfdNewArchiveFile
            '
            Me.sfdNewArchiveFile.DefaultExt = "*.zip"
            Me.sfdNewArchiveFile.Filter = "ZIP archives|*.zip"
            '
            'ofdArchiveFile
            '
            Me.ofdArchiveFile.Filter = "ZIP archives|*.zip"
            '
            'Form1
            '
            Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
            Me.ClientSize = New System.Drawing.Size(416, 221)
            Me.Controls.Add(Me.pbProgress)
            Me.Controls.Add(Me.tabControl1)
            Me.Name = "Form1"
            Me.Text = "Spanning demo (C) ComponentAce 2007"
            Me.tabControl1.ResumeLayout(False)
            Me.tabPage1.ResumeLayout(False)
            Me.tabPage2.ResumeLayout(False)
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



		Private Sub button7_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles button7.Click
			' create ZipForge object
            Dim ZipForge1 As ZipForge = New ZipForge
			' set file name
            ZipForge1.FileName = tbArchiveFile.Text
			' open archive
            ZipForge1.OpenArchive()
			' set base directory
            ZipForge1.BaseDir = tbExtractTo.Text
			' set OnOverallProgress event
            AddHandler ZipForge1.OnOverallProgress, AddressOf zf_OnOverallProgress
			' extract all files
            ZipForge1.ExtractFiles("*.*")
			' close archive
            ZipForge1.CloseArchive()
		End Sub


		Private Sub createArchive_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles createSplittingButton.Click, createSpanningButton.Click
			' create ZipForge object
            Dim ZipForge1 As ZipForge = New ZipForge
			' set OnOverallProgress event
			AddHandler ZipForge1.OnOverallProgress, AddressOf zf_OnOverallProgress

			' specify spanning mode
			If (CType(IIf(TypeOf sender Is Button, sender, Nothing), Button)).Name = createSpanningButton.Name Then
                ZipForge1.SpanningMode = SpanningMode.Spanning
			Else
                ZipForge1.SpanningMode = SpanningMode.Splitting
			End If
			' set volume size to 1.44 Mb			
            ZipForge1.SpanningOptions.VolumeSize = VolumeSize.Disk1_44MB

			' code to set custom volume size:
            ' zf.SpanningOptions.VolumeSize = VolumeSize.Custom;
			' zf.SpanningOptions.CustomVolumeSize = 1024*1024; // 1Mb 

			' set archive file 
			ZipForge1.FileName = tbNewArchiveFile.Text
			' create a new archive
			ZipForge1.OpenArchive(System.IO.FileMode.Create)
			' set base directory to the folder containing selected file
			ZipForge1.BaseDir = System.IO.Path.GetDirectoryName(tbSourceFiles.Text)
			' add file(s)
			ZipForge1.AddFiles(tbSourceFiles.Text)
			' close archive
			ZipForge1.CloseArchive()
		End Sub

		Private Sub btnSourceFiles_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnSourceFiles.Click
			If ofdSourceFiles.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
				tbSourceFiles.Text = ofdSourceFiles.FileName
			End If
		End Sub

		Private Sub btnNewArchiveFile_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnNewArchiveFile.Click
			If sfdNewArchiveFile.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
				tbNewArchiveFile.Text = sfdNewArchiveFile.FileName
			End If
		End Sub


		Private Sub btnArchiveFile_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnArchiveFile.Click
			If ofdArchiveFile.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
				tbArchiveFile.Text = ofdArchiveFile.FileName
			End If
		End Sub

		Private Sub btnExtractTo_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnExtractTo.Click
			If fbdExtractTo.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
				 tbExtractTo.Text = fbdExtractTo.SelectedPath
			End If
		End Sub

        Private Sub zf_OnOverallProgress(ByVal sender As Object, ByVal progress As Double, ByVal operation As ProcessOperation, ByVal progressPhase As ProgressPhase, ByRef cancel As Boolean)
            pbProgress.Value = CInt(Fix(progress))
        End Sub
    End Class
End Namespace
