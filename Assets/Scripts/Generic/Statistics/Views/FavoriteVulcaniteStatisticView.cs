using System;
using Berserk.Shared.Data.Customisation;
using BerserkV3.Common.DataBase;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UI;
using UnityEngine;

namespace Statistics
{
	public class FavoriteVulcaniteStatisticView : SingleStatisticView
	{
		public FavoriteVulcaniteStatisticView(StatisticModel model,
		                                      IStatisticViewFactory factory,
		                                      Transform parentView = null) : base(model, factory, parentView)
		{
		}

		protected override void OnUpdate(BaseView view, IStatisticParam param)
		{
			if (view == null)
				throw new NullReferenceException($"View is missing, Input Param : {param}");

			if (!(view is VulcaniteStatisticView statView))
				throw new InvalidOperationException($"Unkown view type : {view.GetType().Name}, Input Param : {param}");

			statView.SetActiveImage(true);
			statView.SetActiveImageMask(true);

			statView.SetActiveTextValue(true);
			statView.SetTextValue("Favorite<br>Vulcanite");

			statView.SetActiveBorderImage(true);
			statView.SetActiveTextBackground(false);
			statView.SetActiveBackground(false);
			statView.SetActiveTextKey(false);
			statView.SetImageAspectRatio();
			statView.ReverseArrangement(true);

			var borderTask = UniTask.CompletedTask;
			var farameItem = CustomisationServiceAdapter.Repository.GetFirstEquipped(CustomisationType.AvatarFrame);
			if (farameItem != null)
				borderTask = statView.BorderRawImage.LoadResourceAsync(farameItem.AssetData.URL);

			UniTask.WhenAll(borderTask,
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
			if (!(View is VulcaniteStatisticView statView))
				return;
			
			statView.RawImage.ReleaseResource();
			statView.BorderRawImage.ReleaseResource();
		}
	}
}