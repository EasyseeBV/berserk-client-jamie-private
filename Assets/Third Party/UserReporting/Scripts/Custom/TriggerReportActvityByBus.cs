using System;
using BerserkV3.Startup.Events;
using UnityEngine;

namespace Third_Party.UserReporting.Scripts.Custom
{

	public class TriggerReportActvityByBus : MonoBehaviour
	{
		private void OnEnable()
		{
			StartupBus.ReportAvailable += false;
		}

		private void OnDisable()
		{
			StartupBus.ReportAvailable += true;
		}

		private void OnDestroy()
		{
			StartupBus.ReportAvailable += true;
		}
	}

}