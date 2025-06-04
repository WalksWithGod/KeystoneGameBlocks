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
        Me.button2 = New System.Windows.Forms.Button
        Me.button1 = New System.Windows.Forms.Button
        Me.label1 = New System.Windows.Forms.Label
        Me.ZipForge1 = New ZipForge
        Me.SuspendLayout()
        '
        'button2
        '
        Me.button2.Location = New System.Drawing.Point(297, 58)
        Me.button2.Name = "button2"
        Me.button2.TabIndex = 5
        Me.button2.Text = "Exit"
        '
        'button1
        '
        Me.button1.Location = New System.Drawing.Point(131, 58)
        Me.button1.Name = "button1"
        Me.button1.TabIndex = 4
        Me.button1.Text = "Start"
        '
        'label1
        '
        Me.label1.Location = New System.Drawing.Point(5, 13)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(509, 42)
        Me.label1.TabIndex = 3
        Me.label1.Text = "This demo shows how to create and open an archive in a stream, and how to add and" & _
        " exctract archived files to streams."
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
        Me.ClientSize = New System.Drawing.Size(519, 94)
        Me.Controls.Add(Me.button2)
        Me.Controls.Add(Me.button1)
        Me.Controls.Add(Me.label1)
        Me.Name = "Form1"
        Me.Text = "ZipForge.NET Streams demo (c) ComponentAce 2007"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents button2 As System.Windows.Forms.Button
    Private WithEvents button1 As System.Windows.Forms.Button
    Private WithEvents label1 As System.Windows.Forms.Label
    Friend WithEvents ZipForge1 As ZipForge


#End Region

    Private Sub button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button1.Click
        Dim archiveStream As System.IO.MemoryStream = New System.IO.MemoryStream
        Dim itemStream As System.IO.MemoryStream = New System.IO.MemoryStream
        ' Create archive in stream
        ZipForge1.OpenArchive(archiveStream, True)
        ' Add item from stream            
        Dim A(4) As Byte
        A(0) = 1
        A(1) = 2
        A(2) = 3
        A(3) = 4
        itemStream.Write(A, 0, 4)
        ZipForge1.AddFromStream("testfile.txt", itemStream)
        ' Close arhive
        ZipForge1.CloseArchive()
        ' Open archive in stream
        ZipForge1.OpenArchive(archiveStream, False)
        ' extract item to stream
        itemStream = Nothing
        itemStream = New System.IO.MemoryStream
        ZipForge1.ExtractToStream("testfile.txt", itemStream)
        ' Close archive
        ZipForge1.CloseArchive()
        MessageBox.Show("All files were added and extracted successfully")

    End Sub

    Private Sub button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button2.Click
        Close()
    End Sub
End Class
