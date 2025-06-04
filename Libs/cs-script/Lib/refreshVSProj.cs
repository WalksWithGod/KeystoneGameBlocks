using System;
using System.Xml;
using System.Windows.Forms;
using System.IO;

//css_import debugVS8.0.cs;
//css_import debugVS9.0.cs;
//C:\Users\Master\AppData\Local\Temp\CSSCRIPT\594769\test (script).csproj
class Script
{
	static string usage =   "Usage: cscscript refreshVSProj file...\nUpdates C# script project file loaded in VisualStudio.\n"+
							"Can be used as a Visual Studio external tool macro:\n\t refreshVSProj \"$(ProjectDir)\\$(ProjectFileName)\"\n";

	static public void Main(string[] args)
	{
		if (args.Length == 0 || (args.Length == 1 && (args[0] == "?" || args[0] == "/?" || args[0] == "-?" || args[0].ToLower() == "help")))
		{
			Console.WriteLine(usage);
		}
		else
		{
			System.Diagnostics.Debug.Assert(false);
			bool isVS8 = true;
			bool isVS9 = true;

            VS90.VSProjectDoc doc = new VS90.VSProjectDoc(args[0]);
            XmlNode node = doc.SelectFirstNode("//Project");
            XmlAttribute version = node.Attributes["ToolsVersion"];
            if (version != null && version.Value == "3.5")
            {
                isVS9 = true;
                isVS8 = false;
            }
            else
            {
                isVS9 = false;
                isVS8 = true;
            }
			
			if (isVS8)
			{
				VS80.Script.i_Main(new string[] { "/r", args[0] });
			}
			else if (isVS9)
			{
				VS90.Script.i_Main(new string[] { "/r", args[0] });
			}
		}
	}
}

