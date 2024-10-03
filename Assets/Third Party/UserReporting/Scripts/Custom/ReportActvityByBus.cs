using BerserkV3.Startup.Events;
using UnityEngine;

namespace Third_Party.UserReporting.Scripts.Custom
{

	public class ReportActvityByBus : MonoBehaviour
	{
		private GameObject self;
		private void Awake()
		{
			self = gameObject;
			StartupBus.ReportAvailable.SubscribeRaw(OnActivityChanged);
		}

		private void OnActivityChanged(bool value)
		{
			if (self && self.activeSelf != value)
				gameObject.SetActive(value);
		}

		private void OnDestroy()
		{
			StartupBus.ReportAvailable.Unsubscribe(OnActivityChanged);
			self = null;
		}
	}

}