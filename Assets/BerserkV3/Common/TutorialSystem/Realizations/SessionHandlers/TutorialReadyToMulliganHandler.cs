using System.Threading.Tasks;
using Berserk.Shared.GameCore.Commands.Cmd;
using BerserkV3.GameCore.Network.Abstraction;
using Cysharp.Threading.Tasks;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{

	public class TutorialReadyToMulliganHandler : ITutorialInvokeHandler, ITutorialOrderable, ITutorialIdentity
	{
		private readonly IGameHub gameHub;

		public int Order => -1;

		public string[] Ids { get; } =
		{
			TutorialTrigger.ReadyToMulligan.ToString()
		};

		public TutorialReadyToMulliganHandler(IGameHub gameHub)
		{
			this.gameHub = gameHub;
		}

		public Task InvokeAsync(ITutorialHintEntity hint)
		{
			gameHub.PerformCommandAsync<ReadyToMulliganCmd>().Forget();
			return Task.CompletedTask;
		}
	}
}