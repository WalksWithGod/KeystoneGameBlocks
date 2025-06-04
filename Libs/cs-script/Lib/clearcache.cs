using System;
using System.IO;

class Script 
{
	const string usage = "Usage: cscscript clearCache\n"+
						 "Deletes all compiled script files (.csc) from the ScriptLibrary directory. "+
						 "Use this script if version of of target CLR is changed.\n";
						 

	static public void Main(string[] args)
	{
		if (args.Length == 1 && (args[0] == "?" || args[0] == "/?" || args[0] == "-?" || args[0].ToLower() == "help"))
		{
			Console.WriteLine(usage);
		}
		else
		{
			foreach (string file in Directory.GetFiles(Path.Combine(Environment.GetEnvironmentVariable("CSSCRIPT_DIR"), "Lib"), "*.csc"))
			{
				try
				{
					File.Delete(file);
					Console.WriteLine("Cleared: "+file);
				}
				catch
				{
				}
			}
		}
	}
}