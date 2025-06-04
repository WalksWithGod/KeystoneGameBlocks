using System;
using KeyServices;

namespace KeyMaster
{
    /// <summary>
    /// Summary description for Application.
    /// </summary>
    class Application
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            #region serviceInstall
            //WindowsServiceInstallInfo wsInstallInfo;
            //WindowsServiceInstallUtil wsInstallUtil;
            //RemoteWindowsServiceInstallUtil rwsInstallUtil;
            //bool result;
            //string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            //string serviceName  ="KeyAuthenticator.exe";
            ////
            //// Install with default settings
            ////
            //wsInstallInfo = new WindowsServiceInstallInfo(currentDirectory, serviceName, WindowsServiceAccountType.LocalService);
            //wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);

            ////Log to see any error
            ////wsInstallUtil.Install(@"C:\test.txt");
            //result = wsInstallUtil.Install();
            //Console.Out.WriteLine("Installed : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////Log to see any error
            ////wsInstallUtil.Uninstall(@"C:\test.txt");
            //result = wsInstallUtil.Uninstall();
            //Console.Out.WriteLine("Uninstalled : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////
            //// Pass new settings before installing windows service
            ////
            //wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithLocalSystem", "MyService Description", currentDirectory, serviceName, WindowsServiceAccountType.LocalSystem);
            //wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);

            ////Log to see any error
            ////wsInstallUtil.Install(@"C:\test.txt");
            //result = wsInstallUtil.Install();
            //Console.Out.WriteLine("Installed : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////Log to see any error
            ////wsInstallUtil.Uninstall(@"C:\test.txt");
            //result = wsInstallUtil.Uninstall();
            //Console.Out.WriteLine("Uninstalled : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////
            //// Installing windows service to start with username and password, but use default service name and description
            ////
            ////If install with local user:
            ////wsInstallInfo = new WindowsServiceInstallInfo(Directory.GetCurrentDirectory(), "MyService.exe", @".\username", @"password");
            ////If install with network user:
            ////wsInstallInfo = new WindowsServiceInstallInfo(Directory.GetCurrentDirectory(), "MyService.exe", @"networkdomain\username", @"password");
            //wsInstallInfo = new WindowsServiceInstallInfo(currentDirectory, serviceName, @".\IBMR40", @"");
            //wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);

            ////Log to see any error
            ////wsInstallUtil.Install(@"C:\test.txt");
            //result = wsInstallUtil.Install();
            //Console.Out.WriteLine("Installed : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////Log to see any error
            ////wsInstallUtil.Uninstall(@"C:\test.txt");
            //result = wsInstallUtil.Uninstall();
            //Console.Out.WriteLine("Uninstalled : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////
            //// Installing windows service to start with username and password in a remote machine
            ////
            ////If install with local user:
            ////wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", Directory.GetCurrentDirectory(), "MyService.exe", @".\username", @"password");
            ////If install with network user:
            ////wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", Directory.GetCurrentDirectory(), "MyService.exe", @"networkdomain\username", @"password");
            //wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", currentDirectory, serviceName, @".\IBMR40", @"");
            //rwsInstallUtil = new RemoteWindowsServiceInstallUtil("localhost", wsInstallInfo);

            ////Log to see any error
            ////wsInstallUtil.Install(@"C:\test.txt");
            //result = rwsInstallUtil.Install();
            //Console.Out.WriteLine("Installed : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////Log to see any error
            ////wsInstallUtil.Uninstall(@"C:\test.txt");
            //result = rwsInstallUtil.Uninstall();
            //Console.Out.WriteLine("Uninstalled : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////
            //// Installing windows service to start with username and password
            ////
            ////If install with local user:
            ////wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", Directory.GetCurrentDirectory(), "MyService.exe", @".\username", @"password");
            ////If install with network user:
            ////wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", Directory.GetCurrentDirectory(), "MyService.exe", @"networkdomain\username", @"password");
            //wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", currentDirectory, serviceName, @".\IBMR40", @"");
            //wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);

            //Console.Out.WriteLine("Before installing service remotely, make sure:");
            //Console.Out.WriteLine("1. You've run PsTools/psexec.exe the first time and agreed to PsTools Policy");
            //Console.Out.WriteLine("2. The remote machine also has installutil.exe");

            ////Change the installutil path if the remote machine doesnt have same Framework version
            ////RemoteWindowsServiceInstallUtil.InstallUtilPath = @"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727";
            ////Change PsTools Path if PsTools folder is not in the same location of program assembly
            ////RemoteWindowsServiceInstallUtil.PsToolsPath = @"C:\Program Files\PsTools";

            ////Log to see any error
            ////wsInstallUtil.Install(@"C:\test.txt");
            //result = wsInstallUtil.Install();
            //Console.Out.WriteLine("Remote Installed : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////Log to see any error
            ////wsInstallUtil.Uninstall(@"C:\test.txt");
            //result = wsInstallUtil.Uninstall();
            //Console.Out.WriteLine("Remote Uninstalled : " + result);

            //Console.Out.WriteLine("Press any key to continue ...");
            //Console.ReadKey();

            ////
            //// End of program
            ////
            //return;
            #endregion
            // the above code is code for installing and uninstalling services in code
#if (!DEBUG)
            // we'll go ahead and create an array so that we can add the different services that
            // we'll create over time.
            KeyServiceBase[] servicesToRun;

            // to create a new instance of a new service, just add it to the list of services 
            // specified in the ServiceBase array constructor.
            servicesToRun = new KeyServiceBase[] { new KeyMaster.MasterServerService() };

            // now run all the service that we have created. This doesn't actually cause the services
            // to run but it registers the services with the Service Control Manager so that it can
            // when you start the service the SCM will call the OnStart method of the service.
            KeyServiceBase.Run(servicesToRun);     
#else
            // Debug code: this allows the process to run as a non-service.
            // It will kick off the service start point, but never kill it.
            // Shut down the debugger to exit
            MasterServerService service = new MasterServerService();
            service.DebugStartEntryPoint(null);
            // Put a breakpoint on the following line to always catch
            // your service when it has finished its work
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif
        }
    }
}
