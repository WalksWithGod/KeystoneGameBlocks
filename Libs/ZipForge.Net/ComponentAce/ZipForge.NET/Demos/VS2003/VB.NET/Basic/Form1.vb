Imports ComponentAce.Compression.ZipForge
Imports ComponentAce.Compression.Archiver

Public Class Form1
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.label1 = New System.Windows.Forms.Label
        Me.progressBar1 = New System.Windows.Forms.ProgressBar
        Me.button2 = New System.Windows.Forms.Button
        Me.button1 = New System.Windows.Forms.Button
        Me.ZipForge1 = New ZipForge
        Me.SuspendLayout()
        '
        'label1
        '
        Me.label1.Location = New System.Drawing.Point(35, 5)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(338, 59)
        Me.label1.TabIndex = 7
        Me.label1.Text = "This demo illustrates how to add files and extract files using ZipForge. Source f" & _
        "iles are located in Source folder. Destination archive file will be located in A" & _
        "rchive\test.zip. Files will be extracted to folder Dest."
        '
        'progressBar1
        '
        Me.progressBar1.Location = New System.Drawing.Point(40, 69)
        Me.progressBar1.Name = "progressBar1"
        Me.progressBar1.Size = New System.Drawing.Size(335, 23)
        Me.progressBar1.TabIndex = 6
        '
        'button2
        '
        Me.button2.ForeColor = System.Drawing.SystemColors.ControlText
        Me.button2.Location = New System.Drawing.Point(226, 98)
        Me.button2.Name = "button2"
        Me.button2.TabIndex = 5
        Me.button2.Text = "Exit"
        '
        'button1
        '
        Me.button1.Location = New System.Drawing.Point(112, 98)
        Me.button1.Name = "button1"
        Me.button1.TabIndex = 4
        Me.button1.Text = "Start"
        '
        'ZipForge1
        '
        Me.ZipForge1.Active = False
        Me.ZipForge1.BaseDir = ""
        Me.ZipForge1.CompressionLevel = CompressionLevel.Fastest
        Me.ZipForge1.CompressionMode = CType(1, Byte)
        Me.ZipForge1.ExtractCorruptedFiles = False
        Me.ZipForge1.FileName = Nothing
        Me.ZipForge1.InMemory = False
        Me.ZipForge1.OpenCorruptedArchives = True
        Me.ZipForge1.Password = ""
        Me.ZipForge1.SFXStub = Nothing
        Me.ZipForge1.SpanningMode = SpanningMode.None
        Me.ZipForge1.TempDir = Nothing
        '
        'Form1
        '
        Me.AutoScaleBaseSize = New System.Drawing.Size(5, 13)
        Me.ClientSize = New System.Drawing.Size(409, 126)
        Me.Controls.Add(Me.label1)
        Me.Controls.Add(Me.progressBar1)
        Me.Controls.Add(Me.button2)
        Me.Controls.Add(Me.button1)
        Me.Name = "Form1"
        Me.Text = "ZipForge.NET basic demo (c) ComponentAce 2007"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents progressBar1 As System.Windows.Forms.ProgressBar
    Private WithEvents button2 As System.Windows.Forms.Button
    Private WithEvents button1 As System.Windows.Forms.Button
    Friend WithEvents ZipForge1 As ZipForge


#End Region

    Private Sub button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button1.Click
        Dim DemoFolder As String = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\..\\..\\"
        ' Set archive file name
        ZipForge1.FileName = DemoFolder + "Archive\test.zip"
        ' Create a new archive file
        ZipForge1.OpenArchive(System.IO.FileMode.Create)
        ' Set path to folder with the files to archive
        ZipForge1.BaseDir = DemoFolder + "\Source"
        ' Add all files and directories from the source folder to the archive
        ZipForge1.AddFiles("*.*")
        ' Set path to the destination folder
        ZipForge1.BaseDir = DemoFolder + "\Dest"
        ' extract all files in archive
        ZipForge1.ExtractFiles("*.*")
        ' Close archive
        ZipForge1.CloseArchive()
        MessageBox.Show("All files were added and extracted successfully.")
    End Sub

    Private Sub button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button2.Click
        Close()
    End Sub

    Private Sub ZipForge1_OnOverallProgress(ByVal sender As Object, ByVal progress As Double, ByVal operation As ProcessOperation, ByVal progressPhase As ProgressPhase, ByRef cancel As Boolean) Handles ZipForge1.OnOverallProgress
        progressBar1.Value = progress
    End Sub
End Class
