

using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Game.Entities;
using ServerCore.Infrastructure.Models;

namespace Vulcan.Data
{
	// todo REMOVE this LEGACY
	public static class ContextExtension
	{
		public static InteractiveCardModel ToInteractiveCardModel(this IInteractiveEntity entity)
		{
			return default;
		}

		public static InteractiveCardModel ToInteractiveCardModel(this DataBase dataBase)
		{
			return default;
		}

		public static EntityStateModel ToEntityStateModel(this IInteractiveEntity entity)
		{
			return default;
		}

		public static EntityStateModel ToEntityStateModel(this DataBase dataBase, bool isCanAttack = false)
		{
			return default;
		}

		public static CardData ToCardData(this InteractiveCardModel model, Owner owner = Owner.None)
		{
			// var currentOwner = owner != Owner.None
			// 	? owner
			// 	: ActorsContextResolver.GetOwnerByName(model.EntityState.OwnerUserName);
			//
			// var isCardHidden = string.IsNullOrEmpty(model.CardId) || model.EntityState == null;
			// var cardData = isCardHidden
			// 	? new CardData()
			// 	: ContentRepository.GetCopyOfCard(model.CardId);
			//
			// var context = ActorsContextResolver.GetPlayer(owner);
			// var cardBackItem = context?.UICustomizations?.FirstOrDefault(x => x.Type == CustomisationType.CardBack);
			// var cardBackUrl = cardBackItem.GetCustomisationAsset<AssetData>()?.URL;
			//
			// cardData.Owner = currentOwner;
			// cardData.BackArtUrl = cardBackUrl;
			// cardData.UID = model.Id;
			// cardData.IsSpawnedByEffect = model.IsSummonedByEffect;
			//
			// if (isCardHidden)
			// 	return cardData;
			//
			// cardData.Attack = new CardStat(model.EntityState.CurrentAttack, model.EntityState.MaxAttack);
			// cardData.Hp = new CardStat(model.EntityState.CurrentHealth, model.EntityState.MaxHealth);
			// cardData.Lava = new CardStat(model.EntityState.CurrentMana, model.EntityState.MaxMana);
			// cardData.EffectsContainer = model.EntityState.CurrentEffects.ToEffectContainer();

			return default;
		}

		public static RoundData ToRoundData(this RoundMessage message)
		{
			// return new RoundData
			// {
			// 	RoundNumber = message.RoundModel.RoundNumber,
			// 	TurnOwner = ActorsContextResolver.GetOwnerByTurn(message.RoundModel.RoundNumber),
			// 	TurnTimerEndsOn = message.RoundModel.TurnTimerEndsOn
			// };
			return default;
		}

		public static RoundData ToRoundData(this SessionContextModel message)
		{
			// var selfIndex = message.Players.First(ResolverHelpers.IsPlayerSelf).PlayerIndex;
			// return new RoundData
			// {
			// 	RoundNumber = message.RoundNumber,
			// 	TurnOwner = ActorsContextResolver.GetOwnerByTurn( message.RoundNumber, selfIndex),
			// 	TurnTimerEndsOn = message.TurnTimerEndsOn
			// };
			return default;
		}

		public static ActorData ToActorData(this SessionPlayerModel model, Owner owner)
		{
			// var avatarItemDto = model.UICustomizations?.FirstOrDefault(x => x.Type == CustomisationType.AvatarFrame);
			// var avatarItemUrl = avatarItemDto.GetCustomisationAsset<AssetData>()?.URL;
			// var actor = new ActorData
			// {
			// 	Id = model.Id,
			// 	UID = model.VulcaniteModel.Id,
			// 	Title = model.UserName,
			// 	ArtUrl = GameDataBaseAdapter.Instance.GetHero(model.VulcaniteModel.CardId)?.ArtUrl,
			// 	//TODO: bot verification required
			// 	IsSpawnedByResolver = owner == Owner.Opponent,
			// 	Hp = new CardStat(model.VulcaniteModel.EntityState.CurrentHealth, model.VulcaniteModel.EntityState.MaxHealth),
			// 	Lava = new CardStat(model.VulcaniteModel.EntityState.CurrentMana, model.VulcaniteModel.EntityState.MaxMana),
			// 	Attack = new CardStat(model.VulcaniteModel.EntityState.CurrentAttack, model.VulcaniteModel.EntityState.MaxAttack),
			// 	Owner = owner,
			// 	HasFirstTurn = model.PlayerIndex == 0,
			// 	EffectsContainer = model.VulcaniteModel.EntityState.CurrentEffects.ToEffectContainer(),
			// 	AvatarBorderUrl = avatarItemUrl
			// };
			return default;
		}

		public static DynamicEffectModel ToDynamicEffectModel(this EffectState effectState)
		{
			return default;
		}

		public static EffectsContainer ToEffectContainer(this IEnumerable<DynamicEffectModel> dynamicEffects)
		{
			return default;
		}
		
		public static EffectState ToEffectStateModel(this DynamicEffectModel effectState)
		{
			return default;
		}

		public static string GetEffectDescription(this KeywordData data, Berserk.Shared.Data.Game.EffectData effectData)
		{
			if (string.IsNullOrEmpty(data?.Description) || effectData == null)
				return string.Empty;
			
			return $"{data.Description.Replace("{value}", $"{effectData.Value}")}";
		}
	}
}