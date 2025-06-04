unit WinForm1;

interface

uses System.ComponentModel, 
  System.Windows.Forms, System.Drawing, ComponentAce.Compression.ZipForge,
    ComponentAce.Compression.Archiver;

type
  Form1 = class(System.Windows.Forms.Form)
  protected
    SaveFileDialog1: System.Windows.Forms.SaveFileDialog;
    Button3: System.Windows.Forms.Button;
    Button2: System.Windows.Forms.Button;
    Button1: System.Windows.Forms.Button;
    TextBox2: System.Windows.Forms.TextBox;
    TextBox1: System.Windows.Forms.TextBox;
    Label1: System.Windows.Forms.Label;
    OpenFileDialog1: System.Windows.Forms.OpenFileDialog;
    Label2: System.Windows.Forms.Label;
    OpenFileDialog2: System.Windows.Forms.OpenFileDialog;
  strict private
    components: System.ComponentModel.Container;
  public
    constructor Create;
  strict protected
    procedure Dispose(disposing: Boolean); override;
  strict private
    procedure InitializeComponent;
    [STAThread]
    class procedure Main;static; 
    procedure Button1_Click(sender: System.Object; e: System.EventArgs);
    procedure Button2_Click(sender: System.Object; e: System.EventArgs);
    procedure Button3_Click(sender: System.Object; e: System.EventArgs);
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
  Self.SaveFileDialog1 := System.Windows.Forms.SaveFileDialog.Create;
  Self.Button3 := System.Windows.Forms.Button.Create;
  Self.Button2 := System.Windows.Forms.Button.Create;
  Self.Button1 := System.Windows.Forms.Button.Create;
  Self.TextBox2 := System.Windows.Forms.TextBox.Create;
  Self.TextBox1 := System.Windows.Forms.TextBox.Create;
  Self.Label1 := System.Windows.Forms.Label.Create;
  Self.OpenFileDialog1 := System.Windows.Forms.OpenFileDialog.Create;
  Self.Label2 := System.Windows.Forms.Label.Create;
  Self.OpenFileDialog2 := System.Windows.Forms.OpenFileDialog.Create;
  Self.SuspendLayout;
  Self.SaveFileDialog1.Filter := 'Applications | *.exe';
  Self.Button3.Location := System.Drawing.Point.Create(176, 96);
  Self.Button3.Name := 'Button3';
  Self.Button3.Size := System.Drawing.Size.Create(160, 24);
  Self.Button3.TabIndex := 13;
  Self.Button3.Text := 'Make SFX...';
  Include(Self.Button3.Click, Self.Button3_Click);
  Self.Button2.Location := System.Drawing.Point.Create(408, 56);
  Self.Button2.Name := 'Button2';
  Self.Button2.Size := System.Drawing.Size.Create(64, 21);
  Self.Button2.TabIndex := 12;
  Self.Button2.Text := 'Browse...';
  Include(Self.Button2.Click, Self.Button2_Click);
  Self.Button1.Location := System.Drawing.Point.Create(408, 24);
  Self.Button1.Name := 'Button1';
  Self.Button1.Size := System.Drawing.Size.Create(65, 21);
  Self.Button1.TabIndex := 11;
  Self.Button1.Text := 'Browse...';
  Include(Self.Button1.Click, Self.Button1_Click);
  Self.TextBox2.Location := System.Drawing.Point.Create(128, 56);
  Self.TextBox2.Name := 'TextBox2';
  Self.TextBox2.Size := System.Drawing.Size.Create(272, 20);
  Self.TextBox2.TabIndex := 10;
  Self.TextBox2.Text := '';
  Self.TextBox1.Location := System.Drawing.Point.Create(128, 24);
  Self.TextBox1.Name := 'TextBox1';
  Self.TextBox1.Size := System.Drawing.Size.Create(272, 20);
  Self.TextBox1.TabIndex := 9;
  Self.TextBox1.Text := '';
  Self.Label1.Location := System.Drawing.Point.Create(16, 24);
  Self.Label1.Name := 'Label1';
  Self.Label1.TabIndex := 7;
  Self.Label1.Text := 'Archive file name';
  Self.OpenFileDialog1.DefaultExt := '*.zip';
  Self.OpenFileDialog1.Filter := 'zip archives | *.zip';
  Self.Label2.Location := System.Drawing.Point.Create(16, 64);
  Self.Label2.Name := 'Label2';
  Self.Label2.TabIndex := 8;
  Self.Label2.Text := 'SFX stub file name';
  Self.OpenFileDialog2.DefaultExt := '*.exe';
  Self.OpenFileDialog2.Filter := 'Applications | *.exe';
  Self.AutoScaleBaseSize := System.Drawing.Size.Create(5, 13);
  Self.ClientSize := System.Drawing.Size.Create(504, 125);
  Self.Controls.Add(Self.TextBox2);
  Self.Controls.Add(Self.TextBox1);
  Self.Controls.Add(Self.Label1);
  Self.Controls.Add(Self.Label2);
  Self.Controls.Add(Self.Button3);
  Self.Controls.Add(Self.Button2);
  Self.Controls.Add(Self.Button1);
  Self.Name := 'Form1';
  Self.Text := 'MakeSFX demo (C) ComponentAce 2007';
  Self.ResumeLayout(False);
end;

class procedure Form1.Main;
begin
  Application.Run(Form1.Create);
end;

procedure Form1.Button1_Click(sender: System.Object; e: System.EventArgs);
begin
  if (Self.OpenFileDialog1.ShowDialog = System.Windows.Forms.DialogResult.OK) then
    Self.TextBox1.Text := Self.OpenFileDialog1.FileName;
end;

procedure Form1.Button2_Click(sender: System.Object; e: System.EventArgs);
begin
  if (Self.OpenFileDialog2.ShowDialog = System.Windows.Forms.DialogResult.OK) then
    Self.TextBox2.Text := Self.OpenFileDialog2.FileName;
end;

procedure Form1.Button3_Click(sender: System.Object; e: System.EventArgs);
var
  zipForge: ComponentAce.Compression.ZipForge.ZipForge;
begin
  if (Self.SaveFileDialog1.ShowDialog = System.Windows.Forms.DialogResult.OK) then
  begin
    zipForge := ComponentAce.Compression.ZipForge.ZipForge.Create;
    zipForge.FileName := Self.TextBox1.Text;
    zipForge.SFXStub := Self.TextBox2.Text;
    zipForge.MakeSFX(Self.SaveFileDialog1.FileName);
  end;
end;

end.

