using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using PIDataReaderLib;
using System.IO;
using PIDataReaderCommons;
using System.Threading;
using System.Timers;

/*
 * PROCESS
 * 1. Install as service using the following instruction:
 *		sc.exe create PIDataReaderSvcTest binPath= "D:\Projects\DOTNET\PIDataReaderApps\PIDataReaderSvc\bin\Debug\PIDataReaderSvc.exe -c D:\Projects\DOTNET\PIDataReaderApps\SampleConfigFiles\config.tags.xml -f none"
 * 2. Open the service console and setup the service
 *		General
 *			Startup type: Automatic
 *		Log On
 *			Logon as: \EMA\XFDIAOSIPI with appropriate password
 *		Recovery
 *			First failure: Restart the Service
 *			Second failure: Restart the Service
 *			Subsequent failures: Restart the Service
 * 3. Start the service from the service console
 * 
 * */

/*
* Installato come servizio usando 
*	sc.exe create PIDataReaderSvc binPath= "D:\Projects\DOTNET\PIDataReaderSvc\bin\Debug\PIDataReaderSvc.exe" (REMEMBER THE WHITE SPACE AFTER THE = SIGN!!!!)
*	sc.exe create PIDataReaderSvc binPath= "D:\Projects\DOTNET\PIDataReaderSvc\bin\Debug\PIDataReaderSvc.exe -c D:\Projects\DOTNET\PIDataReaderSvc\config.tags.xml -f none"
* Parte con sc start PIDataReaderSvc -c D:\Projects\DOTNET\PIDataReaderSvc\config.tags.xml -f none
* 
* Eliminare servizio con sc delete TestService01
* */
namespace PIDataReaderSvc {
	public partial class PIDataReaderSvc : ServiceBase {
		private static Logger logger = LogManager.GetCurrentClassLogger();
		
		public enum ServiceState {
			SERVICE_STOPPED = 0x00000001,
			SERVICE_START_PENDING = 0x00000002,
			SERVICE_STOP_PENDING = 0x00000003,
			SERVICE_RUNNING = 0x00000004,
			SERVICE_CONTINUE_PENDING = 0x00000005,
			SERVICE_PAUSE_PENDING = 0x00000006,
			SERVICE_PAUSED = 0x00000007,
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct ServiceStatus {
			public long dwServiceType;
			public ServiceState dwCurrentState;
			public long dwControlsAccepted;
			public long dwWin32ExitCode;
			public long dwServiceSpecificExitCode;
			public long dwCheckPoint;
			public long dwWaitHint;
		};

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

		PIDRController piDataReaderCtrl;

		public PIDataReaderSvc() {
			InitializeComponent();
		}

		protected override void OnStart(string[] args) {
			ServiceStatus serviceStatus = new ServiceStatus();
			serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
			serviceStatus.dwWaitHint = 100000;
			SetServiceStatus(this.ServiceHandle, ref serviceStatus);

			//WRITE CODE TO INITIALIZE AND START SERVICE
			piDataReaderCtrl = new PIDRController(Version.getVersion(), PIDataReaderLib.Version.getVersion(), true);

			List<string> al = new List<string>(Environment.GetCommandLineArgs());
			al.RemoveAt(0);
			string[] commandLineArgs = al.ToArray<string>();

			int res = piDataReaderCtrl.start(commandLineArgs);
			if (ExitCodes.EXITCODE_SUCCESS != res) {
				logger.Fatal("Failed to start service! Reason: {0}", res);
				Stop();
			}
			
			serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
			SetServiceStatus(this.ServiceHandle, ref serviceStatus);
		}

		protected override void OnStop() {
			ServiceStatus serviceStatus = new ServiceStatus();
			serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
			serviceStatus.dwWaitHint = 100000;
			SetServiceStatus(this.ServiceHandle, ref serviceStatus);

			//DELIBERATELY NOT TRYING TO CLOSE MQTT CLIENT GRACEFULLY: WE WANT IT TO SEND A LAST WILL MESSAGE!

			piDataReaderCtrl.stop();
			logger.Info("Service was stopped correctly.");

			piDataReaderCtrl.sendMail();

			serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
			SetServiceStatus(this.ServiceHandle, ref serviceStatus);
		}
		
	}
}
