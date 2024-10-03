using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using RR.Core.EventLayer;

namespace BerserkV3.Startup.Events
{
	public class StartupBus : EventBus
	{
		static StartupBus()
		{
			InitFields<StartupBus>();
			AddLoadingTask.HideInLog = true;
			AddLoadingUnitask.HideInLog = true;
		}

		public static State<bool> IsAuthorizing;
		public static RREvent<string> OnAuthProcess;
		public static RREvent<string> OnPINCodeCreated;
		
		public static RREvent<Task> AddLoadingTask;
		public static RREvent<UniTask> AddLoadingUnitask;
		public static RREvent<bool> ReportAvailable;
		public static State<bool> TestFly;
		public static float LastRequestTime;
	}
}