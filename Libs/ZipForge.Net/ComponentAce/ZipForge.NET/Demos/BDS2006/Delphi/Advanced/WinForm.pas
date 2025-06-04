unit WinForm;

interface

uses
  System.Drawing, System.Collections, System.ComponentModel,
  System.Windows.Forms, System.Data, System.IO, 
  ComponentAce.Compression.ZipForge, ComponentAce.Compression.Archiver;

type
  TWinForm = class(System.Windows.Forms.Form)
  {$REGION 'Designer Managed Code'}
  strict private
    /// <summary>
    /// Required designer variable.
    /// </summary>
    Components: System.ComponentModel.Container;
        button1: System.Windows.Forms.Button;
    button2: System.Windows.Forms.Button;
    label1: System.Windows.Forms.Label;
    ZipForge1: ComponentAce.Compression.ZipForge.ZipForge;
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    procedure InitializeComponent;
    procedure button1_Click(sender: System.Object; e: System.EventArgs);
    procedure button2_Click(sender: System.Object; e: System.EventArgs);
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
  Self.ZipForge1 := ComponentAce.Compression.ZipForge.ZipForge.Create;
  Self.SuspendLayout;
  // 
  // label1
  // 
  Self.label1.Location := System.Drawing.Point.Create(40, 20);
  Self.label1.Name := 'label1';
  Self.label1.Size := System.Drawing.Size.Create(384, 55);
  Self.label1.TabIndex := 0;
  Self.label1.Text := 'This demo illustrates how to handle ZIP archives. Sou' +
  'rce files are located in Source folder. Destination archive file will be ' +
  'located in Archive\test.zip. Files will be extracted to folders Dest and ' +
  'Dest1.';
  // 
  // button1
  // 
  Self.button1.Location := System.Drawing.Point.Create(140, 78);
  Self.button1.Name := 'button1';
  Self.button1.TabIndex := 2;
  Self.button1.Text := 'Start';
  Include(Self.button1.Click, Self.button1_Click);
  // 
  // button2
  // 
  Self.button2.Location := System.Drawing.Point.Create(247, 78);
  Self.button2.Name := 'button2';
  Self.button2.TabIndex := 3;
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
  Self.ClientSize := System.Drawing.Size.Create(463, 109);
  Self.Controls.Add(Self.button2);
  Self.Controls.Add(Self.button1);
  Self.Controls.Add(Self.label1);
  Self.Name := 'TWinForm';
  Self.Text := 'ZipForge.NET Advanced Demo (c) ComponentAce 2007';
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
  text1: string;
begin
  text1 := Concat(System.IO.Path.GetDirectoryName(Application.ExecutablePath));
  Directory.SetCurrentDirectory(text1);
  if (Directory.Exists('Dest')) then
        Directory.Delete('Dest', true);
  Directory.CreateDirectory('Dest');
  self.ZipForge1.FileName := 'Archive\test.zip';
  System.IO.File.Copy('Source\1.txt', 'Source1\2.txt', true);
  System.IO.File.Copy('Source\uMain.pas', 'Source1\2.pas', true);
  System.IO.File.Copy('Source\dummy.mp3', 'Source1\dummy2.mp3', true);
  Directory.CreateDirectory('Source1\33.txt');
  self.ZipForge1.OpenArchive(FileMode.Create);
  self.ZipForge1.Password := 'The password';
  self.ZipForge1.BaseDir := 'Source';
  self.ZipForge1.NoCompressionMasks.Add('*.mp3');
  self.ZipForge1.AddFiles('*.*', (FileAttributes.Normal or (FileAttributes.Archive or FileAttributes.Directory)), '*.txt');
  self.ZipForge1.BaseDir := 'Dest';
  self.ZipForge1.ExtractFiles('*.*');
  self.ZipForge1.Options.StorePath := StorePathMode.FullPath;
  self.ZipForge1.BaseDir := 'Source1';
  self.ZipForge1.MoveFiles('*.txt', (FileAttributes.Normal or FileAttributes.Archive));
  self.ZipForge1.BaseDir := Directory.GetDirectoryRoot(Directory.GetCurrentDirectory);
  self.ZipForge1.Options.Overwrite := OverwriteMode.Always;
  self.ZipForge1.UpdateFiles(Concat(text1, '\Source1\*.*'), (FileAttributes.Normal or FileAttributes.Archive), '2???.t*');
  self.ZipForge1.TempDir := 'Temp';
  try
        self.ZipForge1.TestFiles('*.*')
  except
        on obj1: TObject do
              MessageBox.Show('Archive is corrupted')
  end;
  self.ZipForge1.Options.StorePath := StorePathMode.RelativePath;
  self.ZipForge1.BaseDir := 'Dest1';
  self.ZipForge1.ExtractFiles('*.*');
  self.ZipForge1.CloseArchive;
  MessageBox.Show('All files were added and extracted successfully.')

end;

end.
