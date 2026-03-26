using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.ProgressDrawer;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.Utils;
using BerserkV3.Lobby.Deck;
using BerserkV3.Lobby.Network;
using Cysharp.Threading.Tasks;

namespace BerserkV3.Lobby.MatchMaking.Practice
{
	public class PracticeApplication : IPracticeApplication
	{
		private readonly ISharedConfig sharedConfig;
		private readonly ISceneService sceneService;
		private readonly IProgressDrawer progressDrawer;
		private readonly IDeckApplication deckApplication;

		public PracticeApplication(
			ISharedConfig sharedConfig,
			ISceneService sceneService,
			IProgressDrawer progressDrawer,
			IDeckApplication deckApplication)
		{
			this.sharedConfig = sharedConfig;
			this.sceneService = sceneService;
			this.progressDrawer = progressDrawer;
			this.deckApplication = deckApplication;
		}

		public async UniTask<bool> StartPracticeMatchAsync()
		{
			var model = new LobbyPracticeStartSessionModel
			{
				Difficulty = PracticeMode.Hard,
				MatchMode = MatchMode.Practice,
				DeckId = deckApplication.Current?.Id
			};
			return await LobbyAPI.PostPracticeStartSession(model);
		}
		
		public async UniTask<bool> StartTutorialMatchAsync()
		{
			var model = new LobbyPracticeStartSessionModel
			{
				Difficulty = PracticeMode.Tutorial,
				MatchMode = MatchMode.Tutorial,
				DeckId = null
			};
			
			return await LobbyAPI.PostPracticeStartSession(model);
		}
		
		public async UniTask<bool> JoinGameAsync()
		{
			var roomResponse = await LobbyAPI.GetActiveSession();
			if (roomResponse && roomResponse.Data != null)
				return await JoinGameAsync(roomResponse.Data);
			
			await LobbyAPI.PostDuelLeaveRoom();
			return false;
		}
		
		public async UniTask<bool> JoinGameAsync(ActiveSessionModel model)
		{
			await progressDrawer.SetProgressTypeAsync(ProgressType.Versus);
			progressDrawer.CreateProgress()
				.ProgressTickAsync(sharedConfig.GameLoadingPreviewTime, 10)
				.Forget();
			
			await progressDrawer.ShowAsync(model.Players.ToArray<object>());
			progressDrawer.AddProgress(sceneService.Load(Scene.Game));
			return true;
		}
	}
}
