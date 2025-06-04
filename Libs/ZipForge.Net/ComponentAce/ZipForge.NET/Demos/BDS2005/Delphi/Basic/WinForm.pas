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
    procedure ZipForge1_OnOverallProgress(sender: System.Object; progress: System.Double; 
      operation: ProcessOperation; progressPhase: ProgressPhase;
      var cancel: Boolean);
    strict private button1: System.Windows.Forms.Button;
    strict private button2: System.Windows.Forms.Button;
    strict private label1: System.Windows.Forms.Label;
    strict private progressBar1: System.Windows.Forms.ProgressBar;
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
begin
  Self.button1 := System.Windows.Forms.Button.Create;
  Self.button2 := System.Windows.Forms.Button.Create;
  Self.progressBar1 := System.Windows.Forms.ProgressBar.Create;
  Self.label1 := System.Windows.Forms.Label.Create;
  Self.ZipForge1 := ComponentAce.Compression.ZipForge.ZipForge.Create;
  Self.SuspendLayout;
  // 
  // button1
  // 
  Self.button1.Location := System.Drawing.Point.Create(111, 102);
  Self.button1.Name := 'button1';
  Self.button1.TabIndex := 0;
  Self.button1.Text := 'Start';
  Include(Self.button1.Click, Self.button1_Click);
  // 
  // button2
  // 
  Self.button2.ForeColor := System.Drawing.SystemColors.ControlText;
  Self.button2.Location := System.Drawing.Point.Create(225, 102);
  Self.button2.Name := 'button2';
  Self.button2.TabIndex := 1;
  Self.button2.Text := 'Exit';
  Include(Self.button2.Click, Self.button2_Click);
  // 
  // progressBar1
  // 
  Self.progressBar1.Location := System.Drawing.Point.Create(37, 63);
  Self.progressBar1.Name := 'progressBar1';
  Self.progressBar1.Size := System.Drawing.Size.Create(335, 23);
  Self.progressBar1.TabIndex := 2;
  // 
  // label1
  // 
  Self.label1.Location := System.Drawing.Point.Create(34, 7);
  Self.label1.Name := 'label1';
  Self.label1.Size := System.Drawing.Size.Create(338, 49);
  Self.label1.TabIndex := 3;
  Self.label1.Text := 'This demo illustrates how to add files and extract fi' +
  'les using ZipForge. Source files are located in Source folder. Destinatio' +
  'n archive file will be located in Archive\test.zip. Files will be extract' +
  'ed to folder Dest.';
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
  Include(Self.ZipForge1.OnOverallProgress, Self.ZipForge1_OnOverallProgress);
  // 
  // TWinForm
  // 
  Self.AutoScaleBaseSize := System.Drawing.Size.Create(5, 13);
  Self.ClientSize := System.Drawing.Size.Create(411, 128);
  Self.Controls.Add(Self.label1);
  Self.Controls.Add(Self.progressBar1);
  Self.Controls.Add(Self.button2);
  Self.Controls.Add(Self.button1);
  Self.Name := 'TWinForm';
  Self.Text := 'ZipForge.NET basic demo (c) ComponentAce';
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

procedure TWinForm.ZipForge1_OnOverallProgress(sender: System.Object; progress: System.Double;
  operation: ProcessOperation; progressPhase: ProgressPhase; 
  var cancel: Boolean);
begin
   self.progressBar1.Value := Trunc(progress)
end;

procedure TWinForm.button2_Click(sender: System.Object; e: System.EventArgs);
begin
  Close;
end;

procedure TWinForm.button1_Click(sender: System.Object; e: System.EventArgs);
var
  text1: string;
begin
  text1 := System.IO.Path.GetDirectoryName(Application.ExecutablePath);
  self.ZipForge1.FileName := Concat(text1, '\Archive\test.zip');
  self.ZipForge1.OpenArchive(FileMode.Create);
  self.ZipForge1.BaseDir := Concat(text1, '\Source');
  self.ZipForge1.AddFiles('*.*');
  self.ZipForge1.BaseDir := Concat(text1, '\Dest');
  self.ZipForge1.ExtractFiles('*.*');
  self.ZipForge1.CloseArchive;
  MessageBox.Show('All files were added and extracted successfully.')
end;

end.
