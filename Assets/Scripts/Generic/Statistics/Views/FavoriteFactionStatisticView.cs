using System;
using Cysharp.Threading.Tasks;
using Game;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UI;
using UnityEngine;

namespace Statistics
{
	public class FavoriteFactionStatisticView : SingleStatisticView
	{
		public FavoriteFactionStatisticView(StatisticModel model,
											 IStatisticViewFactory factory,
											 Transform parentView = null) : base(model, factory, parentView)
		{
		}

		protected override void OnUpdate(BaseView view, IStatisticParam param)
		{
			if (view == null)
				throw new NullReferenceException($"View is missing, Input Param : {param}");

			if (!(view is VulcaniteStatisticView statView))
				throw
					new InvalidOperationException($"Unkown view type : {view.GetType().Name}, Input Param : {param}");

			statView.SetActiveImage(true);
			statView.SetActiveImageMask(false);

			statView.SetActiveTextValue(true);
			statView.SetTextValue("Favorite<br>Faction");

			statView.SetActiveBorderImage(false);
			statView.SetActiveBackground(false);
			statView.SetActiveTextBackground(false);
			statView.SetActiveTextKey(false);
			statView.ReverseArrangement(true);
			
			statView.RawImage
				.LoadResourceAsync(GetArtUrl(param.Get("Id")))
				.ContinueWith(() =>
				{
					statView.SetImageAspectRatio();
					statView.SetActive(true);
				})
				.Forget();
		}

		private string GetArtUrl(string id)
		{
			return $"{id}_Flag";
		}

		protected override void OnDispose()
		{
			base.OnDispose();
			if (!(View is BaseStatisticView statView))
				return;
			statView.RawImage.texture.DestroyImmediateSafe();
		}
	}
}