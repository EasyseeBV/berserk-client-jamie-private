using System;
using System.Linq;
using BerserkV3.Lobby.MatchMaking.Leagues;
using Cysharp.Threading.Tasks;
using Lobby;
using RR.UI.FrameSystem;
using UnityEngine;
using UI;
using RR.Core.ResourceManagament;
using Sirenix.Utilities;

namespace Statistics
{
	public class EloStatisticView : MultipleStatisticView
	{
		public EloStatisticView(StatisticModel model,
								IStatisticViewFactory viewFactory,
								Transform parent = null,
								int displayCount = 0)
			: base(model, viewFactory, parent, displayCount)
		{
		}

		protected override void OnInitialize(BaseView view, IStatisticParam param)
		{
			if (view == null)
				throw new NullReferenceException($"View is missing, Input Param : {param}");

			if (!(view is BaseStatisticView statView))
				throw new InvalidOperationException($"Unkown view type : {view.GetType().Name}, Input Param : {param}");

			statView.SetActiveImage(true);
			statView.SetActiveImageMask(true);

			statView.SetActiveTextValue(true);
			statView.SetTextValue("MMR : " + param.Get("Value"));

			statView.SetActiveBackground(false);
			statView.SetActiveTextBackground(false);
			statView.SetActiveTextKey(false);
			statView.RawImage
				.LoadResourceAsync(GetArtUrl(param.Get("Id")))
				.ContinueWith(() =>
				{
					statView.SetImageAspectRatio();
					statView.SetActive(true);
				})
				.Forget();
		}

		private string GetArtUrl(string paramKey)
		{
			if (!int.TryParse(paramKey, out var index)
				|| index < 0
				|| index >= LobbyBus.Leagues.Value.Count)
			{
				var model = LobbyBus.Leagues.Value.FirstOrDefault(x => x.Id == paramKey)
							?? throw new InvalidOperationException("League key is missing");
				return model.GetArtURL();
			}

			return LobbyBus.Leagues.Value[index].GetArtURL();
		}

		protected override void OnDispose()
		{
			Views?
				.Select(x => x as BaseStatisticView)
				.Where(x => x != null)
				.ForEach(x => x.RawImage.ReleaseResource());
			base.OnDispose();
		}
	}
}