using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Berserk.Shared.Data.Tutor;
using BerserkV3.Startup.Utils;
using RR.Core.DebugSystem;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.Common.TutorialSystem
{

	public class RemoteTutorialProgressRepository : DefaultTutorialProgressRepository
	{
		public override async Task InitAsync(IEnumerable<string> ids)
		{
			try
			{
				await TaskUtil.RetryAsync(TryGetProgressAsync);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
			finally
			{
				await FillAsync(ids);
			}
		}

		public override async Task SaveAsync()
		{
			try
			{
				await TaskUtil.RetryAsync(TrySaveAsync);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public override void ResetAll()
		{
			Progress.Clear();
		}

		private async Task<TryResult> TryGetProgressAsync()
		{
			var result = await TutorialAPI.GetProgress();
			if (!result.IsSuccess)
				return TryResult.Retry;

			if (result.Data?.Ids is {Count: > 0})
				Progress = result.Data.Ids.ToDictionary(key => key, _ => true);

			return TryResult.Success;
		}

		private async Task<TryResult> TrySaveAsync()
		{
			var model = new TutorProgressModel {Ids = Progress.Where(x => x.Value).Select(x => x.Key).ToList()};
			if (!await TutorialAPI.PostProgress(model))
				return TryResult.Retry;

			return TryResult.Success;
		}
	}

}