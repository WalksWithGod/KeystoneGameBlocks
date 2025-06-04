using System;
using System.ServiceProcess;

namespace KeyServices
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
			// we'll go ahead and create an array so that we can add the different services that
			// we'll create over time.
			ServiceBase[]	servicesToRun;

			// to create a new instance of a new service, just add it to the list of services 
			// specified in the ServiceBase array constructor.
			servicesToRun = new ServiceBase[] { new KeyAdminService(), 
												new KeyLoginService(),
												new KeyGameService(),
												new KeyLobbyService()
											  };

			// now run all the service that we have created. This doesn't actually cause the services
			// to run but it registers the services with the Service Control Manager so that it can
			// when you start the service the SCM will call the OnStart method of the service.
			ServiceBase.Run( servicesToRun );
		}
	}
}
