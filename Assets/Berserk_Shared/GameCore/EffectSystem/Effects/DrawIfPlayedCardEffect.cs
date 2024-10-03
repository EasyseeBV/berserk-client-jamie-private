using System;
using System.Linq;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.EffectSystem.Effects
{
	[EffectKeyword(EffectKeyword.DrawIfPlayedCard)]
	public class DrawIfPlayedCardEffect : DrawEffect
	{
		private struct EffectMetaData
		{
			public string CardIds;
			public string SubTypes;
			public string Factions;
			public string Races;
			public Owner CheckOwner;
			public int? LastTurns;
			public int? LastRounds;
			public bool ExcludeExecutor;
		}
		
		public override bool CanExecute()
		{
			try
			{
				var metaData = JsonConvert.DeserializeObject<EffectMetaData>(EffectData.Meta);
				if (metaData.CheckOwner == Owner.None)
					throw new InvalidOperationException($"[{EffectData.Id}] Cannot use {nameof(metaData.CheckOwner)} : {metaData.CheckOwner}");
			
				var checkTarget = metaData.CheckOwner == Owner.Self
					? GameContext.PlayerRepository.Get(Executor.RuntimeData.OwnerUserId)
					: GameContext.PlayerRepository.GetOpposite(Executor.RuntimeData.OwnerUserId);
			
				return HasCardsByFilter(checkTarget, metaData) && base.CanExecute();
			}
			catch (Exception)
			{
				DefaultSharedLogger.Error($"Cannot deserialize meta data : {nameof(EffectMetaData)}, from : {EffectData.Meta}");
				throw;
			}
		}
		
		private bool HasCardsByFilter(IRuntimePlayer runtimePlayer, EffectMetaData metaData)
		{
			var disableCheckFaction = string.IsNullOrEmpty(metaData.Factions);
			var disableCheckSubType = string.IsNullOrEmpty(metaData.SubTypes);
			var disableCheckRace = string.IsNullOrEmpty(metaData.Races);
			var disableCheckByids = string.IsNullOrEmpty(metaData.CardIds);
			var disableChekbyRound = !metaData.LastRounds.HasValue;
			var disableChekbyTurn = !metaData.LastTurns.HasValue;

			if (disableCheckByids && disableChekbyRound && disableChekbyTurn && disableCheckFaction && disableCheckRace && disableCheckSubType)
				throw new InvalidOperationException("All filers disabled, cannot find any played cards");
				
			var currRound = GameContext.Timer.RuntimeData.Round;
			var currTurn = GameContext.Timer.RuntimeData.Turn;
				
			var cardIds = disableCheckByids
				? Array.Empty<string>()
				: metaData.CardIds.Split(",", StringSplitOptions.RemoveEmptyEntries);

			return runtimePlayer.RuntimeData.PlayedCards
				.Any(p => (disableChekbyRound || (currRound - p.Round <= metaData.LastRounds))
				          && (disableChekbyTurn || (currTurn - p.Turn <= metaData.LastTurns))
				          && (disableCheckByids || cardIds.Contains(p.CardId))
				          && (disableCheckSubType || CheckBy(p.RuntimeId, t => t.Data.SubTypes, () => metaData.SubTypes, metaData.ExcludeExecutor))
				          && (disableCheckFaction || CheckBy(p.RuntimeId, t => t.Data.Factions, () => metaData.Factions, metaData.ExcludeExecutor))
				          && (disableCheckRace || CheckBy(p.RuntimeId, t => t.Data.Races, () => metaData.Races, metaData.ExcludeExecutor)));
		}
		
		private bool CheckBy<T>(int runtimeId, Func<IRuntimeGameCard,T[]> checkTarget, Func<string> enumsInString, bool excludeExecutor) where T : struct
		{
			if (!GameContext.GameRuntimePool.TryGet(runtimeId, out var target) 
			    || target is not IRuntimeGameCard gameCard
			    || (excludeExecutor && gameCard.RuntimeData.Id == Executor.RuntimeData.Id))
				return false;
			
			var metaDatas = enumsInString().ToEnums<T>();
			return checkTarget(gameCard).Any(checkStruct => metaDatas.Contains(checkStruct));
		}
	}
}