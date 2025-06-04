using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;

namespace KeyServices
{
    /// <summary>
    /// A child service is any service that depends on another service to be currently running.
    /// Thus a child service can attempt to start the dependant service if it determines it's not running.
    /// Our KeyLobbyService is a good example.  It dervives from KeyChildServiceBase and 
    /// is dependant on the KeyAuthentication service to be running first.
    /// </summary>
	public abstract class KeyChildServiceBase : KeyServiceBase
	{
        protected KeyChildServiceBase()
		{
			// TODO: Add any initialization after the InitComponent call
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			// gain access to the KeyAdminSvc on the local computer.
			ServiceController controlAdmin = new ServiceController("KeyAdminSvc", ".");

			if( controlAdmin.Status != System.ServiceProcess.ServiceControllerStatus.Running &&
				controlAdmin.Status	!= ServiceControllerStatus.StartPending ) {
				
				// start the admin service
				controlAdmin.Start();
			}

			base.OnStart( args );
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			// TODO: Add code here to perform any tear-down necessary to stop your service.
			base.OnStop();
		}
	}
}
