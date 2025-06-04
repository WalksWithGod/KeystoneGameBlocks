using System;
using System.Reflection;
using CSScriptLibrary;
using System.IO;

public class Host
{
    static void Main()
    {
        string code =
            @"using System;
			  public class Script
			  {
				  public static void SayHello(string greeting)
				  {
					  Console.WriteLine(""Static:   "" + greeting);
				  }
                  public void Say(string greeting)
				  {
					  Console.WriteLine(""Instance: "" + greeting);
				  }
			  }";

        var script = new AsmHelper(CSScript.LoadCode(code, null, true));

        //call static method
        script.Invoke("*.SayHello", "Hello World!");

        //call static method via emitted FastMethodInvoker
        var SayHello = script.GetStaticMethod("*.SayHello", "");
        SayHello("Hello World! (emitted method)");

        //call instance method
        object obj = script.CreateObject("Script");

        script.InvokeInst(obj, "*.Say", "Hello World!");

        //call instance method via emitted FastMethodInvoker
        var Say = script.GetMethod(obj, "*.Say", "");
        Say("Hello World! (emitted method)");
    }
}

