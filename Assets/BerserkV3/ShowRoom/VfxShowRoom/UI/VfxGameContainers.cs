using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.UI;
using UnityEngine;

namespace BerserkV3.ShowRoom.VfxShowRoom
{
	public class VfxGameContainers : IGameContainers
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

		public VfxGameContainers(IGameView gameView)
		{
			VfxContainer = gameView.VfxContainer;
			TableContainer = gameView.TableContainer;
			InShowFirstRow = gameView.InShowFirstRow;
			SelfHeroContainer = gameView.SelfHeroContainer;
			OpponentHeroContainer = gameView.OpponentHeroContainer;
			GameContainer = gameView.GameInteractionContainer;
			SelfHandContainer = gameView.SelfHandContainer;
			OpponentHandContainer = gameView.OpponentHandContainer;
			OpponentSpawnContainer = gameView.OpponentSpawnContainer;
			SelfSpawnContainer = gameView.SelfSpawnContainer;
		}
	}
}