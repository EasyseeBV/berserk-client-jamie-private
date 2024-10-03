using UnityEngine;
using System.Runtime.InteropServices;

public class WebGLSaveTextFile : MonoBehaviour
{
	#if UNITY_WEBGL
	[DllImport("__Internal")]
	private static extern void download(string content, string fileName, string contentType);
	#endif

	public static void Save(string content, string fileName)
	{
		#if !UNITY_EDITOR && UNITY_WEBGL
			download(content, fileName, "text/plain");
		#endif
	}
}