using System.Collections.Generic;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.LogicContext;

namespace Berserk.Shared.Data.Abstraction
{
	public interface IGameDatabase
	{
		bool Initialized { get; }
		
		#region Initialization
		IGameDatabase LoadFrom(string data);
		void FillFrom(string data);
		void FillFrom(GameDatabaseModel data);
		GameDatabaseModel GetModel();
		#endregion
		
		#region Get Methods
		#region Links

		IEnumerable<LinkData> AllLinks();

		LinkData GetLink(string id);

		#endregion

		#region Cards

		IEnumerable<CardData> AllCards();

		CardData GetCard(string id);

		IEnumerable<CardData> GetCards(IEnumerable<string> ids);

		#endregion

		#region Heroes

		IEnumerable<HeroData> AllHeroes();

		IEnumerable<HeroData> GetHeroes(IEnumerable<string> ids);

		HeroData GetHero(string id);

		#endregion

		#region Effects

		IEnumerable<EffectData> AllEffects();

		EffectData GetEffectConfig(string id);

		IEnumerable<EffectData> GetEffects(IEnumerable<string> ids);

		#endregion

		#region Keywords

		IEnumerable<KeywordData> AllKeywords();

		KeywordData GetKeyword(string id);

		#endregion
		
		#region Customisations
		
		IEnumerable<CustomisationData> AllCustomisations();

		IEnumerable<CustomisationData> GetCustomisations(IEnumerable<string> ids);

		CustomisationData GetCustomisationData(string id);

		#endregion

		#region Localizations

		IEnumerable<LocalizationData> AllLocalizations();

		string GetLocalization(string id);

		#endregion
		
		#region TutorialCards
		IEnumerable<TutorialCardData> AllTutorialCards();
		IEnumerable<TutorialCardData> GetTutorialCards(IEnumerable<string> ids);
		TutorialCardData GetTutorialCard(string id);

		#endregion

		#region AIBehaviours

		IEnumerable<AiBehaviourData> AllAiBehaviours();

		AiNode GetAiBehaviourNode(PracticeMode practiceMode);

		#endregion
		#endregion
	}
}