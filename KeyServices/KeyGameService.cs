using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;

namespace KeyServices
{
	public class KeyGameService : KeyChildServiceBase
	{
		public KeyGameService()
		{
			this.ServiceName = "KeyGameSvc";
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			base.OnStart( args );
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			base.OnStop();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected override int Execute() 
        {
			
			// for right now we'll just log a message in the Application message log to let us know that
			// our service is working
			System.Diagnostics.EventLog.WriteEntry(ServiceName, ServiceName + "::Execute()");

			return 0;
		}

	}
}
