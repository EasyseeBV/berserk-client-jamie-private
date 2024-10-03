using System;
using System.Linq;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.DataBase;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using ServerCore.Infrastructure.Models;
using UI;
using UnityEngine;
using Vulcan.Data;

namespace Statistics
{
	public class VulcanitesWinRateStatisticView : MultipleStatisticView
	{
		public VulcanitesWinRateStatisticView(StatisticModel model,
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

			if (!(view is VulcaniteStatisticView statView))
				throw new InvalidOperationException($"Unkown view type : {view.GetType().Name}, Input Param : {param}");

			statView.SetActiveImage(true);
			statView.SetActiveImageMask(true);
			statView.SetActiveTextBackground(true);

			statView.SetActiveTextValue(true);
			statView.SetTextValue(param.Get("Value") + "<size=65%>%");

			statView.SetActiveBorderImage(true);
			statView.SetActiveBackground(false);
			statView.SetActiveTextKey(false);

			var borderTask = UniTask.CompletedTask;
			var farameItem = CustomisationServiceAdapter.Repository.GetFirstEquipped(CustomisationType.AvatarFrame);
			if (farameItem != null)
				borderTask = statView.BorderRawImage.LoadResourceAsync(farameItem.AssetData.URL);

			UniTask.WhenAll(
				borderTask,
				statView.RawImageMask.LoadResourceAsync("Vulcanite_Mask"),
				statView.RawImage.LoadResourceAsync(GetArtUrl(param.Get("Id"))))
				.ContinueWith(() =>
				{
					statView.SetImageAspectRatio();
					statView.SetActive(true);
				})
				.Forget();
		}

		private string GetArtUrl(string id)
		{
			return GameDataBaseAdapter.Instance.GetHero(id)?.ArtUrl;
		}
		
		
		protected override void OnDispose()
		{
			base.OnDispose();
			Views?.Select(x => x as VulcaniteStatisticView)
				.Where(x => x != null)
				.ForEach(x =>
				{
					x.RawImage.ReleaseResource();
					x.BorderRawImage.ReleaseResource();
				});
		}
	}
}