using System;

namespace BerserkV3.Lobby.UI.Home.CommonWidgets.Models
{
	public class TimerModel
	{
		public DateTime EndTime { get; private set; }
		public string Prefix { get; private set; }
		public string Suffix { get; private set; }

		public TimerModel(DateTime endTime, string prefix = "", string suffix = "")
		{
			EndTime = endTime;
			Prefix = prefix;
			Suffix = suffix;
		}
	}
}