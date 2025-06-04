using System;
using System.Threading;
using System.Collections;
using System.IO;

class Script
{
    const string usage = "Usage: cscscript clearTemp\n" +
                         "Deletes all temporary files created by the script engine.\n";


    static public void Main(string[] args)
    {
        if (args.Length == 1 && (args[0] == "?" || args[0] == "/?" || args[0] == "-?" || args[0].ToLower() == "help"))
        {
            Console.WriteLine(usage);
        }
        else
        {
            //delete temporary Visusl Studio projects
            string baseDir = Path.Combine(Path.GetTempPath(), "CSSCRIPT");
            if (Directory.Exists(baseDir))
            {
                foreach (string projectDir in Directory.GetDirectories(baseDir))
                {
                    string dirLock = Path.Combine(projectDir, "dir.lock");
                    if (File.Exists(dirLock))
                        try
                        {
                            File.Delete(dirLock);
                        }
                        catch 
                        {
                            continue; //the directory is in use
                        }
                    DeleteDir(projectDir);
               }

                //delete all DLLs; any dll that is currently in use will not be deleted (try...catch); 
                string[] files = Directory.GetFiles(baseDir);

                Thread.Sleep(3000); //allow just created files to be loaded (get locked)

                foreach (string file in files)
                {
                    try
                    {
                        File.Delete(file);
                    }
                    catch { }
                }
            }
        }
    }

    static private void DeleteDir(string dir)
    {
        //deletes folder recursively
        try
        {
            foreach (string file in Directory.GetFiles(dir))
                try
                {
                    File.Delete(file);
                }
                catch
                {
                }

            foreach (string subDir in Directory.GetDirectories(dir))
                DeleteDir(subDir);

            Directory.Delete(dir);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}