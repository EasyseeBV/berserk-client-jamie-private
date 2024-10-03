using System.Threading.Tasks;
using BerserkV3.Startup.Abstractions;
using Cysharp.Threading.Tasks;
using RR.UI.FrameSystem;
using UI;
using UnityEngine;

namespace BerserkV3.Startup.Applications
{
	public class MessageApplication : IMessageApplication
	{
		public async Task Info(string message, string okText = null, string title = null)
		{
			if (!Application.isPlaying)
				return;
			
			await UniTask.WaitWhile(() => ConfirmationDialog.Instance.VisibleState == VisibleState.Visible);
			var tcs = new TaskCompletionSource<bool>();
			ConfirmationDialog.Instance.Init()
				.SetMessage(message)
				.SetAnyResponse(() => tcs.SetResult(false))
				.SetTitle(title ?? "Information")
				.SetOk(okText ?? "Try again")
				.SetCancel()
				.Apply();
			await tcs.Task;
			await UniTask.WaitWhile(() => ConfirmationDialog.Instance.VisibleState == VisibleState.Visible);
		}

		public async Task Critial(string mesage = null, string okText = null, string title = null)
		{
			if (!Application.isPlaying)
				return;

			mesage ??= "Something went wrong. Please make a report.";
			await Info(mesage, okText ?? "Quit", title);
			Application.Quit(0);
		}
	}
}