using System;
using System.Collections.Generic;
using System.Threading;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.Cards.EffectHints
{
	public interface IEffectWindowView
	{
		void Show();
		void Close();
		UniTask InitAsync(IEnumerable<EffectHintModel> models, CancellationToken token = default);
		UniTask UpdatePreviewAsync(CardDataAdapter? cardData, CancellationToken token = default);
		void SetOnCloseAction(Action value);
	}

	public partial class EffectWindowView : BaseView, IEffectWindowView
	{
		[SerializeField] protected GameObject Prototype;
		private string EffectIconUrl => "EffectIcon_{0}";
		
		protected override void OnClosed()
		{
			base.OnClosed();
			GainedEffectContainer.DestroyChildren();
			InnateEffectContainer.DestroyChildren();
			ExitButton.onClick.RemoveAllListeners();
		}

		public UniTask InitAsync(
			IEnumerable<EffectHintModel> models, 
			CancellationToken token = default)
		{
			
			CardHandArtView.SetActive(false);
			return UniTask
				.WhenAll(models.Select(model => CreateViewAsync(model, token)))
				.AttachExternalCancellation(token);
		}

		public void SetOnCloseAction(Action value)
		{
			ExitButton.onClick.AddListener(() => value?.Invoke());
		}
		
		public async UniTask UpdatePreviewAsync(CardDataAdapter? cardData, CancellationToken token = default)
		{
			if (cardData.HasValue)
				await CardHandArtView.SetupAsync(cardData.Value, token);
			
			CardHandArtView.SetActive(cardData.HasValue);
		}
		
		private UniTask CreateViewAsync(EffectHintModel model, CancellationToken token)
		{
			var parentContainer = model.Origins == EffectOrigin.Gained ? GainedEffectContainer : InnateEffectContainer;
			var view = Instantiate(Prototype, parentContainer).GetComponent<EffectHintItemView>();
			return view.SetupAsync(string.Format(EffectIconUrl,model.IconUrl),model.Title,model.Description,token);
		}
	}
}