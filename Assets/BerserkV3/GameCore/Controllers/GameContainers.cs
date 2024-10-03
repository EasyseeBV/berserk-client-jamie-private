using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.UI;
using UnityEngine;

namespace BerserkV3.GameCore.Controllers
{
	public interface IGameContainers
	{
		Transform GameContainer { get; }
		Transform VfxContainer { get; }
		RectTransform TableContainer { get; }
		RectTransform InShowFirstRow { get; }
		RectTransform InShowSecondRow { get; }
		Transform SelfHandContainer { get; }
		Transform OpponentHandContainer { get; }
		Transform DeckSelfContainer { get; }
		Transform DeckOpponentContainer { get; }
		Transform MulliganContainer { get; }
		Transform ReplaceMulliganContainer { get; }
		RectTransform GraveyardSelfContainer { get; }
		RectTransform GraveyardOpponentContainer { get; }
		Transform OpponentSpawnContainer { get; }
		Transform SelfSpawnContainer { get; }
		Transform SelfHeroContainer { get; }
		Transform OpponentHeroContainer { get; }
	}

	public class GameContainers : IGameContainers
	{
		public Transform GameContainer { get; }
		public Transform VfxContainer { get; }
		public RectTransform TableContainer { get; }
		public RectTransform InShowFirstRow { get; }
		public RectTransform InShowSecondRow { get; }
		public Transform SelfHandContainer { get; }
		public Transform OpponentHandContainer { get; }
		public Transform DeckSelfContainer { get; }
		public Transform DeckOpponentContainer { get; }
		public Transform MulliganContainer { get; }
		public Transform ReplaceMulliganContainer { get; }
		public RectTransform GraveyardSelfContainer { get; }
		public RectTransform GraveyardOpponentContainer { get; }
		public Transform OpponentSpawnContainer { get; }
		public Transform SelfSpawnContainer { get; }
		public Transform SelfHeroContainer { get; }
		public Transform OpponentHeroContainer { get; }

		public GameContainers(
			IGameView gameView, 
			IMulliganView mulliganView, 
			IGraveyardView graveyardView,
			IEnumerable<IDeckHolderView> deckHolderViews)
		{
			VfxContainer = gameView.VfxContainer;
			TableContainer = gameView.TableContainer;
			InShowFirstRow = gameView.InShowFirstRow;
			InShowSecondRow = gameView.InShowSecondRow;
			SelfHeroContainer = gameView.SelfHeroContainer;
			OpponentHeroContainer = gameView.OpponentHeroContainer;
			MulliganContainer = mulliganView.MulliganContainer;
			ReplaceMulliganContainer = mulliganView.MulliganReplaceContainer;
			GraveyardSelfContainer = graveyardView.SelfContainer;
			GraveyardOpponentContainer = graveyardView.OpponentContainer;
			GameContainer = gameView.GameInteractionContainer;
			SelfHandContainer = gameView.SelfHandContainer;
			OpponentHandContainer = gameView.OpponentHandContainer;
			OpponentSpawnContainer = gameView.OpponentSpawnContainer;
			SelfSpawnContainer = gameView.SelfSpawnContainer;
			
			foreach (var deckHolderView in deckHolderViews)
			{
				switch (deckHolderView.Owner)
				{
					case Owner.Self:
						DeckSelfContainer = deckHolderView.TargetView.transform;
						break;
					
					case Owner.Opponent:
						DeckOpponentContainer = deckHolderView.TargetView.transform;
						break;
				}
			}
		}
	}
}