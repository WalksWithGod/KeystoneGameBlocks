Imports ComponentAce.Compression.Archiver
Imports ComponentAce.Compression.ZipForge

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
        Me.button2 = New System.Windows.Forms.Button
        Me.button1 = New System.Windows.Forms.Button
        Me.label1 = New System.Windows.Forms.Label
        Me.ZipForge1 = New ZipForge
        Me.SuspendLayout()
        '
        'button2
        '
        Me.button2.Location = New System.Drawing.Point(247, 72)
        Me.button2.Name = "button2"
        Me.button2.TabIndex = 6
        Me.button2.Text = "Exit"
        '
        'button1
        '
        Me.button1.Location = New System.Drawing.Point(140, 72)
        Me.button1.Name = "button1"
        Me.button1.TabIndex = 5
        Me.button1.Text = "Start"
        '
        'label1
        '
        Me.label1.Location = New System.Drawing.Point(39, 14)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(384, 55)
        Me.label1.TabIndex = 4
        Me.label1.Text = "This demo illustrates how to handle ZIP archives. Source files are located in Sou" & _
        "rce folder. Destination archive file will be located in Archive\test.zip. Files " & _
        "will be extracted to folders Dest and Dest1."
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
        Me.ClientSize = New System.Drawing.Size(463, 109)
        Me.Controls.Add(Me.button2)
        Me.Controls.Add(Me.button1)
        Me.Controls.Add(Me.label1)
        Me.Name = "Form1"
        Me.Text = "ZipForge.NET Advanced Demo (c) ComponentAce 2007"
        Me.ResumeLayout(False)

    End Sub
#End Region
    Private WithEvents button2 As System.Windows.Forms.Button
    Private WithEvents button1 As System.Windows.Forms.Button
    Private WithEvents label1 As System.Windows.Forms.Label
    Friend WithEvents ZipForge1 As ZipForge




    Private Sub button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button1.Click
        Dim DemoFolder As String = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\..\..\"
        System.IO.Directory.SetCurrentDirectory(DemoFolder)

        ' clear Dest folder
        If System.IO.Directory.Exists("Dest") Then
            System.IO.Directory.Delete("Dest", True)
        End If
        System.IO.Directory.CreateDirectory("Dest")

        ZipForge1.FileName = "Archive\test.zip"

        System.IO.File.Copy("Source\1.txt", "Source1\2.txt", True)
        System.IO.File.Copy("Source\uMain.pas", "Source1\2.pas", True)
        System.IO.File.Copy("Source\dummy.mp3", "Source1\dummy2.mp3", True)
        System.IO.Directory.CreateDirectory("Source1\33.txt")
        ' Create a new archive file
        ZipForge1.OpenArchive(System.IO.FileMode.Create)
        ' Let's encrypt all files
        ZipForge1.Password = "The password"
        ' Set path to folder with some text files to BaseDir
        ZipForge1.BaseDir = "Source"
        ' Do not compress MPEG3 files
        ZipForge1.NoCompressionMasks.Add("*.mp3")
        ' Add all files and directories from Source excluding text files to the archive
        ZipForge1.AddFiles("*.*", System.IO.FileAttributes.Archive Or System.IO.FileAttributes.Normal Or System.IO.FileAttributes.Directory, "*.txt")
        ' Set path to destination folder            
        ZipForge1.BaseDir = "Dest"
        ' Extract all files and directories from the archive to BaseDir
        ' After extracting directory Dest should contain all files from folder
        ' Source excluding *.txt files
        ZipForge1.ExtractFiles("*.*")
        ' Use full path
        ZipForge1.Options.StorePath = StorePathMode.FullPath
        ' Set path to destination folder
        ZipForge1.BaseDir = "Source1"
        ' Move all text files from Source1 to the archive
        ' After moving directory Source1 should not contain any text files
        ZipForge1.MoveFiles("*.txt", System.IO.FileAttributes.Normal Or System.IO.FileAttributes.Archive)
        ' Set path to current drive
        ZipForge1.BaseDir = System.IO.Directory.GetDirectoryRoot(System.IO.Directory.GetCurrentDirectory())
        ' Overwrite all files
        ZipForge1.Options.Overwrite = OverwriteMode.Always
        ' Update all files excluding 1???.t* from Source1
        ZipForge1.UpdateFiles(DemoFolder + "\Source1\*.*", System.IO.FileAttributes.Archive Or System.IO.FileAttributes.Normal, "2???.t*")
        ' Set temporary directory
        ZipForge1.TempDir = DemoFolder + "Temp"
        ' Test all files and directories in the archive
        Try
            ZipForge1.TestFiles("*.*")
        Catch
            MessageBox.Show("Archive is corrupted")
        End Try

        ' Use full path
        ZipForge1.Options.StorePath = StorePathMode.RelativePath
        ZipForge1.BaseDir = "Dest1"
        ' Extract all files to Dest1
        ZipForge1.ExtractFiles("*.*")
        ' Close the archive
        ZipForge1.CloseArchive()
        MessageBox.Show("All files were added and extracted successfully.")


    End Sub

    Private Sub button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button2.Click
        Close()
    End Sub
End Class
