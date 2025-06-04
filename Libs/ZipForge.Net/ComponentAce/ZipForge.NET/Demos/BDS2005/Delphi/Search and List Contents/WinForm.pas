unit WinForm;

interface

uses
  System.Drawing, System.Collections, System.ComponentModel, System.IO,
  System.Windows.Forms, System.Data, ComponentAce.Compression.ZipForge,
  ComponentAce.Compression.Archiver;

type
  TWinForm = class(System.Windows.Forms.Form)
  {$REGION 'Designer Managed Code'}
  strict private
    /// <summary>
    /// Required designer variable.
    /// </summary>
    Components: System.ComponentModel.Container;
    procedure button1_Click(sender: System.Object; e: System.EventArgs);
    procedure button2_Click(sender: System.Object; e: System.EventArgs);
    strict private button1: System.Windows.Forms.Button;
    strict private button2: System.Windows.Forms.Button;
    strict private columnCRC: System.Windows.Forms.ColumnHeader;
    strict private columnModified: System.Windows.Forms.ColumnHeader;
    strict private columnName: System.Windows.Forms.ColumnHeader;
    strict private columnPath: System.Windows.Forms.ColumnHeader;
    strict private columnRate: System.Windows.Forms.ColumnHeader;
    strict private columnSize: System.Windows.Forms.ColumnHeader;
    strict private label1: System.Windows.Forms.Label;
    strict private listView1: System.Windows.Forms.ListView;
    strict private PackedSize: System.Windows.Forms.ColumnHeader;
    strict private ZipForge1: ComponentAce.Compression.ZipForge.ZipForge;


    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    procedure InitializeComponent;
  {$ENDREGION}
  strict protected
    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    procedure Dispose(Disposing: Boolean); override;
  private
    { Private Declarations }
  public
    constructor Create;
  end;

  [assembly: RuntimeRequiredAttribute(TypeOf(TWinForm))]

implementation

{$AUTOBOX ON}

{$REGION 'Windows Form Designer generated code'}
/// <summary>
/// Required method for Designer support -- do not modify
/// the contents of this method with the code editor.
/// </summary>
procedure TWinForm.InitializeComponent;
type
  TArrayOfSystem_Windows_Forms_ColumnHeader = array of System.Windows.Forms.ColumnHeader;
begin
  Self.button1 := System.Windows.Forms.Button.Create;
  Self.button2 := System.Windows.Forms.Button.Create;
  Self.ZipForge1 := ComponentAce.Compression.ZipForge.ZipForge.Create;
  Self.label1 := System.Windows.Forms.Label.Create;
  Self.listView1 := System.Windows.Forms.ListView.Create;
  Self.columnName := System.Windows.Forms.ColumnHeader.Create;
  Self.columnModified := System.Windows.Forms.ColumnHeader.Create;
  Self.columnSize := System.Windows.Forms.ColumnHeader.Create;
  Self.PackedSize := System.Windows.Forms.ColumnHeader.Create;
  Self.columnRate := System.Windows.Forms.ColumnHeader.Create;
  Self.columnCRC := System.Windows.Forms.ColumnHeader.Create;
  Self.columnPath := System.Windows.Forms.ColumnHeader.Create;
  Self.SuspendLayout;
  // 
  // button1
  // 
  Self.button1.Location := System.Drawing.Point.Create(151, 241);
  Self.button1.Name := 'button1';
  Self.button1.TabIndex := 1;
  Self.button1.Text := 'Search';
  Include(Self.button1.Click, Self.button1_Click);
  // 
  // button2
  // 
  Self.button2.Location := System.Drawing.Point.Create(319, 241);
  Self.button2.Name := 'button2';
  Self.button2.TabIndex := 2;
  Self.button2.Text := 'Exit';
  Include(Self.button2.Click, Self.button2_Click);
  // 
  // ZipForge1
  // 
  Self.ZipForge1.Active := False;
  Self.ZipForge1.BaseDir := '';
  Self.ZipForge1.CompressionLevel := CompressionLevel.Fastest;
  Self.ZipForge1.CompressionMode := (Byte(1));
  Self.ZipForge1.ExtractCorruptedFiles := False;
  Self.ZipForge1.FileName := nil;
  Self.ZipForge1.InMemory := False;
  Self.ZipForge1.OpenCorruptedArchives := True;
  Self.ZipForge1.Password := '';
  Self.ZipForge1.SFXStub := nil;
  Self.ZipForge1.SpanningMode := SpanningMode.None;
  Self.ZipForge1.TempDir := nil;
  // 
  // label1
  // 
  Self.label1.Location := System.Drawing.Point.Create(12, 9);
  Self.label1.Name := 'label1';
  Self.label1.Size := System.Drawing.Size.Create(462, 33);
  Self.label1.TabIndex := 3;
  Self.label1.Text := 'This demo illustrates how to search files stored insi' +
  'de the archive. Source archive file is located in Archive\test.zip';
  // 
  // listView1
  // 
  Self.listView1.Anchor := (System.Windows.Forms.AnchorStyles((((System.Windows.Forms.AnchorStyles.Top 
    or System.Windows.Forms.AnchorStyles.Bottom) or System.Windows.Forms.AnchorStyles.Left) 
    or System.Windows.Forms.AnchorStyles.Right)));
  Self.listView1.Columns.AddRange(TArrayOfSystem_Windows_Forms_ColumnHeader.Create(Self.columnName, 
          Self.columnModified, Self.columnSize, Self.PackedSize, Self.columnRate, 
          Self.columnCRC, Self.columnPath));
  Self.listView1.Location := System.Drawing.Point.Create(12, 47);
  Self.listView1.Name := 'listView1';
  Self.listView1.Size := System.Drawing.Size.Create(521, 182);
  Self.listView1.TabIndex := 4;
  Self.listView1.View := System.Windows.Forms.View.Details;
  // 
  // columnName
  // 
  Self.columnName.Text := 'Name';
  Self.columnName.Width := 122;
  // 
  // columnModified
  // 
  Self.columnModified.Text := 'Modified';
  Self.columnModified.Width := 118;
  // 
  // columnSize
  // 
  Self.columnSize.Text := 'Size';
  // 
  // PackedSize
  // 
  Self.PackedSize.Text := 'Packed';
  // 
  // columnRate
  // 
  Self.columnRate.Text := 'Rate';
  Self.columnRate.Width := 39;
  // 
  // columnCRC
  // 
  Self.columnCRC.Text := 'CRC';
  Self.columnCRC.Width := 55;
  // 
  // columnPath
  // 
  Self.columnPath.Text := 'Path';
  // 
  // TWinForm
  // 
  Self.AutoScaleBaseSize := System.Drawing.Size.Create(5, 13);
  Self.ClientSize := System.Drawing.Size.Create(545, 276);
  Self.Controls.Add(Self.listView1);
  Self.Controls.Add(Self.label1);
  Self.Controls.Add(Self.button2);
  Self.Controls.Add(Self.button1);
  Self.Name := 'TWinForm';
  Self.Text := 'ZipForge.NET Search demo (c) ComponentAce 2007';
  Self.ResumeLayout(False);
end;
{$ENDREGION}

procedure TWinForm.Dispose(Disposing: Boolean);
begin
  if Disposing then
  begin
    if Components <> nil then
      Components.Dispose();
  end;
  inherited Dispose(Disposing);
end;

constructor TWinForm.Create;
begin
  inherited Create;
  //
  // Required for Windows Form Designer support
  //
  InitializeComponent;
  //
  // TODO: Add any constructor code after InitializeComponent call
  //
end;

procedure TWinForm.button2_Click(sender: System.Object; e: System.EventArgs);
begin
  Close;
end;

procedure TWinForm.button1_Click(sender: System.Object; e: System.EventArgs);
var
  Item: ArchiveItem;
  LVItem: System.Windows.Forms.ListViewItem;
  LVSubItem: ListViewItem.ListViewSubItem;
begin
  self.ZipForge1.FileName := System.IO.Path.GetDirectoryName(Application.ExecutablePath) + '\Archive\Test.zip';
  self.ZipForge1.OpenArchive;
  Item := ArchiveItem.Create;
  if (self.ZipForge1.FindFirst('*.*', Item)) then
    repeat
      LVItem := ListViewItem.Create;
      LVItem.Text := Item.FileName;
      LVSubItem := ListViewItem.ListViewSubItem.Create(LVItem, Concat(Item.LastWriteTime.ToShortDateString, ' ', Item.LastWriteTime.ToShortTimeString));
      LVItem.SubItems.Add(LVSubItem);
      LVSubItem := ListViewItem.ListViewSubItem.Create(LVItem, Item.UncompressedSize.ToString);
      LVItem.SubItems.Add(LVSubItem);
      LVSubItem := ListViewItem.ListViewSubItem.Create(LVItem, Item.CompressedSize.ToString);
      LVItem.SubItems.Add(LVSubItem);
      LVSubItem := ListViewItem.ListViewSubItem.Create(LVItem, Item.CompressionRate.ToString);
      LVItem.SubItems.Add(LVSubItem);
      LVSubItem := ListViewItem.ListViewSubItem.Create(LVItem, Item.CRC.ToString);
      LVItem.SubItems.Add(LVSubItem);
      LVSubItem := ListViewItem.ListViewSubItem.Create(LVItem, Item.StoredPath);
      LVItem.SubItems.Add(LVSubItem);
      self.listView1.Items.Add(LVItem)
    until not self.ZipForge1.FindNext(Item);
  self.ZipForge1.CloseArchive
end;

end.
