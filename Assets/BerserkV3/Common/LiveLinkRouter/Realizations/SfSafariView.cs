namespace BerserkV3.Common.LiveLinkRouter
{
	public static class SfSafariView
	{
#if !UNITY_EDITOR && UNITY_IOS
	    [System.Runtime.InteropServices.DllImport("__Internal")]
	    extern static void launchUrl(string url);
	    [System.Runtime.InteropServices.DllImport("__Internal")]
	    extern static void dismiss();

		public static void OpenURL(string url)
		{
			launchUrl(url);
		}

		public static void Dismiss()
		{
			dismiss();
		}
#endif
	}
}