using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Startup.Events;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.Common.Utils
{
	/// <summary>
	/// After remake lobby architecture this can be removed
	/// </summary>
	public struct CardDataAdapter
	{
		public string Id { get; set; }
		public int Attack { get; set; }
		public int Armor { get; set; }
		public int Hp { get; set; }
		public int BaseLava { get; set; } 
		public int Lava { get; set; }       
		public bool HasFactionPenalty { get; set; }
		public bool IsToken { get; set; }
		public List<string> EffectsIds { get; set; }
		public List<InvalidAction> InvalidActions { get; set; }
		public ObjectType Type { get; set; }
		public Season Season { get; set; }
		public int LimitInDeck { get; set; }
		public int AddAtRegistration { get; set; }

		// Info
		public string Title { get; set; }
		public string ArtUrl { get; set; }
		public string Artist { get; set; }
		public string Description { get; set; }
		public string Tooltip { get; set; }

		public Rarity Rarity { get; set; }
		public Race[] Races { get; set; }
		public Faction[] Factions { get; set; }
		public SubType[] SubTypes { get; set; }
		public ArtType ArtType { get; set; }
		public Quadrant Quadrant { get; set; }

		public CardDataAdapter(ICardData data)
		{
			Id = data.Id;
			Attack = data.Attack;
			Armor = data.Armor;
			Hp = data.Hp;
			BaseLava = data.Mana;
			Lava = data.Mana;
			HasFactionPenalty = false;
			EffectsIds = data.EffectsIds.ToList();
			InvalidActions = data.InvalidActions.ToList();
			Type = data.Type;
			Season = data.Season;
			LimitInDeck = data.LimitInDeck;
			AddAtRegistration = data.AddAtRegistration;
			Title = data.Title;
			ArtUrl = data.ArtUrl;
			Artist = data.Artist;
			Description = data.Description;
			Tooltip = data.Tooltip;
			Rarity = data.Rarity;
			Races = data.Races;
			Factions = data.Factions;
			SubTypes = data.SubTypes;
			ArtType = data.ArtType;
			Quadrant = data.Quadrant;
			IsToken = data.SubTypes != null && data.SubTypes.Contains(SubType.Token);
		}

		public CardDataAdapter(IPreviewCardData data)
		{
			Id = data.Id;
			Attack = data.Attack;
			Armor = data.Armor;
			Hp = data.Hp;
			BaseLava = data.Mana;
			Lava = data.Mana;
			HasFactionPenalty = false;
			EffectsIds = data.EffectsIds.ToList();
			InvalidActions = data.InvalidActions.ToList();
			Type = data.Type;
			Season = data.Season;
			LimitInDeck = data.LimitInDeck;
			AddAtRegistration = data.AddAtRegistration;
			Title = data.Title;
			ArtUrl = data.ArtUrl;
			Artist = data.Artist;
			Description = data.Description;
			Tooltip = data.Tooltip;
			Rarity = data.Rarity;
			Races = data.Races;
			Factions = data.Factions;
			SubTypes = data.SubTypes;
			ArtType = data.ArtType;
			Quadrant = data.Quadrant;
			IsToken = data.IsToken;
		}
	}

	public static class GameCoreExtension
	{
		public static CardDataAdapter ToCardDataAdapter(this ICardData data)
		{
			if (data == null)
				throw new NullReferenceException("Card data is missing!");

			return new CardDataAdapter(data);
		}

		public static CardDataAdapter ToCardDataAdapter(this IPreviewCardData data)
		{
			if (data == null)
				throw new NullReferenceException("Card data is missing!");

			return new CardDataAdapter(data);
		}

		public static T AddLoadingTask<T>(this T task) where T : Task
		{
			StartupBus.AddLoadingTask += task;
			return task;
		}

		public static UniTask AddLoadingTask(this UniTask task)
		{
			StartupBus.AddLoadingUnitask += task;
			return task;
		}

		public static UniTask<T> AddLoadingTask<T>(this UniTask<T> task)
		{
			StartupBus.AddLoadingUnitask += task;
			return task;
		}

		public static async UniTask ProgressTickAsync(this IProgress<float> progress, int milliseconds, int tickCount, CancellationToken token = default)
		{
			var delay = milliseconds / tickCount;
			while (Application.isPlaying && tickCount > 0 && !token.IsCancellationRequested)
			{
				await UniTask.Delay(delay);
				progress.Report((float) delay / milliseconds);
				tickCount--;
			}
			
			if (token.IsCancellationRequested)
				return;
			
			progress.Report(1f);
		}

		public static async UniTask ProgressTickAsync(this IProgress<float> progress, AsyncOperation operation, Action onDone = null, CancellationToken token = default)
		{
			while (Application.isPlaying && !operation.isDone && !token.IsCancellationRequested)
			{
				await UniTask.Yield(PlayerLoopTiming.PostLateUpdate);
				progress.Report(operation.progress);
			}

			if (token.IsCancellationRequested)
				return;

			progress.Report(1f);
			onDone?.Invoke();
		}
		
		public static CardDataAdapter ApplyDeckFactionCost(
			this CardDataAdapter adapter,
			Faction deckFaction,
			int maxLava = 10)
		{
			if (adapter.Factions != null && adapter.Factions.Contains(deckFaction))
			{
				adapter.Lava = adapter.BaseLava;
				adapter.HasFactionPenalty = false;
				return adapter;
			}

			var baseLava = adapter.BaseLava;
			
			if (baseLava <= 0)
			{
				adapter.Lava = baseLava;
				adapter.HasFactionPenalty = false;
				return adapter;
			}
			
			var increase = Mathf.CeilToInt(Mathf.Log(baseLava + 1, 2f));
			var adjusted = baseLava + increase;

			if (adjusted > maxLava)
				adjusted = maxLava;

			adapter.Lava = adjusted;
			adapter.HasFactionPenalty = adjusted > baseLava;

			return adapter;
		}
	}
}