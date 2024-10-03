using System.Collections.Generic;
using System.Threading;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.TutorialSystem;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Game.TutorialSystemV2.Realizations;
using RR.UI.FrameSystem;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.GameCore.UI
{
	public interface IGraveyardPileView
	{
		Owner Owner { get; }
		GameObject TargetView { get; }
		void UpdatePile(int cardsCount);
		UniTask SetArt(string artUrl, CancellationToken token);
	}

	public partial class GraveyardPileView : BaseView, IGraveyardPileView
	{
		[SerializeField] private List<RawImage> cards;
		[SerializeField] private Owner owner = Owner.None;
		public new Owner Owner => owner;
		public GameObject TargetView => gameObject;

		protected override void OnAwake()
		{
			base.OnAwake();
			cards.ForEach(x => SetActive(x, false));
			this.SetHintTarget($"{TutorialTrigger.GameGraveyard}_{owner}").SetTransitionFactorSize().Init();
		}

		private void OnDestroy()
		{
			cards?.ForEach(x=> x.ReleaseResource());
		}

		public UniTask SetArt(string artUrl, CancellationToken token)
		{
			return UniTask.WhenAll(cards.Select(x => x.LoadResourceAsync(artUrl, token)));
		}
		
		public void UpdatePile(int cardsCount)
		{
			for (var i = 0; i < cards.Count; i++)
			{
				var cardImage = cards[i];
				if (i < cardsCount)
				{
					SetActive(cardImage, true);
					if (i == cardsCount - 1)
						cardImage.transform.SetAsLastSibling();
				}
				else
				{
					SetActive(cardImage, false);
				}
			}
		}
	}
}