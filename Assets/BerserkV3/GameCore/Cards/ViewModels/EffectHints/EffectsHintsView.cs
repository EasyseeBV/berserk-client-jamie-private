using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RR.Core.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.Cards.EffectHints
{
	public interface IEffectsHintsView
	{
		public GameObject InnateContainer { get; }
		public GameObject GainedContainer { get; }
		
		void Show();
		void Hide();
		void SetOnClickAction(Action value);
		UniTask FillAsync(IEnumerable<EffectHintModel> models,int maxElements, CancellationToken token = default);
		void Clear();
	}
	
	public class EffectsHintsView : MonoBehaviour, IEffectsHintsView
	{
		private const string MORE_THEN_LIMIT_IMAGE = "EffectIcon_More";
		
		[SerializeField] private CanvasGroup effectsCanvasGroup;
		[SerializeField] private float fadeDuration = 0.6f;
		[SerializeField] private GameObject innateEffects;
		[SerializeField] private GameObject gainedEffects;
		[SerializeField] private GameObject innateContainer;
		[SerializeField] private GameObject gainedContainer;
		[SerializeField] private List<EffectHintImageItemView> innateEffectViews = new();
		[SerializeField] private List<EffectHintImageItemView> gainedEffectViews = new();
		
		private int showKeywordsLimit;
		private Button innateEffectViewsButton;
		private Button gainedEffectViewsButton;
		private Tween fadeTween;
		
		public GameObject InnateContainer => innateContainer;
		public GameObject GainedContainer => gainedContainer;
		
		public void SetOnClickAction(Action value)
		{
			if (innateEffectViewsButton)
			{
				innateEffectViewsButton.onClick.RemoveAllListeners();
				innateEffectViewsButton.onClick.AddListener(() => value?.Invoke());
			}
			
			if (gainedEffectViewsButton)
			{
				gainedEffectViewsButton.onClick.RemoveAllListeners();
				gainedEffectViewsButton.onClick.AddListener(() => value?.Invoke());
			}
		}
		
		public async void Show()
		{
			innateEffects.gameObject.SetActive(true);
			gainedEffects.gameObject.SetActive(true);
			fadeTween?.Kill();
			fadeTween = effectsCanvasGroup.DOFade(1f, fadeDuration);
			await fadeTween.Play().AsyncWaitForCompletion();
		}
		
		public async void Hide()
		{
			fadeTween?.Kill();
			fadeTween = effectsCanvasGroup.DOFade(0f, fadeDuration);
			await fadeTween.Play().AsyncWaitForCompletion();
			innateEffects.gameObject.SetActive(false);
			gainedEffects.gameObject.SetActive(false);
		}
		
		public async UniTask FillAsync(IEnumerable<EffectHintModel> models,int maxElements, CancellationToken token = default)
		{
			SetButtonsInteractable(false);
			showKeywordsLimit = maxElements;
			
			innateEffectViews.Concat(gainedEffectViews).ForEach(x => x.SetActive(false));
			
			await UniTask.WhenAll(models.GroupBy(x => x.Origins).Select(x => InitAndDisplayAsync(x, x.Key, token)));
			
			SetButtonsInteractable(innateEffectViews.Count(x=> x.IsActive) == showKeywordsLimit - 1 
			                       || gainedEffectViews.Count(x=> x.IsActive) == showKeywordsLimit - 1);
		}
		
		private UniTask InitAndDisplayAsync(
			IEnumerable<EffectHintModel> models, 
			EffectOrigin origin,
			CancellationToken token)
		{
			var effectModels = models.ToList();
			var effectViews = origin == EffectOrigin.Gained ? gainedEffectViews : innateEffectViews;
			if (effectViews.Count < showKeywordsLimit)
				throw new ApplicationException($"{nameof(effectViews)} not enough views to draw.");
			
			return UniTask.WhenAll(effectModels.Take(showKeywordsLimit).Select(SetupAsync));
			UniTask SetupAsync(EffectHintModel model, int index)
			{
				var view = effectViews[index];
				if (effectModels.Count <= showKeywordsLimit) 
					return view.ShowAsync(model.IconUrl, token);
				
				var iconUrl = index == showKeywordsLimit - 1
					? MORE_THEN_LIMIT_IMAGE
					: model.IconUrl;
				return view.ShowAsync(iconUrl, token);
			}
		}
		
		private void SetButtonsInteractable(bool value)
		{
			if (innateEffectViewsButton)
				innateEffectViewsButton.interactable = value;
			
			if (gainedEffectViewsButton)
				gainedEffectViewsButton.interactable = value;
		}
		
		public void Clear()
		{
			if (innateEffectViewsButton)
				innateEffectViewsButton.onClick.RemoveAllListeners();
			
			if (gainedEffectViewsButton)
				gainedEffectViewsButton.onClick.RemoveAllListeners();
			
			fadeTween?.Kill();
			fadeTween = null;
		}
	}
}