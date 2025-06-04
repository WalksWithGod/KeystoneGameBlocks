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
    _Exit: System.Windows.Forms.Button;
    Label1: System.Windows.Forms.Label;
    Start: System.Windows.Forms.Button;
    ZipForge1: ZipForge;

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    procedure InitializeComponent;
    procedure Start_Click(sender: System.Object; e: System.EventArgs);
    procedure _Exit_Click(sender: System.Object; e: System.EventArgs);
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
  Self.Label1 := System.Windows.Forms.Label.Create;
  Self.Start := System.Windows.Forms.Button.Create;
  Self._Exit := System.Windows.Forms.Button.Create;
  Self.ZipForge1 := ZipForge.Create;
  Self.SuspendLayout;
  // 
  // Label1
  // 
  Self.Label1.Location := System.Drawing.Point.Create(10, 14);
  Self.Label1.Name := 'Label1';
  Self.Label1.Size := System.Drawing.Size.Create(457, 40);
  Self.Label1.TabIndex := 3;
  Self.Label1.Text := 'This demo illustrates how to use a transaction system' +
  '. Source files are located in Source and Source1 folder. Destination arch' +
  'ive file will be located in Archive\test.zip. Files will be extracted to ' +
  'folder Dest.';
  // 
  // Start
  // 
  Self.Start.BackColor := System.Drawing.SystemColors.Control;
  Self.Start.Location := System.Drawing.Point.Create(122, 70);
  Self.Start.Name := 'Start';
  Self.Start.Size := System.Drawing.Size.Create(75, 25);
  Self.Start.TabIndex := 2;
  Self.Start.Text := 'Start';
  Include(Self.Start.Click, Self.Start_Click);
  // 
  // _Exit
  // 
  Self._Exit.BackColor := System.Drawing.SystemColors.Control;
  Self._Exit.Location := System.Drawing.Point.Create(242, 70);
  Self._Exit.Name := '_Exit';
  Self._Exit.Size := System.Drawing.Size.Create(75, 25);
  Self._Exit.TabIndex := 4;
  Self._Exit.Text := 'Exit';
  Include(Self._Exit.Click, Self._Exit_Click);
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
  Self.ClientSize := System.Drawing.Size.Create(477, 109);
  Self.Controls.Add(Self.Label1);
  Self.Controls.Add(Self.Start);
  Self.Controls.Add(Self._Exit);
  Self.Name := 'TWinForm';
  Self.Text := 'ZipForge.NET Transaction System demo (c) ComponentAce 2007';
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

procedure TWinForm._Exit_Click(sender: System.Object; e: System.EventArgs);
begin
  Close;
end;

procedure TWinForm.Start_Click(sender: System.Object; e: System.EventArgs);
var
  text1: string;
begin
  text1 := Path.GetDirectoryName(Application.ExecutablePath) + '\';
  self.ZipForge1.FileName := Concat(text1, 'Archive\test.zip');
  self.ZipForge1.OpenArchive(FileMode.Create);
  self.ZipForge1.BeginUpdate;
  self.ZipForge1.BaseDir := Concat(text1, 'Source');
  try
    self.ZipForge1.AddFiles('*.*')
  except
    self.ZipForge1.CancelUpdate;
    self.ZipForge1.CloseArchive;
    MessageBox.Show('Error adding all files');
    exit
  end;
  self.ZipForge1.BaseDir := Concat(text1, 'Source1\');
  try
  self.ZipForge1.AddFiles('*.htm*')
  except
    self.ZipForge1.CancelUpdate;
    self.ZipForge1.CloseArchive;
    MessageBox.Show('Error adding html files');
    exit
  end;
  self.ZipForge1.EndUpdate;
  self.ZipForge1.BaseDir := Concat(text1, 'Dest');
  self.ZipForge1.ExtractFiles('*.*');
  self.ZipForge1.CloseArchive;
  MessageBox.Show('All files were added and extracted successfully.')
end;

end.
