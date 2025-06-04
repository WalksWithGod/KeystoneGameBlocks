unit WinForm1;

interface

uses System.ComponentModel, System.IO,
  System.Drawing, ComponentAce.Compression.ZipForge, System.Windows.Forms,
    ComponentAce.Compression.Archiver;

type
  Form1 = class(System.Windows.Forms.Form)
  strict private
    components: System.ComponentModel.Container;
    button1: Button;
    button2: Button;
    button3: Button;
    folderBrowserDialog1: FolderBrowserDialog;
    label1: &Label;
    checkBox1: System.Windows.Forms.CheckBox;
    textBox1: TextBox;
  public
    constructor Create;
  strict protected
    procedure Dispose(disposing: Boolean); override;
  strict private
    procedure InitializeComponent;
    [STAThread]
    class procedure Main;static;
    procedure button1_Click(sender: System.Object; e: System.EventArgs);
    procedure button2_Click(sender: System.Object; e: System.EventArgs);
    procedure button3_Click(sender: System.Object; e: System.EventArgs);
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
  Self.label1 := System.Windows.Forms.Label.Create;
  Self.textBox1 := System.Windows.Forms.TextBox.Create;
  Self.folderBrowserDialog1 := System.Windows.Forms.FolderBrowserDialog.Create;
  Self.button1 := System.Windows.Forms.Button.Create;
  Self.button2 := System.Windows.Forms.Button.Create;
  Self.button3 := System.Windows.Forms.Button.Create;
  Self.checkBox1 := System.Windows.Forms.CheckBox.Create;
  Self.SuspendLayout;
  Self.label1.Location := System.Drawing.Point.Create(14, 8);
  Self.label1.Name := 'label1';
  Self.label1.Size := System.Drawing.Size.Create(320, 16);
  Self.label1.TabIndex := 0;
  Self.label1.Text := 'Select destination folder';
  Self.textBox1.Location := System.Drawing.Point.Create(16, 32);
  Self.textBox1.Name := 'textBox1';
  Self.textBox1.Size := System.Drawing.Size.Create(320, 20);
  Self.textBox1.TabIndex := 1;
  Self.textBox1.Text := '';
  Self.button1.Location := System.Drawing.Point.Create(344, 32);
  Self.button1.Name := 'button1';
  Self.button1.TabIndex := 2;
  Self.button1.Text := 'Browse...';
  Include(Self.button1.Click, Self.button1_Click);
  Self.button2.Location := System.Drawing.Point.Create(128, 96);
  Self.button2.Name := 'button2';
  Self.button2.TabIndex := 3;
  Self.button2.Text := 'Extract';
  Include(Self.button2.Click, Self.button2_Click);
  Self.button3.Location := System.Drawing.Point.Create(224, 96);
  Self.button3.Name := 'button3';
  Self.button3.TabIndex := 4;
  Self.button3.Text := 'Exit';
  Include(Self.button3.Click, Self.button3_Click);
  Self.checkBox1.Location := System.Drawing.Point.Create(16, 64);
  Self.checkBox1.Name := 'checkBox1';
  Self.checkBox1.Size := System.Drawing.Size.Create(176, 16);
  Self.checkBox1.TabIndex := 5;
  Self.checkBox1.Text := 'Overwrite existing files';
  Self.AutoScaleBaseSize := System.Drawing.Size.Create(5, 13);
  Self.ClientSize := System.Drawing.Size.Create(424, 133);
  Self.Controls.Add(Self.checkBox1);
  Self.Controls.Add(Self.button3);
  Self.Controls.Add(Self.button2);
  Self.Controls.Add(Self.button1);
  Self.Controls.Add(Self.textBox1);
  Self.Controls.Add(Self.label1);
  Self.Name := 'Form1';
  Self.Text := 'ZipForge.NET SFX  stub demo (C) ComponentAce 2007';
  Self.ResumeLayout(False);
end;

class procedure Form1.Main;
begin
  Application.Run(Form1.Create);
end;

procedure Form1.button1_Click(sender: System.Object; e: System.EventArgs);
begin
  if (Self.folderBrowserDialog1.ShowDialog = System.Windows.Forms.DialogResult.OK) then
    Self.textBox1.Text := Self.folderBrowserDialog1.SelectedPath;
end;

procedure Form1.button2_Click(sender: System.Object; e: System.EventArgs);
var
  ZipForge1: ComponentAce.Compression.ZipForge.ZipForge;
begin
  ZipForge1 := ComponentAce.Compression.ZipForge.ZipForge.Create;
  ZipForge1.FileName := Application.ExecutablePath;
  ZipForge1.OpenArchive(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
  ZipForge1.BaseDir := Self.textBox1.Text;
  if Self.checkBox1.Checked then
    ZipForge1.Options.Overwrite := OverwriteMode.Always
  else
    ZipForge1.Options.Overwrite := OverwriteMode.Never;
  ZipForge1.ExtractFiles('*.*');
  ZipForge1.CloseArchive;
  System.Windows.Forms.MessageBox.Show('All files were extracted successfully');
end;

procedure Form1.button3_Click(sender: System.Object; e: System.EventArgs);
begin
  Close;
end;

end.
