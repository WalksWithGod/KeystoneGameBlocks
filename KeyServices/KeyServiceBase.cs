using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;

namespace KeyServices
{
	public class KeyServiceBase : System.ServiceProcess.ServiceBase
	{
        public KeyServiceBase() 
        {
			// create a new timespan object with a default of 10 seconds delay.
			m_delay = new TimeSpan(0, 0, 0, 10, 0 );
		}

		/// <summary>
		/// Set things in motion so your service can do its work.
		/// </summary>
		protected override void OnStart(string[] args) {
			// create our threadstart object to wrap our delegate method
			ThreadStart ts = new ThreadStart( this.ServiceMain );

			// create the manual reset event and set it to an initial state of unsignaled
			m_shutdownEvent = new ManualResetEvent(false);

			// create the worker thread
			m_thread = new Thread( ts );

			// go ahead and start the worker thread
			m_thread.Start();

			// call the base class so it has a chance to perform any work it needs to
			base.OnStart( args );
		}
 
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop() {
			// signal the event to shutdown
			m_shutdownEvent.Set();

			// wait for the thread to stop giving it 10 seconds
			m_thread.Join(10000);

			// call the base class 
			base.OnStop();
		}

		/// <summary>
		/// This main loop for the service which is running in a seperate thread.
		/// </summary>
		protected void ServiceMain() {
			bool		bSignaled	= false;
			int			nReturnCode	= 0;
			
			while( true ) 
            {
				// wait for the event to be signaled or for the configured delay
				bSignaled = m_shutdownEvent.WaitOne( m_delay, true );

				// if we were signaled to shutdow, exit the loop
				if( bSignaled == true )
					break;

				// let's do some work
				nReturnCode = Execute();
			}

			Thread.CurrentThread.Abort();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		protected virtual int Execute() 
        {
			return -1;
		}

		protected Thread			m_thread;
		protected ManualResetEvent	m_shutdownEvent;
		protected TimeSpan			m_delay;
	}
}
