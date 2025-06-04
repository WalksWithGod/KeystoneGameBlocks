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
      strict private label1: System.Windows.Forms.Label;
      strict private ZipForge1: ZipForge;

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
begin
  Self.label1 := System.Windows.Forms.Label.Create;
  Self.button1 := System.Windows.Forms.Button.Create;
  Self.button2 := System.Windows.Forms.Button.Create;
  Self.ZipForge1 := ZipForge.Create;
  Self.SuspendLayout;
  // 
  // label1
  // 
  Self.label1.Location := System.Drawing.Point.Create(13, 14);
  Self.label1.Name := 'label1';
  Self.label1.Size := System.Drawing.Size.Create(509, 42);
  Self.label1.TabIndex := 0;
  Self.label1.Text := 'This demo shows how to create and open an archive in ' +
  'a stream, and how to add and exctract archived files to streams.';
  // 
  // button1
  // 
  Self.button1.Location := System.Drawing.Point.Create(139, 59);
  Self.button1.Name := 'button1';
  Self.button1.TabIndex := 1;
  Self.button1.Text := 'Start';
  Include(Self.button1.Click, Self.button1_Click);
  // 
  // button2
  // 
  Self.button2.Location := System.Drawing.Point.Create(305, 59);
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
  // TWinForm
  // 
  Self.AutoScaleBaseSize := System.Drawing.Size.Create(5, 13);
  Self.ClientSize := System.Drawing.Size.Create(519, 94);
  Self.Controls.Add(Self.button2);
  Self.Controls.Add(Self.button1);
  Self.Controls.Add(Self.label1);
  Self.Name := 'TWinForm';
  Self.Text := 'ZipForge.NET Streams demo (c) ComponentAce 2007';
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
type
  TByteArray = array of byte;
var
  Stream1, Stream2: System.IO.MemoryStream;
begin
  stream1 := MemoryStream.Create;
  stream2 := MemoryStream.Create;
  self.ZipForge1.OpenArchive(stream1, true);
  stream2.Write(TByteArray.Create( 1, 2, 3, 4 ), 0, 4);
  self.ZipForge1.AddFromStream('testfile.txt', stream2);
  self.ZipForge1.CloseArchive;
  self.ZipForge1.OpenArchive(stream1, false);
  stream2 := nil;
  stream2 := MemoryStream.Create;
  self.ZipForge1.ExtractToStream('testfile.txt', stream2);
  self.ZipForge1.CloseArchive;
  MessageBox.Show('All files were added and extracted successfully')
end;

end.
