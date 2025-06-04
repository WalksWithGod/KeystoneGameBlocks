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
        Me.Label1 = New System.Windows.Forms.Label
        Me.Start = New System.Windows.Forms.Button
        Me.Button1 = New System.Windows.Forms.Button
        Me.ZipForge1 = New ZipForge
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.Location = New System.Drawing.Point(10, 14)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(457, 40)
        Me.Label1.TabIndex = 5
        Me.Label1.Text = "This demo illustrates how to use a transaction system. Source files are located i" & _
        "n Source and Source1 folder. Destination archive file will be located in Archive" & _
        "\test.zip. Files will be extracted to folder Dest."
        '
        'Start
        '
        Me.Start.BackColor = System.Drawing.SystemColors.Control
        Me.Start.Location = New System.Drawing.Point(122, 70)
        Me.Start.Name = "Start"
        Me.Start.Size = New System.Drawing.Size(75, 25)
        Me.Start.TabIndex = 4
        Me.Start.Text = "Start"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(267, 71)
        Me.Button1.Name = "Button1"
        Me.Button1.TabIndex = 6
        Me.Button1.Text = "Exit"
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
        Me.ClientSize = New System.Drawing.Size(477, 109)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.Start)
        Me.Name = "Form1"
        Me.Text = "ZipForge.NET Transaction System demo (c) ComponentAce 2007"
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents Label1 As System.Windows.Forms.Label
    Public WithEvents Start As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents ZipForge1 As ZipForge


#End Region

    Private Sub Start_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Start.Click
        Dim DemoPath As String = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\..\\..\\"
        ZipForge1.FileName = DemoPath + "Archive\test.zip"
        ' Create a new archive file            
        ZipForge1.OpenArchive(System.IO.FileMode.Create)
        ' Start a transaction
        ZipForge1.BeginUpdate()
        ' Set path to folder with some HTML files to BaseDir
        ZipForge1.BaseDir = DemoPath + "Source"
        ' Add all files from Source folder to the archive
        Try

            ZipForge1.AddFiles("*.*")
        Catch
            ' If errors occurs rollback transaction. All modifications will be cancelled.            
            ZipForge1.CancelUpdate()
            ' Close archive and exit current procedure                
            ZipForge1.CloseArchive()
            MessageBox.Show("Error adding all files")
            Return
        End Try
        ' Set path to folder with some HTML files to BaseDir            
        ZipForge1.BaseDir = DemoPath + "Source1\\"
        ' Add all HTML files from Source1 folder to the archive
        Try
            ZipForge1.AddFiles("*.htm*")
        Catch
            ' If errors occurs rollback transaction. All modifications will be cancelled.                
            ZipForge1.CancelUpdate()
            ' Close archive and exit current procedure                
            ZipForge1.CloseArchive()
            MessageBox.Show("Error adding html files")
            Return
        End Try
        ' Commit a transaction. All modifications will be saved.            
        ZipForge1.EndUpdate()
        ' Set path to destination folder            
        ZipForge1.BaseDir = DemoPath + "Dest"
        ' Extract all files            
        ZipForge1.ExtractFiles("*.*")
        ' Close the archive            
        ZipForge1.CloseArchive()
        MessageBox.Show("All files were added and extracted successfully.")

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Close()
    End Sub
End Class
