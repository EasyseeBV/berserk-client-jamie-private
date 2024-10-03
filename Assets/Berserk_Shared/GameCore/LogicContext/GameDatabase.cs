using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.Data.Serialization;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicContext
{
	public class GameDatabaseModel
	{
		public LinkData[] Links { get; set; }
		public CardData[] Cards { get; set; }
		public HeroData[] Heroes { get; set; }
		public EffectData[] Effects { get; set; }
		public KeywordData[] Keywords { get; set; }
		public CustomisationData[] Customisations { get; set; }
		public LocalizationData[] Localizations { get; set; }
		public TutorialCardData[] TutorialCards { get; set; }
		public AiBehaviourData[] AiBehaviours { get; set; }
	}

	public class GameDatabase : IGameDatabase
	{
		protected Dictionary<string, LinkData> Links = new();
		protected Dictionary<string, CardData> Cards = new();
		protected Dictionary<string, HeroData> Heroes = new();
		protected Dictionary<string, EffectData> Effects = new();
		protected Dictionary<string, KeywordData> Keywords = new();
		protected Dictionary<string, CustomisationData> Customisations = new();
		protected Dictionary<string, LocalizationData> Localizations = new();
		protected Dictionary<string, TutorialCardData> TutorialCards = new();
		protected Dictionary<string, AiBehaviourData> AiBehaviours = new();
		
		[JsonIgnore] public bool Initialized { get; protected set; }

		#region Initialization
		public virtual IGameDatabase LoadFrom(string data)
		{
			if (string.IsNullOrEmpty(data))
				throw new NullReferenceException($"Provide a valid json of type {nameof(GameDatabaseModel)}");
					
			var model = JsonConvert.DeserializeObject<GameDatabaseModel>(data, SharedSerializationHelper.DeserializeSettings);
			if (model == null)
				throw new NullReferenceException($"Provide a valid json of type {nameof(GameDatabaseModel)}");

			var instance = new GameDatabase();
			instance.FillFrom(model);
			return instance;
		}

		public virtual void FillFrom(GameDatabaseModel data)
		{
			try
			{
				Links = data.Links?.ToDictionary(v => v.Id) ?? new Dictionary<string, LinkData>();
				Cards = data.Cards?.ToDictionary(v => v.Id) ?? new Dictionary<string, CardData>();
				Heroes = data.Heroes?.ToDictionary(v => v.Id) ?? new Dictionary<string, HeroData>();
				Effects = data.Effects?.ToDictionary(v => v.Id) ?? new Dictionary<string, EffectData>();
				Keywords = data.Keywords?.ToDictionary(v => v.Id) ?? new Dictionary<string, KeywordData>();
				Customisations = data.Customisations?.ToDictionary(v => v.Id) ?? new Dictionary<string, CustomisationData>();
				Localizations = data.Localizations?.ToDictionary(v => v.Id) ?? new Dictionary<string, LocalizationData>();
				TutorialCards = data.TutorialCards?.ToDictionary(v => v.Id) ?? new Dictionary<string, TutorialCardData>();
				AiBehaviours = data.AiBehaviours?.ToDictionary(v => v.Id) ?? new Dictionary<string, AiBehaviourData>();
				Initialized = true;
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				throw;
			}
		}

		public GameDatabaseModel GetModel()
		{
			return new GameDatabaseModel
			{
				Links = Links?.Values.ToArray(),
				Cards = Cards?.Values.ToArray(),
				Heroes = Heroes?.Values.ToArray(),
				Effects = Effects?.Values.ToArray(),
				Keywords = Keywords?.Values.ToArray(),
				Customisations = Customisations?.Values.ToArray(),
				Localizations = Localizations?.Values.ToArray(),
				TutorialCards = TutorialCards?.Values.ToArray(),
				AiBehaviours = AiBehaviours?.Values.ToArray()
			};
		}

		public void FillFrom(string json)
		{
			try
			{
				var model = JsonConvert.DeserializeObject<GameDatabaseModel>(json, SharedSerializationHelper.DeserializeSettings);

				if (model == null)
					throw new NullReferenceException($"Provide a valid json of type {nameof(GameDatabaseModel)}");

				FillFrom(model);
			}
			catch (Exception e)
			{
				if (e is JsonSerializationException)
				{
					DefaultSharedLogger.Error($"{e.Message}\n{e.InnerException}\n{e.StackTrace}");
					throw;
				}

				DefaultSharedLogger.Error(e);
				throw;
			}
		}

		public override string ToString()
		{
			return JsonConvert.SerializeObject(GetModel(), SharedSerializationHelper.SerializeSettings);
		}

		#endregion
		
		#region Get Methods

		#region Links
		
		public IEnumerable<LinkData> AllLinks()
		{
			return Links.Values.ToArray();
		}
		
		public LinkData GetLink(string id)
		{
			return Links.TryGetValue(id, out var value) ? value : default;
		}
		
		#endregion

		#region Cards
		public IEnumerable<CardData> AllCards()
		{
			if (Cards == null) return new List<CardData>();
			return Cards.Values;
		}
		
		public CardData GetCard(string id)
		{
			if (Cards.TryGetValue(id, out var card))
				return card;

			return null;
		}

		public IEnumerable<CardData> GetCards(IEnumerable<string> ids)
		{
			return GetByIds(ids, Cards);
		}
		#endregion

		#region Heroes
		public IEnumerable<HeroData> AllHeroes()
		{
			if (Heroes == null) return new List<HeroData>();
			return Heroes.Values;
		}
		
		public IEnumerable<HeroData> GetHeroes(IEnumerable<string> ids)
		{
			return GetByIds(ids, Heroes);
		}
		
		public HeroData GetHero(string id)
		{
			return !string.IsNullOrEmpty(id) 
			       && Heroes.TryGetValue(id, out var hero) ? hero : null;
		}
		#endregion

		#region Effects
		public IEnumerable<EffectData> AllEffects()
		{
			if (Effects == null) return new List<EffectData>();
			return Effects.Values;
		}
		
		public EffectData GetEffectConfig(string id)
		{
			if (Effects.TryGetValue(id, out var result))
				return result;

			throw new Exception($"Can't find effect config {id}");
		}

		public IEnumerable<EffectData> GetEffects(IEnumerable<string> ids)
		{
			return GetByIds(ids, Effects);
		}
		#endregion

		#region Keywords
		public IEnumerable<KeywordData> AllKeywords()
		{
			if (Keywords == null) return new List<KeywordData>();
			return Keywords.Values;
		}
		
		public KeywordData GetKeyword(string id)
		{
			if (Keywords.TryGetValue(id, out var value))
				return value;

			return null;
		}
		#endregion

		#region Customisations

		public IEnumerable<CustomisationData> AllCustomisations()
		{
			if (Customisations == null) return new List<CustomisationData>();
			return Customisations.Values.ToArray();
		}		

		public IEnumerable<CustomisationData> GetCustomisations(IEnumerable<string> ids)
		{
			return GetByIds(ids, Customisations);
		}
		
		public CustomisationData GetCustomisationData(string id)
		{
			if (string.IsNullOrEmpty(id))
				return default;
			
			return Customisations.TryGetValue(id, out var data) ? data : default;
		}

		#endregion
		
		#region Localizations
		public IEnumerable<LocalizationData> AllLocalizations()
		{
			if (Localizations == null) return new List<LocalizationData>();
			return Localizations.Values.ToArray();
		}

		public string GetLocalization(string id)
		{
			return Localizations.TryGetValue(id, out var localizationData) ? localizationData.Text : $"Localization doesn't exist for : {id}";
		}
		#endregion
		
		#region TutorialCards
		public IEnumerable<TutorialCardData> AllTutorialCards()
		{
			if (TutorialCards == null) return new List<TutorialCardData>();
			return TutorialCards.Values.ToArray();
		}
		
		public IEnumerable<TutorialCardData> GetTutorialCards(IEnumerable<string> ids)
		{
			return GetByIds(ids, TutorialCards);
		}
		
		public TutorialCardData GetTutorialCard(string id) 
		{
			if (string.IsNullOrEmpty(id))
				return default;
			
			return TutorialCards.TryGetValue(id, out var data) ? data : default;
		}

		#endregion

		#region AIBehaviours
		public IEnumerable<AiBehaviourData> AllAiBehaviours()
		{
			if (AiBehaviours == null) return new List<AiBehaviourData>();
			return AiBehaviours.Values.ToArray();
		}
		
		public AiNode GetAiBehaviourNode(PracticeMode practiceMode)
		{
			if (!AiBehaviours.TryGetValue(practiceMode.ToString(), out var result) || result == null)
				throw new NullReferenceException($"{nameof(AiBehaviours)} - {practiceMode} not found");

			return result.Node;
		}
		#endregion
		
		protected static IEnumerable<TData> GetByIds<TData>(IEnumerable<string> ids, IReadOnlyDictionary<string, TData> dic) where TData : class
		{
			if (ids == null)
				return Array.Empty<TData>();
			
			var idsArray = ids.ToArray();
			var result = new TData[idsArray.Length];

			for (var i = 0; i < result.Length; ++i)
			{
				var id = idsArray[i];
				try
				{
					result[i] = dic[id];
				}
				catch
				{
					result[i] = null;
				}
			}

			return result;
		}
		#endregion
	}
}