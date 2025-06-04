unit WinForm1;


interface

uses ComponentAce.Compression.ZipForge, ComponentAce.Compression.Archiver,
  System.ComponentModel,
  System.Windows.Forms,
  System.Drawing, System.IO;

type
  Form1 = class(System.Windows.Forms.Form)
  strict private
    tabControl1: System.Windows.Forms.TabControl;
    tabPage1: System.Windows.Forms.TabPage;
    label1: System.Windows.Forms.Label;
    label2: System.Windows.Forms.Label;
    tabPage2: System.Windows.Forms.TabPage;
    label3: System.Windows.Forms.Label;
    label4: System.Windows.Forms.Label;
    button7: System.Windows.Forms.Button;
    createSplittingButton: System.Windows.Forms.Button;
    createSpanningButton: System.Windows.Forms.Button;
    btnNewArchiveFile: System.Windows.Forms.Button;
    btnSourceFiles: System.Windows.Forms.Button;
    tbNewArchiveFile: System.Windows.Forms.TextBox;
    tbSourceFiles: System.Windows.Forms.TextBox;
    btnExtractTo: System.Windows.Forms.Button;
    btnArchiveFile: System.Windows.Forms.Button;
    tbExtractTo: System.Windows.Forms.TextBox;
    tbArchiveFile: System.Windows.Forms.TextBox;
    ofdSourceFiles: System.Windows.Forms.OpenFileDialog;
    sfdNewArchiveFile: System.Windows.Forms.SaveFileDialog;
    pbProgress: System.Windows.Forms.ProgressBar;
    ofdArchiveFile: System.Windows.Forms.OpenFileDialog;
    fbdExtractTo: System.Windows.Forms.FolderBrowserDialog;
    components: System.ComponentModel.Container;
  public
    constructor Create;
  strict protected
    procedure Dispose(disposing: Boolean); override;
  strict private
    procedure InitializeComponent;
    [STAThread]
    class procedure Main;static; 
    procedure button7_Click(sender: System.Object; e: System.EventArgs);
    procedure createArchive_Click(sender: System.Object; e: System.EventArgs);
    procedure btnSourceFiles_Click(sender: System.Object; e: System.EventArgs);
    procedure btnNewArchiveFile_Click(sender: System.Object; e: System.EventArgs);
    procedure btnArchiveFile_Click(sender: System.Object; e: System.EventArgs);
    procedure btnExtractTo_Click(sender: System.Object; e: System.EventArgs);
    procedure zf_OnOverallProgress(sender: System.Object; progress: System.Double; 
      operation: ProcessOperation; progressPhase: ProgressPhase;
      var cancel: Boolean);
  end;
  
implementation

{$AUTOBOX ON}
{$HINTS OFF}
{$WARNINGS OFF}

constructor Form1.Create;
begin
  inherited Create;
  InitializeComponent;
end;

procedure Form1.Dispose(disposing: Boolean);
begin
  if disposing then
    if (Self.components <> nil) then
      Self.components.Dispose;
  inherited Dispose(disposing);
end;

procedure Form1.InitializeComponent;
begin
  Self.tabControl1 := System.Windows.Forms.TabControl.Create;
  Self.tabPage1 := System.Windows.Forms.TabPage.Create;
  Self.createSplittingButton := System.Windows.Forms.Button.Create;
  Self.createSpanningButton := System.Windows.Forms.Button.Create;
  Self.btnNewArchiveFile := System.Windows.Forms.Button.Create;
  Self.btnSourceFiles := System.Windows.Forms.Button.Create;
  Self.label2 := System.Windows.Forms.Label.Create;
  Self.label1 := System.Windows.Forms.Label.Create;
  Self.tbNewArchiveFile := System.Windows.Forms.TextBox.Create;
  Self.tbSourceFiles := System.Windows.Forms.TextBox.Create;
  Self.tabPage2 := System.Windows.Forms.TabPage.Create;
  Self.button7 := System.Windows.Forms.Button.Create;
  Self.btnExtractTo := System.Windows.Forms.Button.Create;
  Self.btnArchiveFile := System.Windows.Forms.Button.Create;
  Self.label3 := System.Windows.Forms.Label.Create;
  Self.label4 := System.Windows.Forms.Label.Create;
  Self.tbExtractTo := System.Windows.Forms.TextBox.Create;
  Self.tbArchiveFile := System.Windows.Forms.TextBox.Create;
  Self.pbProgress := System.Windows.Forms.ProgressBar.Create;
  Self.ofdSourceFiles := System.Windows.Forms.OpenFileDialog.Create;
  Self.sfdNewArchiveFile := System.Windows.Forms.SaveFileDialog.Create;
  Self.ofdArchiveFile := System.Windows.Forms.OpenFileDialog.Create;
  Self.fbdExtractTo := System.Windows.Forms.FolderBrowserDialog.Create;
  Self.tabControl1.SuspendLayout;
  Self.tabPage1.SuspendLayout;
  Self.tabPage2.SuspendLayout;
  Self.SuspendLayout;
  Self.tabControl1.Controls.Add(Self.tabPage1);
  Self.tabControl1.Controls.Add(Self.tabPage2);
  Self.tabControl1.Dock := System.Windows.Forms.DockStyle.Top;
  Self.tabControl1.Location := System.Drawing.Point.Create(0, 0);
  Self.tabControl1.Name := 'tabControl1';
  Self.tabControl1.SelectedIndex := 0;
  Self.tabControl1.Size := System.Drawing.Size.Create(416, 208);
  Self.tabControl1.TabIndex := 0;
  Self.tabPage1.Controls.Add(Self.createSplittingButton);
  Self.tabPage1.Controls.Add(Self.createSpanningButton);
  Self.tabPage1.Controls.Add(Self.btnNewArchiveFile);
  Self.tabPage1.Controls.Add(Self.btnSourceFiles);
  Self.tabPage1.Controls.Add(Self.label2);
  Self.tabPage1.Controls.Add(Self.label1);
  Self.tabPage1.Controls.Add(Self.tbNewArchiveFile);
  Self.tabPage1.Controls.Add(Self.tbSourceFiles);
  Self.tabPage1.Location := System.Drawing.Point.Create(4, 22);
  Self.tabPage1.Name := 'tabPage1';
  Self.tabPage1.Size := System.Drawing.Size.Create(408, 182);
  Self.tabPage1.TabIndex := 0;
  Self.tabPage1.Text := 'Create archive';
  Self.createSplittingButton.Location := System.Drawing.Point.Create(40, 120);
  Self.createSplittingButton.Name := 'createSplittingButton';
  Self.createSplittingButton.Size := System.Drawing.Size.Create(144, 23);
  Self.createSplittingButton.TabIndex := 7;
  Self.createSplittingButton.Text := 'Create splitting archive';
  Include(Self.createSplittingButton.Click, Self.createArchive_Click);
  Self.createSpanningButton.Location := System.Drawing.Point.Create(192, 120);
  Self.createSpanningButton.Name := 'createSpanningButton';
  Self.createSpanningButton.Size := System.Drawing.Size.Create(152, 23);
  Self.createSpanningButton.TabIndex := 6;
  Self.createSpanningButton.Text := 'Create spanning archive';
  Include(Self.createSpanningButton.Click, Self.createArchive_Click);
  Self.btnNewArchiveFile.Location := System.Drawing.Point.Create(312, 72);
  Self.btnNewArchiveFile.Name := 'btnNewArchiveFile';
  Self.btnNewArchiveFile.TabIndex := 5;
  Self.btnNewArchiveFile.Text := 'Browse...';
  Include(Self.btnNewArchiveFile.Click, Self.btnNewArchiveFile_Click);
  Self.btnSourceFiles.Location := System.Drawing.Point.Create(312, 24);
  Self.btnSourceFiles.Name := 'btnSourceFiles';
  Self.btnSourceFiles.TabIndex := 4;
  Self.btnSourceFiles.Text := 'Browse...';
  Include(Self.btnSourceFiles.Click, Self.btnSourceFiles_Click);
  Self.label2.AutoSize := True;
  Self.label2.Location := System.Drawing.Point.Create(8, 80);
  Self.label2.Name := 'label2';
  Self.label2.Size := System.Drawing.Size.Create(60, 16);
  Self.label2.TabIndex := 3;
  Self.label2.Text := 'Archive file';
  Self.label1.AutoSize := True;
  Self.label1.Location := System.Drawing.Point.Create(8, 32);
  Self.label1.Name := 'label1';
  Self.label1.Size := System.Drawing.Size.Create(63, 16);
  Self.label1.TabIndex := 2;
  Self.label1.Text := 'Source files';
  Self.tbNewArchiveFile.Location := System.Drawing.Point.Create(80, 72);
  Self.tbNewArchiveFile.Name := 'tbNewArchiveFile';
  Self.tbNewArchiveFile.Size := System.Drawing.Size.Create(216, 20);
  Self.tbNewArchiveFile.TabIndex := 1;
  Self.tbNewArchiveFile.Text := '';
  Self.tbSourceFiles.Location := System.Drawing.Point.Create(80, 24);
  Self.tbSourceFiles.Name := 'tbSourceFiles';
  Self.tbSourceFiles.Size := System.Drawing.Size.Create(216, 20);
  Self.tbSourceFiles.TabIndex := 0;
  Self.tbSourceFiles.Text := '';
  Self.tabPage2.Controls.Add(Self.button7);
  Self.tabPage2.Controls.Add(Self.btnExtractTo);
  Self.tabPage2.Controls.Add(Self.btnArchiveFile);
  Self.tabPage2.Controls.Add(Self.label3);
  Self.tabPage2.Controls.Add(Self.label4);
  Self.tabPage2.Controls.Add(Self.tbExtractTo);
  Self.tabPage2.Controls.Add(Self.tbArchiveFile);
  Self.tabPage2.Location := System.Drawing.Point.Create(4, 22);
  Self.tabPage2.Name := 'tabPage2';
  Self.tabPage2.Size := System.Drawing.Size.Create(408, 182);
  Self.tabPage2.TabIndex := 1;
  Self.tabPage2.Text := 'Extract files from archive';
  Self.button7.Location := System.Drawing.Point.Create(128, 120);
  Self.button7.Name := 'button7';
  Self.button7.Size := System.Drawing.Size.Create(128, 24);
  Self.button7.TabIndex := 12;
  Self.button7.Text := 'Extract files';
  Include(Self.button7.Click, Self.button7_Click);
  Self.btnExtractTo.Location := System.Drawing.Point.Create(312, 72);
  Self.btnExtractTo.Name := 'btnExtractTo';
  Self.btnExtractTo.TabIndex := 11;
  Self.btnExtractTo.Text := 'Browse...';
  Include(Self.btnExtractTo.Click, Self.btnExtractTo_Click);
  Self.btnArchiveFile.Location := System.Drawing.Point.Create(312, 24);
  Self.btnArchiveFile.Name := 'btnArchiveFile';
  Self.btnArchiveFile.TabIndex := 10;
  Self.btnArchiveFile.Text := 'Browse...';
  Include(Self.btnArchiveFile.Click, Self.btnArchiveFile_Click);
  Self.label3.AutoSize := True;
  Self.label3.Location := System.Drawing.Point.Create(8, 80);
  Self.label3.Name := 'label3';
  Self.label3.Size := System.Drawing.Size.Create(52, 16);
  Self.label3.TabIndex := 9;
  Self.label3.Text := 'Extract to';
  Self.label4.AutoSize := True;
  Self.label4.Location := System.Drawing.Point.Create(8, 32);
  Self.label4.Name := 'label4';
  Self.label4.Size := System.Drawing.Size.Create(60, 16);
  Self.label4.TabIndex := 8;
  Self.label4.Text := 'Archive file';
  Self.tbExtractTo.Location := System.Drawing.Point.Create(80, 72);
  Self.tbExtractTo.Name := 'tbExtractTo';
  Self.tbExtractTo.Size := System.Drawing.Size.Create(216, 20);
  Self.tbExtractTo.TabIndex := 7;
  Self.tbExtractTo.Text := '';
  Self.tbArchiveFile.Location := System.Drawing.Point.Create(80, 24);
  Self.tbArchiveFile.Name := 'tbArchiveFile';
  Self.tbArchiveFile.Size := System.Drawing.Size.Create(216, 20);
  Self.tbArchiveFile.TabIndex := 6;
  Self.tbArchiveFile.Text := '';
  Self.pbProgress.Dock := System.Windows.Forms.DockStyle.Bottom;
  Self.pbProgress.Location := System.Drawing.Point.Create(0, 205);
  Self.pbProgress.Name := 'pbProgress';
  Self.pbProgress.Size := System.Drawing.Size.Create(416, 16);
  Self.pbProgress.TabIndex := 1;
  Self.sfdNewArchiveFile.DefaultExt := '*.zip';
  Self.sfdNewArchiveFile.Filter := 'ZIP files|*.zip';
  Self.ofdArchiveFile.Filter := 'ZIP archives|*.zip';
  Self.AutoScaleBaseSize := System.Drawing.Size.Create(5, 13);
  Self.ClientSize := System.Drawing.Size.Create(416, 221);
  Self.Controls.Add(Self.pbProgress);
  Self.Controls.Add(Self.tabControl1);
  Self.Name := 'Form1';
  Self.Text := 'Spanning demo (C) ComponentAce 2007';
  Self.tabControl1.ResumeLayout(False);
  Self.tabPage1.ResumeLayout(False);
  Self.tabPage2.ResumeLayout(False);
  Self.ResumeLayout(False);
end;

class procedure Form1.Main;
begin
  Application.Run(Form1.Create);
end;

procedure Form1.button7_Click(sender: System.Object; e: System.EventArgs);
var
  ZipForge1: ZipForge;
begin
  ZipForge1 := ZipForge.Create;
  ZipForge1.FileName := Self.tbArchiveFile.Text;
  ZipForge1.OpenArchive;
  ZipForge1.BaseDir := Self.tbExtractTo.Text;
  Include(ZipForge1.OnOverallProgress, zf_OnOverallProgress);
  ZipForge1.ExtractFiles('*.*');
  ZipForge1.CloseArchive;
end;

procedure Form1.createArchive_Click(sender: System.Object; e: System.EventArgs);
var
  ZipForge1: ZipForge;
begin
  ZipForge1 := ZipForge.Create;
  Include(ZipForge1.OnOverallProgress, zf_OnOverallProgress);
  if (Name = Self.createSpanningButton.Name) then
    ZipForge1.SpanningMode := SpanningMode.Spanning
  else
    ZipForge1.SpanningMode := SpanningMode.Splitting;
  ZipForge1.SpanningOptions.VolumeSize := VolumeSize.Disk1_44MB;
  ZipForge1.FileName := Self.tbNewArchiveFile.Text;
  ZipForge1.OpenArchive(System.IO.FileMode.Create);
  ZipForge1.BaseDir := System.IO.Path.GetDirectoryName(Self.tbSourceFiles.Text);
  ZipForge1.AddFiles(Self.tbSourceFiles.Text);
  ZipForge1.CloseArchive;
end;

procedure Form1.btnSourceFiles_Click(sender: System.Object; e: System.EventArgs);
begin
  if (Self.ofdSourceFiles.ShowDialog = System.Windows.Forms.DialogResult.OK) then
    Self.tbSourceFiles.Text := Self.ofdSourceFiles.FileName;
end;

procedure Form1.btnNewArchiveFile_Click(sender: System.Object; e: System.EventArgs);
begin
  if (Self.sfdNewArchiveFile.ShowDialog = System.Windows.Forms.DialogResult.OK) then
    Self.tbNewArchiveFile.Text := Self.sfdNewArchiveFile.FileName;
end;

procedure Form1.btnArchiveFile_Click(sender: System.Object; e: System.EventArgs);
begin
  if (Self.ofdArchiveFile.ShowDialog = System.Windows.Forms.DialogResult.OK) then
    Self.tbArchiveFile.Text := Self.ofdArchiveFile.FileName;
end;

procedure Form1.btnExtractTo_Click(sender: System.Object; e: System.EventArgs);
begin
  if (Self.fbdExtractTo.ShowDialog = System.Windows.Forms.DialogResult.OK) then
    Self.tbExtractTo.Text := Self.fbdExtractTo.SelectedPath;
end;

procedure Form1.zf_OnOverallProgress(sender: System.Object; progress: System.Double; 
  operation: ProcessOperation; progressPhase: ProgressPhase;
  var cancel: Boolean);
begin
  Self.pbProgress.Value := System.Convert.ToInt32(progress);
end;

end.

