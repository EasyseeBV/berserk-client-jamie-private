using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicContext;

namespace BerserkV3.Common.PreviewSystem
{

	public class PreviewCardData : IPreviewCardData
	{
		private IRuntimeCardData runtimeCardData;
		private ICardData cardData;
		private Race[] races;
		private Faction[] factions;

		public event Action OnUpdatePreview;
		public string Id { get; private set; }
		public int Hp { get; private set; }
		public int Armor { get; private set; }
		public int Mana { get; private set; }
		public int Attack { get; private set; }
		public bool IsToken { get; private set; }
		public ObjectType Type { get; private set; }
		public string Title { get; private set; }
		public string ArtUrl { get; private set; }
		public List<string> EffectsIds { get; private set; }
		public List<string> IresurrectableIds { get; private set; }
		public List<InvalidAction> InvalidActions { get; private set; }
		public Season Season { get; set; }
		public Rarity Rarity { get; set; }
		public SubType[] SubTypes { get; set; }
		public ArtType ArtType { get; set; }
		public Race[] Races { get; set; }
		public Faction[] Factions { get; set; }
		public Quadrant Quadrant { get; set; }
		public int LimitInDeck { get; set; }
		public int AddAtRegistration { get; set; }
		public string Artist { get; set; }
		public string Description { get; set; }
		public string Tooltip { get; set; }

		public PreviewCardData(ICardData cardData)
		{
			if (cardData == null)
			{
				DefaultSharedLogger.Error($"[{GetType().Name}] Data is missing");
				return;
			}
			
			this.cardData = cardData;
			RefreshStaticData();
			RefreshRuntimeData();
		}

		public PreviewCardData(IRuntimeCardData runtimeCardData, ICardData cardData)
		{
			if (runtimeCardData == null || cardData == null)
			{
				DefaultSharedLogger.Error($"[{GetType().Name}] Runtime data or Data is missing");
				return;
			}
			
			this.cardData = cardData;
			this.runtimeCardData = runtimeCardData;
			runtimeCardData.OnStateChanged += RuntimeCardDataOnOnStateChanged;
			runtimeCardData.Armor.OnChanged += OnStatChanged;
			runtimeCardData.Attack.OnChanged += OnStatChanged;
			runtimeCardData.Hp.OnChanged += OnStatChanged;
			runtimeCardData.Mana.OnChanged += OnStatChanged;
			runtimeCardData.MoveCount.OnChanged += OnStatChanged;
			RefreshStaticData();
			RefreshRuntimeData();
		}

		public void Dispose()
		{
			if (runtimeCardData != null)
			{
				runtimeCardData.OnStateChanged -= RuntimeCardDataOnOnStateChanged;
				runtimeCardData.Armor.OnChanged -= OnStatChanged;
				runtimeCardData.Attack.OnChanged -= OnStatChanged;
				runtimeCardData.Hp.OnChanged -= OnStatChanged;
				runtimeCardData.Mana.OnChanged -= OnStatChanged;
				runtimeCardData.MoveCount.OnChanged -= OnStatChanged;
			}
			
			cardData = null;
			runtimeCardData = null;
			OnUpdatePreview = null;
		}

		private void RefreshRuntimeData()
		{
			if (runtimeCardData == null)
				return;
			
			Hp = runtimeCardData.Hp;
			Mana = runtimeCardData.Mana;
			Attack = runtimeCardData.Attack;
			Armor = runtimeCardData.Armor;
			IsToken = runtimeCardData.IsToken;
			OnUpdatePreview?.Invoke();
		}

		private void RefreshStaticData()
		{
			Id = cardData.Id;
			Hp = cardData.Hp;
			Mana = cardData.Mana;
			Attack = cardData.Attack;
			Armor = cardData.Armor;
			Type = cardData.Type;
			Title = cardData.Title;
			ArtUrl = cardData.ArtUrl;
			EffectsIds = cardData.EffectsIds;
			IresurrectableIds = cardData.IresurrectableIds;
			InvalidActions = cardData.InvalidActions;
			Season = cardData.Season;
			Rarity = cardData.Rarity;
			Races = cardData.Races;
			Factions = cardData.Factions;
			SubTypes = cardData.SubTypes;
			ArtType = cardData.ArtType;
			Quadrant = cardData.Quadrant;
			LimitInDeck = cardData.LimitInDeck;
			AddAtRegistration = cardData.AddAtRegistration;
			Artist = cardData.Artist;
			Description = cardData.Description;
			Tooltip = cardData.Tooltip;
			IsToken = cardData.SubTypes != null && cardData.SubTypes.Contains(SubType.Token);
		}

		private void OnStatChanged(int obj)
		{
			RefreshRuntimeData();
		}

		private void RuntimeCardDataOnOnStateChanged(RuntimeState arg1, RuntimeState arg2)
		{
			RefreshRuntimeData();
		}
		public void SetPreviewMana(int value)
		{
			Mana = value;
			OnUpdatePreview?.Invoke();
		}
	}
}