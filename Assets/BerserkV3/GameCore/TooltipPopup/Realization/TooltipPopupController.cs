using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.GameCore.Cards.EffectHints;
using BerserkV3.GameCore.TooltipPopup.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.TooltipPopup
{
	public class TooltipPopupController : ITooltipPopupController
	{
		private readonly IGameDatabase gameDatabase;
		private readonly ITooltipPopupView popup; 
		
		public TooltipPopupController(
			IGameDatabase gameDatabase,
			ITooltipPopupView popup)
		{
			this.gameDatabase = gameDatabase;
			this.popup = popup;
			
			popup.Close();
		}
		
		public async UniTask DisplayAsync(IRuntimeData runtimeData, EffectOrigin origin, Transform container, CancellationToken ctn)
		{
			try
			{
				var effects = CollectData(runtimeData, origin).ToList();

				if (effects.Count <= 0)
					return;


				await popup.InitAsync(effects, container, origin, ctn);
				await UniTask.Delay(500, delayTiming: PlayerLoopTiming.Update, cancellationToken: ctn);
				popup.Show();
			}
			catch
			{
				popup.Close();
			}
		}

		public void Close()
		{
			popup.Close();
		}
		
		private IEnumerable<TooltipEffectData> CollectData(IRuntimeData runtimeData, EffectOrigin origin)
		{
			try
			{
				if (runtimeData == null)
					return Array.Empty<TooltipEffectData>();

				if (origin == EffectOrigin.Gained)
				{
					return runtimeData.GetGainedEffects()
						.Select(MapTooltipData)
						.Where(x => x != null);
				}
				
				return gameDatabase
					.GetEffects(runtimeData.GetInnateEffectsIds())
					.Select(MapTooltipData)
					.Where(x => x != null);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return Array.Empty<TooltipEffectData>();
			}
		}
		
		private TooltipEffectData MapTooltipData(EffectData data)
		{
			var keyword = gameDatabase.GetKeyword(data?.KeywordId);
			if (!ShouldSpawnTooltip(keyword) || data == null)
				return null;

			return new TooltipEffectData
			{
				Value = data.Value,
				Length = data.Length,
				Title = keyword.Title,
				Description = FormatDescription(data.Value, data.Length, keyword.Description),
				Keyword = data.Keyword,
				EffectConfigId = data.Id,
				IconUrl = data.IconUrl
			};
		}

		private TooltipEffectData MapTooltipData(IRuntimeEffectData runtimeData)
		{
			if (runtimeData == null || runtimeData.Disabled)
				return null;
			
			var effectData = gameDatabase.GetEffectConfig(runtimeData.ConfigId);
			var keyword = gameDatabase.GetKeyword(effectData?.KeywordId);
			if (!ShouldSpawnTooltip(keyword) || effectData == null)
				return null;

			return new TooltipEffectData
			{
				Value = runtimeData.CurrentValue,
				Length = runtimeData.CurrentLength,
				Title = keyword.Title,
				Description = FormatDescription(runtimeData.CurrentValue, runtimeData.CurrentLength, keyword.Description),
				Keyword = effectData.Keyword,
				EffectConfigId = effectData.Id,
				IconUrl = effectData.IconUrl
			};
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

		private static bool ShouldSpawnTooltip(KeywordData data)
		{
			return data != null && !string.IsNullOrEmpty(data.Title) && data.Id != "None";
		}
	}
}