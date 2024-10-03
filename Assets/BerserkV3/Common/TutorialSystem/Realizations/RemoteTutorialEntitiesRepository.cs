using System.Collections.Generic;
using System.Threading.Tasks;
using Berserk.Shared.Data.Tutor;
using BerserkV3.Common.DataBase;
using BerserkV3.Startup.Utils;
using Newtonsoft.Json;
using RR.Game.TutorialSystemV2.Data;
using RR.Game.TutorialSystemV2.Realizations;
using UI;
using UnityEngine;

namespace BerserkV3.Common.TutorialSystem
{

	public class RemoteTutorialEntitiesRepository : DefaultTutorialEntitiesRepository
	{
		public override async Task InitAsync()
		{
			if (Initialized)
				return;
			
			try
			{
				await TaskUtil.RetryAsync(TryGetTutorialConfigAsync);
				Initialized = true;
			}
			catch
			{
				await Info(GameDataBaseAdapter.Instance.GetLocalization("ErrorTutorial"), "OK");
				Initialized = false;
			}
		}

		private async Task<TryResult> TryGetTutorialConfigAsync()
		{
			var response = await TutorialAPI.GetConfig();
			if (string.IsNullOrEmpty(response.Data))
			{
				await Info(GameDataBaseAdapter.Instance.GetLocalization("ErrorTutorialTryAgain"));
				return TryResult.Retry;
			}

			var data = JsonConvert.DeserializeObject<List<TutorHintModel>>(response.Data)!;
			data.ForEach(x => Entities.Add(new DefaultTutorialHintEntity(JsonConvert.DeserializeObject<TutorialBlock>(x.Data))));
			return TryResult.Success;
		}

		private static async Task Info(string message, string okText = null, string title = null)
		{
			if (!Application.isPlaying)
				return;

			var tcs = new TaskCompletionSource<bool>();
			ConfirmationDialog.Instance.Init()
				.SetMessage(message)
				.SetAnyResponse(() => tcs.SetResult(false))
				.SetTitle(title ?? "Information")
				.SetOk(okText ?? "Try again")
				.SetCancel()
				.Apply();
			await tcs.Task;
		}
	}

}