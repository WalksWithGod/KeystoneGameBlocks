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
        Me.listView1 = New System.Windows.Forms.ListView
        Me.columnName = New System.Windows.Forms.ColumnHeader
        Me.columnModified = New System.Windows.Forms.ColumnHeader
        Me.columnSize = New System.Windows.Forms.ColumnHeader
        Me.Packed = New System.Windows.Forms.ColumnHeader
        Me.columnRate = New System.Windows.Forms.ColumnHeader
        Me.columnCRC = New System.Windows.Forms.ColumnHeader
        Me.columnPath = New System.Windows.Forms.ColumnHeader
        Me.label1 = New System.Windows.Forms.Label
        Me.button2 = New System.Windows.Forms.Button
        Me.button1 = New System.Windows.Forms.Button
        Me.ZipForge1 = New ZipForge
        Me.SuspendLayout()
        '
        'listView1
        '
        Me.listView1.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
                    Or System.Windows.Forms.AnchorStyles.Left) _
                    Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.listView1.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.columnName, Me.columnModified, Me.columnSize, Me.Packed, Me.columnRate, Me.columnCRC, Me.columnPath})
        Me.listView1.Location = New System.Drawing.Point(2, 49)
        Me.listView1.Name = "listView1"
        Me.listView1.Size = New System.Drawing.Size(521, 182)
        Me.listView1.TabIndex = 8
        Me.listView1.View = System.Windows.Forms.View.Details
        '
        'columnName
        '
        Me.columnName.Text = "Name"
        Me.columnName.Width = 122
        '
        'columnModified
        '
        Me.columnModified.Text = "Modified"
        Me.columnModified.Width = 118
        '
        'columnSize
        '
        Me.columnSize.Text = "Size"
        '
        'Packed
        '
        Me.Packed.Text = "Packed"
        '
        'columnRate
        '
        Me.columnRate.Text = "Rate"
        Me.columnRate.Width = 39
        '
        'columnCRC
        '
        Me.columnCRC.Text = "CRC"
        Me.columnCRC.Width = 55
        '
        'columnPath
        '
        Me.columnPath.Text = "Path"
        '
        'label1
        '
        Me.label1.Location = New System.Drawing.Point(2, 11)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(462, 33)
        Me.label1.TabIndex = 7
        Me.label1.Text = "This demo illustrates how to search files stored inside the archive. Source archi" & _
        "ve file is located in Archive\test.zip"
        '
        'button2
        '
        Me.button2.Location = New System.Drawing.Point(309, 243)
        Me.button2.Name = "button2"
        Me.button2.TabIndex = 6
        Me.button2.Text = "Exit"
        '
        'button1
        '
        Me.button1.Location = New System.Drawing.Point(141, 243)
        Me.button1.Name = "button1"
        Me.button1.TabIndex = 5
        Me.button1.Text = "Search"
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
        Me.ClientSize = New System.Drawing.Size(525, 276)
        Me.Controls.Add(Me.listView1)
        Me.Controls.Add(Me.label1)
        Me.Controls.Add(Me.button2)
        Me.Controls.Add(Me.button1)
        Me.Name = "Form1"
        Me.Text = "ZipForge.NET Search demo (c) ComponentAce 2007"
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents listView1 As System.Windows.Forms.ListView
    Private WithEvents columnName As System.Windows.Forms.ColumnHeader
    Private WithEvents columnModified As System.Windows.Forms.ColumnHeader
    Private WithEvents columnSize As System.Windows.Forms.ColumnHeader
    Private WithEvents Packed As System.Windows.Forms.ColumnHeader
    Private WithEvents columnRate As System.Windows.Forms.ColumnHeader
    Private WithEvents columnCRC As System.Windows.Forms.ColumnHeader
    Private WithEvents columnPath As System.Windows.Forms.ColumnHeader
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents button2 As System.Windows.Forms.Button
    Private WithEvents button1 As System.Windows.Forms.Button
    Friend WithEvents ZipForge1 As ZipForge



#End Region

    Private Sub button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button1.Click
        ZipForge1.FileName = System.IO.Path.GetDirectoryName(Application.ExecutablePath) + "\\..\\..\\Archive\\test.zip"
        ZipForge1.OpenArchive()
        Dim archiveItem As ArchiveItem = New ArchiveItem
        If ZipForge1.FindFirst("*.*", archiveItem) Then
            Do
                Dim listItem As ListViewItem = New ListViewItem
                listItem.Text = archiveItem.FileName
                Dim subItem As ListViewItem.ListViewSubItem = New ListViewItem.ListViewSubItem(listItem, archiveItem.LastWriteTime.ToShortDateString() + " " + archiveItem.LastWriteTime.ToShortTimeString())
                listItem.SubItems.Add(subItem)
                subItem = New ListViewItem.ListViewSubItem(listItem, archiveItem.UncompressedSize.ToString())
                listItem.SubItems.Add(subItem)
                subItem = New ListViewItem.ListViewSubItem(listItem, archiveItem.CompressedSize.ToString())
                listItem.SubItems.Add(subItem)
                subItem = New ListViewItem.ListViewSubItem(listItem, archiveItem.CompressionRate.ToString())
                listItem.SubItems.Add(subItem)
                subItem = New ListViewItem.ListViewSubItem(listItem, (CType(archiveItem.CRC, System.UInt32)).ToString())
                listItem.SubItems.Add(subItem)
                subItem = New ListViewItem.ListViewSubItem(listItem, archiveItem.StoredPath)
                listItem.SubItems.Add(subItem)
                listView1.Items.Add(listItem)
            Loop While ZipForge1.FindNext(archiveItem)
        End If
        ZipForge1.CloseArchive()
    End Sub

    Private Sub button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles button2.Click
        Close()
    End Sub
End Class
