/*
 * Written by Tung1 (nguyenthanhtungtinbk@yahoo.com)
 * 20070611: Created
 * 20070705: Added description for services
 * 20070726: Added remoteInstallUtil class
 * Interesting reference: http://www.codeproject.com/install/ScriptedServiceInstall.asp (Install a Service using a Script)
 * 
 * SAMPLE USAGE:
        public static void Main()
        {
            WindowsServiceInstallInfo wsInstallInfo;
            WindowsServiceInstallUtil wsInstallUtil;
            bool result;

            //
            // Install with default settings
            //
            wsInstallInfo = new WindowsServiceInstallInfo(Directory.GetCurrentDirectory(), "MyService.exe", WindowsServiceAccountType.LocalService);
            wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);

            //Log to see any error
            //wsInstallUtil.Install(@"C:\test.txt");
            result = wsInstallUtil.Install();
            Console.WriteLine("Installed : " + result);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            //Log to see any error
            //wsInstallUtil.Uninstall(@"C:\test.txt");
            result = wsInstallUtil.Uninstall();
            Console.WriteLine("Uninstalled : " + result);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            //
            // Pass new settings before installing windows service
            //
            wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithLocalSystem", "MyService Description", Directory.GetCurrentDirectory(), "MyService.exe", WindowsServiceAccountType.LocalSystem);
            wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);

            //Log to see any error
            //wsInstallUtil.Install(@"C:\test.txt");
            result = wsInstallUtil.Install();
            Console.WriteLine("Installed : " + result);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            //Log to see any error
            //wsInstallUtil.Uninstall(@"C:\test.txt");
            result = wsInstallUtil.Uninstall();
            Console.WriteLine("Uninstalled : " + result);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            //
            // Installing windows service to start with username and password, but use default service name and description
            //
            //If install with local user:
            //wsInstallInfo = new WindowsServiceInstallInfo(Directory.GetCurrentDirectory(), "MyService.exe", @".\username", @"password");
            //If install with network user:
            //wsInstallInfo = new WindowsServiceInstallInfo(Directory.GetCurrentDirectory(), "MyService.exe", @"networkdomain\username", @"password");
            wsInstallInfo = new WindowsServiceInstallInfo(Directory.GetCurrentDirectory(), "MyService.exe", @".\IBMR40", @"");
            wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);

            //Log to see any error
            //wsInstallUtil.Install(@"C:\test.txt");
            result = wsInstallUtil.Install();
            Console.WriteLine("Installed : " + result);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            //Log to see any error
            //wsInstallUtil.Uninstall(@"C:\test.txt");
            result = wsInstallUtil.Uninstall();
            Console.WriteLine("Uninstalled : " + result);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            //
            // Installing windows service to start with username and password
            //
            //If install with local user:
            //wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", Directory.GetCurrentDirectory(), "MyService.exe", @".\username", @"password");
            //If install with network user:
            //wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", Directory.GetCurrentDirectory(), "MyService.exe", @"networkdomain\username", @"password");
            wsInstallInfo = new WindowsServiceInstallInfo("MyService_StartWithUsernameAndPassword", "This service will start with username and password", Directory.GetCurrentDirectory(), "MyService.exe", @".\IBMR40", @"");
            wsInstallUtil = new WindowsServiceInstallUtil(wsInstallInfo);

            //Log to see any error
            //wsInstallUtil.Install(@"C:\test.txt");
            result = wsInstallUtil.Install();
            Console.WriteLine("Installed : " + result);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            //Log to see any error
            //wsInstallUtil.Uninstall(@"C:\test.txt");
            result = wsInstallUtil.Uninstall();
            Console.WriteLine("Uninstalled : " + result);

            Console.WriteLine("Press any key to continue ...");
            Console.ReadKey();

            //
            // End of program
            //
            return;
        }
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace KeyServices
{
    /// <summary>
    /// 
    /// </summary>
    public enum WindowsServiceAccountType
    {
        /// <summary></summary>
        LocalService,
        /// <summary></summary>
        NetworkService,
        /// <summary></summary>
        LocalSystem,
        /// <summary></summary>
        User
    }

    /// <summary>
    /// 
    /// </summary>
    public class WindowsServiceInstallInfo
    {
        private string _windowsServiceName;
        private string _wsDescription;
        private readonly string _windowsServicePath;
        private readonly string _windowsServiceAssemblyName;
        private readonly WindowsServiceAccountType _wsAccountType;
        private readonly string _wsAccountUserName = "";
        private readonly string _wsAccountPassword = "";

        /// <summary>
        /// Please pass the windows service information in
        /// </summary>
        /// <param name="windowsServicePath">Path to folder where windows service assembly stored</param>
        /// <param name="windowsServiceAssemblyName">Windows Service Assembly Name</param>
        /// <param name="wsAccountType">Windows Service Account Type (not USER type)</param>
        public WindowsServiceInstallInfo(string windowsServicePath, string windowsServiceAssemblyName, WindowsServiceAccountType wsAccountType)
            : this("", windowsServicePath, windowsServiceAssemblyName, wsAccountType) { }

        /// <summary>
        /// Please pass the windows service information in
        /// </summary>
        /// <param name="windowsServiceName">Name of windows service</param>
        /// <param name="windowsServicePath">Path to folder where windows service assembly stored</param>
        /// <param name="windowsServiceAssemblyName">Windows Service Assembly Name</param>
        /// <param name="wsAccountType">Windows Service Account Type (not USER type)</param>
        public WindowsServiceInstallInfo(string windowsServiceName, string windowsServicePath, string windowsServiceAssemblyName, WindowsServiceAccountType wsAccountType)
            : this(windowsServiceName, "", windowsServicePath, windowsServiceAssemblyName, wsAccountType) { }

        /// <summary>
        /// Please pass the windows service information in
        /// </summary>
        /// <param name="windowsServiceName">Name of windows service</param>
        /// <param name="description">Description of windows service</param>
        /// <param name="windowsServicePath">Path to folder where windows service assembly stored</param>
        /// <param name="windowsServiceAssemblyName">Windows Service Assembly Name</param>
        /// <param name="wsAccountType">Windows Service Account Type (not USER type)</param>
        public WindowsServiceInstallInfo(string windowsServiceName, string description, string windowsServicePath, string windowsServiceAssemblyName, WindowsServiceAccountType wsAccountType)
            : this(windowsServiceName, description, windowsServicePath, windowsServiceAssemblyName, wsAccountType, "", "") { }

        /// <summary>
        /// Please pass the windows service information in
        /// </summary>
        /// <param name="windowsServicePath">Path to folder where windows service assembly stored</param>
        /// <param name="windowsServiceAssemblyName">Windows Service Assembly Name</param>        
        /// <param name="wsAccountUserName">Username of Windows Service when Account Type is USER</param>
        /// <param name="wsAccountPassword">Password of Windows Service when Account Type is USER</param>
        public WindowsServiceInstallInfo(string windowsServicePath, string windowsServiceAssemblyName, string wsAccountUserName, string wsAccountPassword)
            : this("", windowsServicePath, windowsServiceAssemblyName, wsAccountUserName, wsAccountPassword) { }

        /// <summary>
        /// Please pass the windows service information in
        /// </summary>
        /// <param name="windowsServiceName">Name of windows service</param>
        /// <param name="windowsServicePath">Path to folder where windows service assembly stored</param>
        /// <param name="windowsServiceAssemblyName">Windows Service Assembly Name</param>
        /// <param name="wsAccountUserName">Username of Windows Service when Account Type is USER</param>
        /// <param name="wsAccountPassword">Password of Windows Service when Account Type is USER</param>
        public WindowsServiceInstallInfo(string windowsServiceName, string windowsServicePath, string windowsServiceAssemblyName, string wsAccountUserName, string wsAccountPassword)
            : this(windowsServiceName, "", windowsServicePath, windowsServiceAssemblyName, wsAccountUserName, wsAccountPassword) { }

        /// <summary>
        /// Please pass the windows service information in
        /// </summary>
        /// <param name="windowsServiceName">Name of windows service</param>
        /// <param name="description">Description of windows service</param>
        /// <param name="windowsServicePath">Path to folder where windows service assembly stored</param>
        /// <param name="windowsServiceAssemblyName">Windows Service Assembly Name</param>        
        /// <param name="wsAccountUserName">Username of Windows Service when Account Type is USER</param>
        /// <param name="wsAccountPassword">Password of Windows Service when Account Type is USER</param>
        public WindowsServiceInstallInfo(string windowsServiceName, string description, string windowsServicePath, string windowsServiceAssemblyName, string wsAccountUserName, string wsAccountPassword)
            : this(windowsServiceName, description, windowsServicePath, windowsServiceAssemblyName, WindowsServiceAccountType.User, wsAccountUserName, wsAccountPassword) { }

        /// <summary>
        /// Please pass the windows service information in
        /// </summary>
        /// <param name="windowsServiceName">Name of windows service</param>
        /// <param name="description">Description of windows service</param>
        /// <param name="windowsServicePath">Path to folder where windows service assembly stored</param>
        /// <param name="windowsServiceAssemblyName">Windows Service Assembly Name</param>
        /// <param name="wsAccountType">Windows Service Account Type</param>
        /// <param name="wsAccountUserName">Username of Windows Service when Account Type is USER</param>
        /// <param name="wsAccountPassword">Password of Windows Service when Account Type is USER</param>
        private WindowsServiceInstallInfo(string windowsServiceName, string description, string windowsServicePath, string windowsServiceAssemblyName, WindowsServiceAccountType wsAccountType, string wsAccountUserName, string wsAccountPassword)
        {
            _windowsServiceName = windowsServiceName.Trim();
            _wsDescription = description.Trim();
            _windowsServicePath = windowsServicePath;
            _windowsServiceAssemblyName = windowsServiceAssemblyName;
            _wsAccountType = wsAccountType;
            _wsAccountUserName = wsAccountUserName;
            _wsAccountPassword = wsAccountPassword;

            if (_wsAccountType == WindowsServiceAccountType.User && _wsAccountUserName == "")
            {
                throw new Exception("Username has to be provided if AccountType to start the windows service is USER");
            }
        }

        /// <summary>
        /// Name of windows service
        /// </summary>
        public string WindowsServiceName
        {
            get { return _windowsServiceName; }
            set { _windowsServiceName = value; }
        }

        /// <summary>
        /// Description of windows service
        /// </summary>
        public string Description
        {
            get { return _wsDescription; }
            set { _wsDescription = value; }
        }

        /// <summary>
        /// Path to folder which contains windows service binary
        /// </summary>
        public string WindowsServicePath
        {
            get { return _windowsServicePath; }
        }
        /// <summary>
        /// Windows service binary file name
        /// </summary>
        public string WindowsServiceAssemblyName
        {
            get { return _windowsServiceAssemblyName; }
        }
        /// <summary>
        /// Account type to start windows service
        /// </summary>
        public WindowsServiceAccountType WsAccountType
        {
            get { return _wsAccountType; }
        }
        /// <summary>
        /// Username to start windows service (if account type is User)
        /// </summary>
        public string WsAccountUserName
        {
            get { return _wsAccountUserName; }
        }
        /// <summary>
        /// Password of username to start windows service (if account type is User)
        /// </summary>
        public string WsAccountPassword
        {
            get { return _wsAccountPassword; }
        }
    }


    /// <summary>
    /// To use this class for installing windows service, you need a project installer (which inherits from DynamicInstaller) added to your assembly
    /// </summary>
    public class WindowsServiceInstallUtil
    {
        /// <summary>
        /// Path to folder where InstallUtil (.Net SDK) stored
        /// </summary>
        public static string InstallUtilPath = System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(); //@"C:\WINDOWS\Microsoft.NET\Framework\v2.0.50727";

        protected WindowsServiceInstallInfo _wsInstallInfo;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wsInstallInfo"></param>
        public WindowsServiceInstallUtil(WindowsServiceInstallInfo wsInstallInfo)
        {
            _wsInstallInfo = wsInstallInfo;
        }


        /// <summary>
        /// Run InstallUtil with specific params
        /// </summary>
        /// <param name="installUtilArguments">CommandLine params</param>
        /// <returns>Status of installation</returns>
        private static bool CallInstallUtil(string installUtilArguments)
        {
            Process proc = new Process();
            proc.StartInfo.FileName = Path.Combine(InstallUtilPath, "installutil.exe");
            proc.StartInfo.Arguments = installUtilArguments;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.UseShellExecute = false;

            proc.Start();
            string outputResult = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            // check result
            if (proc.ExitCode != 0)
            {
                //Console.WriteLine("{0} : failed with code {1}", DateTime.Now, proc.ExitCode);
                //Console.WriteLine(outputResult);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Install windows service
        /// </summary>
        /// <returns></returns>
        public bool Install()
        {
            return Install("");
        }
        /// <summary>
        /// Install windows service
        /// </summary>
        /// <param name="logFilePath">Log file to store installation information</param>
        /// <returns></returns>
        public virtual bool Install(string logFilePath)
        {
            string installUtilArguments = GenerateInstallutilInstallArgs(logFilePath);

            return CallInstallUtil(installUtilArguments);
        }
        protected string GenerateInstallutilInstallArgs(string logFilePath)
        {
            string installUtilArguments = " /account=\"" + _wsInstallInfo.WsAccountType + "\"";
            if (_wsInstallInfo.WindowsServiceName != "")
            {
                installUtilArguments += " /name=\"" + _wsInstallInfo.WindowsServiceName + "\"";
            }
            if (_wsInstallInfo.Description != "")
            {
                installUtilArguments += " /desc=\"" + _wsInstallInfo.Description + "\"";
            }
            if (_wsInstallInfo.WsAccountType == WindowsServiceAccountType.User)
            {
                installUtilArguments += " /user=\"" + _wsInstallInfo.WsAccountUserName + "\" /password=\"" + _wsInstallInfo.WsAccountPassword + "\"";
            }
            installUtilArguments += " \"" + Path.Combine(_wsInstallInfo.WindowsServicePath, _wsInstallInfo.WindowsServiceAssemblyName) + "\"";
            if (logFilePath.Trim() != "")
            {
                installUtilArguments += " /LogFile=\"" + logFilePath + "\"";
            }

            return installUtilArguments;
        }

        /// <summary>
        /// Uninstall windows service
        /// </summary>
        /// <returns></returns>
        public bool Uninstall()
        {
            return Uninstall("");
        }
        /// <summary>
        /// Uninstall windows service
        /// </summary>
        /// <param name="logFilePath">Log file to store installation information</param>
        /// <returns></returns>
        public virtual bool Uninstall(string logFilePath)
        {
            string installUtilArguments = GenerateInstallutilUninstallArgs(logFilePath);

            return CallInstallUtil(installUtilArguments);
        }

        protected string GenerateInstallutilUninstallArgs(string logFilePath)
        {
            string installUtilArguments = " /u ";
            if (_wsInstallInfo.WindowsServiceName != "")
            {
                installUtilArguments += " /name=\"" + _wsInstallInfo.WindowsServiceName + "\"";
            }
            installUtilArguments += " \"" + Path.Combine(_wsInstallInfo.WindowsServicePath, _wsInstallInfo.WindowsServiceAssemblyName) + "\"";
            if (logFilePath.Trim() != "")
            {
                installUtilArguments += " /LogFile=\"" + logFilePath + "\"";
            }

            return installUtilArguments;
        }
    }

    /// <summary>
    /// Install windows service in local machine or remote machine
    /// To use this class for installing windows service, you need a project installer module added to your assembly
    /// </summary>
    public class RemoteWindowsServiceInstallUtil : WindowsServiceInstallUtil
    {
        private readonly string _remoteMachine;

        public RemoteWindowsServiceInstallUtil(WindowsServiceInstallInfo wsInstallInfo) : this("localhost", wsInstallInfo) { }
        public RemoteWindowsServiceInstallUtil(string remoteMachine, WindowsServiceInstallInfo wsInstallInfo)
            : base(wsInstallInfo)
        {
            _remoteMachine = remoteMachine;
        }

        private static string _psToolsPath = Path.Combine(Directory.GetCurrentDirectory(), "PsTools");
        ///<summary>
        ///</summary>
        public static string PsToolsPath
        {
            get { return _psToolsPath; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("value", "Cannot provide a null PS tools path.");
                if (value.Trim().Length == 0)
                    throw new ArgumentException("Cannot provide an empty PS tools path.", "value");

                if (_psToolsPath == value)
                    return;

                string psExecPath = Path.Combine(value, "psexec.exe");

                if (!File.Exists(psExecPath))
                    throw new FileNotFoundException("Cannot file the PS exec file: " + psExecPath);

                //string psKillPath = Path.Combine(value, "pskill.exe");

                //if (!File.Exists(psKillPath))
                //    throw new FileNotFoundException("Cannot file the PS kill file: " + psKillPath);

                _psToolsPath = value;
            }
        }

        /// <summary>
        /// Check if param passed in is a local machine
        /// </summary>
        /// <param name="machineNameOrIpAddress"></param>
        /// <returns>true : local machine, false : Remote Machine</returns>
        public static bool IsLocalMachine(string machineNameOrIpAddress)
        {
            if (machineNameOrIpAddress == "localhost") return true;

            if (machineNameOrIpAddress == null) throw new NullReferenceException("Machine name cannot be null");

            string tmpMachine = machineNameOrIpAddress.ToLower();

            if (tmpMachine == Dns.GetHostName().ToLower()) return true;

            foreach (IPAddress ipaddress in Dns.GetHostAddresses(Dns.GetHostName()))
            {
                if (tmpMachine == ipaddress.ToString().ToLower()) return true;
            }

            return false;
        }

        /// <summary>
        /// Run InstallUtil in remote machine with specific params
        /// </summary>
        /// <param name="installUtilArguments">CommandLine params</param>
        /// <returns>Status of installation</returns>
        private bool CallRemoteInstallUtil(string installUtilArguments)
        {
            string loginInfo = "\\\\" + _remoteMachine;
            loginInfo += (_wsInstallInfo.WsAccountUserName != String.Empty)
                         ? " -u \"" + _wsInstallInfo.WsAccountUserName + "\" -p \"" + _wsInstallInfo.WsAccountPassword + "\""
                         : "";

            Process proc = new Process();
            proc.StartInfo.FileName = Path.Combine(PsToolsPath, "psexec.exe");
            proc.StartInfo.Arguments = loginInfo + " \"" + Path.Combine(InstallUtilPath, "installutil.exe") + "\" " + installUtilArguments;
            proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //proc.StartInfo.RedirectStandardOutput = true; //DONT DO THIS, BECAUSE Windows Service uses this class will hang (psexec doesn't exist)
            proc.StartInfo.UseShellExecute = false;

            //Console.WriteLine(proc.StartInfo.FileName + " " + proc.StartInfo.Arguments);

            proc.Start();
            //string outputResult = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            // check result
            if (proc.ExitCode != 0)
            {
                //Console.WriteLine("{0} : failed with code {1}", DateTime.Now, proc.ExitCode);
                //Console.WriteLine(outputResult);
                return false;
            }

            return true;
        }

        public override bool Install(string logFilePath)
        {
            if (IsLocalMachine(_remoteMachine))
            {
                //Call InstallUtil in the same machine
                return base.Install(logFilePath);
            }
            else
            {
                //Call InstallUtil in the remote machine
                string installUtilArguments = GenerateInstallutilInstallArgs(logFilePath);
                return CallRemoteInstallUtil(installUtilArguments);
            }
        }

        public override bool Uninstall(string logFilePath)
        {
            if (IsLocalMachine(_remoteMachine))
            {
                //Call InstallUtil in the same machine
                return base.Uninstall(logFilePath);
            }
            else
            {
                //Call InstallUtil in the remote machine
                string installUtilArguments = GenerateInstallutilUninstallArgs(logFilePath);
                return CallRemoteInstallUtil(installUtilArguments);
            }
        }
    }

}