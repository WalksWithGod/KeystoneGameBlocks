using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;
using Microsoft.Win32;
namespace ServiceInstaller
{
    /// <summary>
    /// http://www.codeproject.com/KB/cs/DynWinServiceInstallUtil.aspx?display=Print
    /// This is a custom project installer.
    /// Applies a unique name to the service using the /name switch
    /// Sets user name and password using the /user and /password switches
    /// Allows the use of a local account using the /account switch
    /// </summary>
    [RunInstaller(true)]
    public class DynamicInstaller : Installer
    {
        private ServiceProcessInstaller processInstaller;
        private System.ServiceProcess.ServiceInstaller serviceInstaller;

        public DynamicInstaller()
        {
            processInstaller = new ServiceProcessInstaller();
            processInstaller.Account = ServiceAccount.LocalService;
            processInstaller.Username = null;
            processInstaller.Password = null;
            serviceInstaller = new System.ServiceProcess.ServiceInstaller();
            serviceInstaller.StartType = ServiceStartMode.Automatic;
            serviceInstaller.ServiceName = "KeyAuthenticationSvc";
            serviceInstaller.DisplayName = "";
            serviceInstaller.Description = "";
            Installers.AddRange(new Installer[] {
            processInstaller,
            serviceInstaller});
        }

        public string ServiceName
        {
            get { return serviceInstaller.ServiceName; }
            set { serviceInstaller.ServiceName = value; }
        }
        public string DisplayName
        {
            get { return serviceInstaller.DisplayName; }
            set { serviceInstaller.DisplayName = value; }
        }
        public string Description
        {
            get { return serviceInstaller.Description; }
            set { serviceInstaller.Description = value; }
        }
        public ServiceStartMode StartType
        {
            get { return serviceInstaller.StartType; }
            set { serviceInstaller.StartType = value; }
        }
        public ServiceAccount Account
        {
            get { return processInstaller.Account; }
            set { processInstaller.Account = value; }
        }
        public string ServiceUsername
        {
            get { return processInstaller.Username; }
            set { processInstaller.Username = value; }
        }
        public string ServicePassword
        {
            get { return processInstaller.Password; }
            set { processInstaller.Password = value; }
        }

        #region Access parameters
        /// <summary>
        /// Return the value of the parameter in dictated by key
        /// </summary>
        /// <PARAM name="key">Context parameter key</PARAM>
        /// <returns>Context parameter specified by key</returns>
        public string GetContextParameter(string key)
        {
            string sValue = "";
            try
            {
                sValue = this.Context.Parameters[key].ToString();
            }
            catch
            {
                sValue = "";
            }
            return sValue;
        }
        #endregion
        /// <summary>
        /// This method is run before the install process.
        /// This method is overridden to set the following parameters:
        /// service name (/name switch)
        /// account type (/account switch)
        /// for a user account user name (/user switch)
        /// for a user account password (/password switch)
        /// Note that when using a user account,
        /// if the user name or password is not set,
        /// the installing user is prompted for the credentials to use.
        /// </summary>
        /// <PARAM name="savedState"></PARAM>
        protected override void OnBeforeInstall(IDictionary savedState)
        {
            base.OnBeforeInstall(savedState);
            bool isUserAccount = false;

            // Decode the command line switches
            string name = GetContextParameter("name").Trim();
            if (name != "")
            {
                serviceInstaller.ServiceName = name;
            }
            string desc = GetContextParameter("desc").Trim();
            if (desc != "")
            {
                serviceInstaller.Description = desc;
            }
            // What type of credentials to use to run the service
            string acct = GetContextParameter("account");
            switch (acct.ToLower())
            {
                case "user":
                    processInstaller.Account = ServiceAccount.User;
                    isUserAccount = true;
                    break;
                case "localservice":
                    processInstaller.Account = ServiceAccount.LocalService;
                    break;
                case "localsystem":
                    processInstaller.Account = ServiceAccount.LocalSystem;
                    break;
                case "networkservice":
                    processInstaller.Account = ServiceAccount.NetworkService;
                    break;
            }
            // User name and password
            string username = GetContextParameter("user").Trim();
            string password = GetContextParameter("password").Trim();
            // Should I use a user account?

            if (isUserAccount)
            {
                // If we need to use a user account
                // set the user name and password

                if (username != "")
                {
                    processInstaller.Username = username;
                }
                if (password != "")
                {
                    processInstaller.Password = password;
                }
            }
        }
        /// <summary>
        /// Uninstall based on the service name
        /// </summary>
        /// <PARAM name="savedState"></PARAM>
        protected override void OnBeforeUninstall(IDictionary savedState)
        {
            base.OnBeforeUninstall(savedState);
            // Set the service name based on the command line
            string name = GetContextParameter("name").Trim();
            if (name != "")
            {
                serviceInstaller.ServiceName = name;
            }
        }
    }
}


// installService.bat
// --------------------------
//@echo off
//set SERVICE_HOME=<service executable directory>
//set SERVICE_EXE=<service executable name>
//REM the following directory is for .NET 1.1, your mileage may vary
//set INSTALL_UTIL_HOME=C:\WINNT\Microsoft.NET\Framework\v1.1.4322
//REM Account credentials if the service uses a user account
//set USER_NAME=<user account>
//set PASSWORD=<user password>

//set PATH=%PATH%;%INSTALL_UTIL_HOME%

//cd %SERVICE_HOME%

//echo Installing Service...
//installutil /name=<service name>
//  /account=<account type> /user=%USER_NAME% /password=%

//PASSWORD% %SERVICE_EXE%
//echo Done.

// uninstallService.bat
// --------------------------
//@echo off
//set SERVICE_HOME=<service executable directory>
//set SERVICE_EXE=<service executable name>
//REM the following directory is for .NET 1.1, your mileage may vary
//set INSTALL_UTIL_HOME=C:\WINNT\Microsoft.NET\Framework\v1.1.4322

//set PATH=%PATH%;%INSTALL_UTIL_HOME%

//cd %SERVICE_HOME%

//echo Uninstalling Service...
//installutil /u /name=<service name> %SERVICE_EXE%
//echo Done.



//using System;
//using System.Collections;
//using System.ComponentModel;
//using System.Configuration.Install;
//using System.ServiceProcess;
//namespace KeyServices
//{
//    /// <summary>
//    /// Summary description for Keystone Game Blocks Services Installer.
//    /// </summary>
//    [RunInstaller(true)]
//    public class KeyServiceInstaller : System.Configuration.Install.Installer
//    {
//        public KeyServiceInstaller()
//        {
//            ServiceProcessInstaller process = new ServiceProcessInstaller();

//            process.Account = ServiceAccount.LocalSystem;

//            ServiceInstaller serviceAdmin = new ServiceInstaller();

//            serviceAdmin.StartType		= ServiceStartMode.Manual;
//            serviceAdmin.ServiceName	= "KeyAdminSvc";
//            serviceAdmin.DisplayName	= "Keystone Game Blocks Administration Service";
			
//            ServiceInstaller serviceLogin = new ServiceInstaller();

//            serviceLogin.StartType		= ServiceStartMode.Manual;
//            serviceLogin.ServiceName	= "KeyLoginSvc";
//            serviceLogin.DisplayName	= "Keyston Game Blocks Login Service";
//            serviceLogin.ServicesDependedOn	= new string[] { "KeyAdminSvc" };

//            ServiceInstaller serviceGame = new ServiceInstaller();

//            serviceGame.StartType		= ServiceStartMode.Manual;
//            serviceGame.ServiceName		= "KeyGameSvc";
//            serviceGame.DisplayName		= "Keystone Game Blocks Game Service";
//            serviceGame.ServicesDependedOn	= new string[]{ "KeyAdminSvc"  };
			
//            ServiceInstaller serviceLobby = new ServiceInstaller();

//            serviceLobby.StartType		= ServiceStartMode.Manual;
//            serviceLobby.ServiceName	= "KeyLobbySvc";
//            serviceLobby.DisplayName	= "Keystone Game Blocks Lobby Service";
//            serviceLobby.ServicesDependedOn	= new string[] { "KeyAdminSvc" };
			
//            // Microsoft didn't add the ability to add a description for the services we are going to install
//            // To work around this we'll have to add the information directly to the registry but I'll leave
//            // this exercise for later.


//            // now just add the installers that we created to our parents container, the documentation
//            // states that there is not any order that you need to worry about here but I'll still
//            // go ahead and add them in the order that makes sense.
//            Installers.Add( process );
//            Installers.Add( serviceAdmin );
//            Installers.Add( serviceLogin );
//            Installers.Add( serviceGame );
//            Installers.Add( serviceLobby );
//        }

//    }
//}
