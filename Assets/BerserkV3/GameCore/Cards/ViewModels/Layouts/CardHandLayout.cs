using System;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Game;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using RR.Game.TutorialSystemV2.Realizations;

namespace BerserkV3.GameCore.Cards
{

	public partial class CardHandLayout : BaseCardLayout, IHandCardLayout
	{
		private new IRuntimeGameCard RuntimeGameObject => (IRuntimeGameCard) base.RuntimeGameObject;
		private new IRuntimeCardData RuntimeData => RuntimeGameObject?.RuntimeData;

		public CardHandArtView HandArtView => HandCardArtView;
		
		private Faction deckFaction = Faction.None;
		public void SetDeckFaction(Faction faction) => deckFaction = faction;
		
		protected override async UniTask OnEnabledAsync(CancellationToken token)
		{
			if (RuntimeGameObject.Data is not CardData data)
				throw new NotImplementedException($"Can't use data for this layout : {RuntimeGameObject.Data}");

			SetActive(ExchangeTxt,false);
			token.ThrowIfCancellationRequested();

			SelfContainer
				.SetHintTarget($"{TutorialTrigger.FullCardBounds}_{GetOwner()}_{RuntimeData.State}_{RuntimeGameObject.Data.Id}")
				.Init();
			
			var adapter = data.ToCardDataAdapter();


			if (deckFaction != Faction.None)
				adapter = adapter.ApplyDeckFactionCost(deckFaction, maxLava: 10);


			await UniTask.WhenAll(
				base.OnEnabledAsync(token),
				HandCardArtView.SetupAsync(adapter, token));

			token.ThrowIfCancellationRequested();

			HandCardArtView.SetAttackText(RuntimeData.Attack);
			HandCardArtView.SetArmorText(RuntimeData.Armor);
			HandCardArtView.SetHealthText(RuntimeData.Hp);
			HandCardArtView.SetManaText(adapter.Lava);

			RuntimeData.Attack.OnChanged += HandCardArtView.SetAttackText;
			RuntimeData.Armor.OnChanged += HandCardArtView.SetArmorText;
			RuntimeData.Hp.OnChanged += HandCardArtView.SetHealthText;
		}


		protected override void OnDisabled()
		{
			base.OnDisabled();

			if (RuntimeData != null)
			{
				if (RuntimeData.Attack != null)
					RuntimeData.Attack.OnChanged -= HandCardArtView.SetAttackText;
			
				if (RuntimeData.Armor != null)
					RuntimeData.Armor.OnChanged -= HandCardArtView.SetArmorText;

				if (RuntimeData.Mana != null)
					RuntimeData.Mana.OnChanged -= HandCardArtView.SetManaText;

				if (RuntimeData.Hp != null)
					RuntimeData.Hp.OnChanged -= HandCardArtView.SetHealthText;

			}

			HandCardArtView.Release();
		}
		public void SetTitleText(string value)
		{
			if (!ExchangeTxt)
				return;

			SetActive(ExchangeTxt, !string.IsNullOrEmpty(value));
			Set(ExchangeTxt, value);
		}
	}

}