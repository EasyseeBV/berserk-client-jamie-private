using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;

namespace BerserkV3.GameCore.Cards.EffectHints
{
	public enum EffectOrigin
	{
		Gained,
		Innate
	}
	
	public readonly struct EffectHintModel
	{
		public string IconUrl { get; }
		public string Title { get; }
		public string Description { get; }
		public EffectOrigin Origins { get; }
		
		public EffectHintModel(string iconUrl, EffectOrigin origins, string title, string description)
		{
			IconUrl = iconUrl;
			Origins = origins;
			Title = title;
			Description = description;
		}
	}
	
	public class EffectHintsApplication : IEffectHintsApplication
	{
		private const int SHOW_CARD_KEYWORDS_LIMIT = 3;
		private const int SHOW_HERO_KEYWORDS_LIMIT = 3;
		
		private readonly IGameDatabase gameDatabase;
		private readonly IGameLogicEventsSource gameLogicEventsSource;
		private readonly IEffectWindowView effectWindowView;
		
		private IRuntimeGameObject targetObject;
		private IEffectsHintsView targetView;
		private CancellationTokenSource source;
		private CancellationTokenSource drawSource;
		private IDisposable previewData;
		private bool isEffectsWindowUsed;
		private int maxElements;
		
		public EffectHintsApplication(
			IGameDatabase gameDatabase,
			IGameLogicEventsSource gameLogicEventsSource,
			IEffectWindowView effectWindowView)
		{
			this.gameDatabase = gameDatabase;
			this.gameLogicEventsSource = gameLogicEventsSource;
			this.effectWindowView = effectWindowView;
		}
		
		public void Dispose()
		{
			if (source == null)
				return;
			
			if (isEffectsWindowUsed)
				OnHintsWindowClosed();
			
			previewData?.Dispose();
			previewData = null;
			isEffectsWindowUsed = false;
			targetView?.Clear();
			source?.Cancel();
			source?.Dispose();
			source = null;
			targetObject = null;
			targetView = null;
		}
		
		public void Setup(IRuntimeGameObject runtimeGameObject, IRuntimeObjectView view)
		{
			if (source != null)
				return;
			if(runtimeGameObject.RuntimeData.Type == ObjectType.Spell)
				return;
			
			targetObject = runtimeGameObject;
			switch (view)
			{
				case ICardView cardView:
					targetView = cardView.Layout.SelfContainer.GetComponentInChildren<IEffectsHintsView>();
					maxElements = SHOW_CARD_KEYWORDS_LIMIT;
					break;
				case IHeroView heroView:
					targetView = heroView.Layout.SelfContainer.GetComponentInChildren<IEffectsHintsView>();
					maxElements = SHOW_HERO_KEYWORDS_LIMIT;
					break;
				default:
					throw new NotImplementedException();
			}
			
			source = new CancellationTokenSource();
			var token = source.Token;
			gameLogicEventsSource.Subscribe<AddObjectEffect>(AddEffect, token);
			gameLogicEventsSource.Subscribe<ChangeObjectEffect>(ChangeEffect, token);
			gameLogicEventsSource.Subscribe<DeleteObjectEffect>(DeleteEffect, token);
			gameLogicEventsSource.Subscribe<DeleteImposingEffects>(DeleteImposingEffect, token);

			if (runtimeGameObject.RuntimeData.Type != ObjectType.Hero)
				targetView.SetOnClickAction(OnHintsWindowOpenAsync);
			
			Draw(token);
			Show();
		}
		
		public void Show()
		{
			if (source == null)
				return;
			
			targetView?.Show();
		}
		
		public void Hide()
		{
			if (source == null)
				return;
			
			targetView?.Hide();
		}
		
		private IEnumerable<EffectHintModel> GetHintModels()
		{
			var innateHints = gameDatabase.GetEffects(targetObject.RuntimeData.GetInnateEffectsIds())
				.Where(x => !string.IsNullOrEmpty(x.IconUrl))
				.Select(effectData =>
				{
					var keyword = gameDatabase.GetKeyword(effectData.KeywordId);
					return new EffectHintModel(effectData.IconUrl,
						EffectOrigin.Innate,
						keyword?.Title,
						FormatDescription(effectData.Value, effectData.Length, keyword?.Description));
				});

			var gainedHints = gameDatabase
				.GetEffects(targetObject.RuntimeData.GetGainedEffects().Select(x => x.ConfigId))
				.Where(x => !string.IsNullOrEmpty(x.IconUrl))
				.Select(effectData =>
				{
					var keyword = gameDatabase.GetKeyword(effectData.KeywordId);
					return new EffectHintModel(effectData.IconUrl,
						EffectOrigin.Gained,
						keyword?.Title,
						FormatDescription(effectData.Value, effectData.Length, keyword?.Description));
				});

			return gainedHints.Concat(innateHints).ToArray();
		}
		
		private static string FormatDescription(int value, int lenght, string description)
		{
			if (string.IsNullOrEmpty(description))
				return string.Empty;

			return description
				.Replace("{value}", $"{value}")
				.Replace("{valuePercent}", $"{value}%")
				.Replace("{length}", $"{(lenght < 0 ? "<size=130%>∞</size>" : lenght)}");
		}
		
		private void Draw(CancellationToken token)
		{
			if (targetView == null || token.IsCancellationRequested)
				return;
			
			drawSource?.Cancel();
			drawSource?.Dispose();
			drawSource = CancellationTokenSource.CreateLinkedTokenSource(token);
			targetView.FillAsync(GetHintModels(),maxElements, drawSource.Token).Forget();
		}
		
		private void ChangeEffect(ChangeObjectEffect data)
		{
			if (data.RuntimeData.ExecutorId != targetObject.RuntimeData.Id)
				return;
			
			Draw(source.Token);
		}
		
		private void DeleteImposingEffect(DeleteImposingEffects data)
		{
			if (data.RuntimeObjectId != targetObject.RuntimeData.Id)
				return;
			
			Draw(source.Token);
		}
		
		private void DeleteEffect(DeleteObjectEffect data)
		{
			if (data.RuntimeData.ExecutorId != targetObject.RuntimeData.Id)
				return;
			
			Draw(source.Token);
		}
		
		private void AddEffect(AddObjectEffect data)
		{
			if (data.RuntimeData.ExecutorId != targetObject.RuntimeData.Id)
				return;
			
			Draw(source.Token);
		}
		
		private async void OnHintsWindowOpenAsync()
		{
			if (source == null || isEffectsWindowUsed)
				return;
			
			isEffectsWindowUsed = true;
			previewData?.Dispose();
			previewData = null;
			
			await effectWindowView.InitAsync(GetHintModels(), source.Token);
			effectWindowView.SetOnCloseAction(OnHintsWindowClosed);
			effectWindowView.Show();
			
			var previewCardData = targetObject.ToPreviewData();
			if (previewCardData == null)
				return;
			
			previewData = previewCardData;
			previewCardData.OnUpdatePreview += () =>
				effectWindowView.UpdatePreviewAsync(previewCardData.ToCardDataAdapter()).Forget();
			effectWindowView.UpdatePreviewAsync(previewCardData.ToCardDataAdapter()).Forget();
		}
		
		private void OnHintsWindowClosed()
		{
			if (!isEffectsWindowUsed)
				return;
			
			previewData?.Dispose();
			previewData = null;
			effectWindowView?.Close();
			isEffectsWindowUsed = false;
		}
	}
}
