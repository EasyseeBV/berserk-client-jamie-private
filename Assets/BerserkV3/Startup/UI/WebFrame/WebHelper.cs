using UnityEngine;
using Application = UnityEngine.Device.Application;

namespace BerserkV3.Startup.UI
{
	public static class WebHelper
	{
		public static float GetResolutionScale(bool isMobileView)
		{
			return isMobileView ? 1.25f : 0.7f;
		}
		
		public static string GetUserAgent(bool isMobileView = false)
		{
			switch (Application.platform, isMobileView)
			{
				case (RuntimePlatform.tvOS, true) :
				case (RuntimePlatform.IPhonePlayer, true) :
				case (RuntimePlatform.OSXEditor, true) : 
				case (RuntimePlatform.OSXPlayer, true) : 
					return "Mozilla/5.0 (iPhone; CPU iPhone OS 16_6 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.5 Mobile/15E148 Safari/604.1";
				
				case (RuntimePlatform.tvOS, false) :
				case (RuntimePlatform.IPhonePlayer, false) :
				case (RuntimePlatform.OSXEditor, false) : 
				case (RuntimePlatform.OSXPlayer, false) : 
					return "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_5) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/16.5 Safari/605.1.15";

				default:
					return isMobileView ? 
						"Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.5790.166 Mobile Safari/537.36" 
						: "Mozilla/5.0 (Windows; rv:geckoversion) Gecko/geckotrail Firefox/firefoxversion";
			}
		}
	}
}