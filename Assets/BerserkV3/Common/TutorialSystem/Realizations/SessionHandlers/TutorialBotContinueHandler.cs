using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Berserk.Shared.Data.Game;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.AiBehaviour.Commands;
using Berserk.Shared.GameCore.Models;
using BerserkV3.GameCore.Network.Abstraction;
using Cysharp.Threading.Tasks;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace BerserkV3.Common.TutorialSystem.SessionHandlers
{
	public class TutorialBotContinueHandler : ITutorialOrderable, ITutorialCloseHandler, ITutorialIdentity
	{
		private readonly IGameHub gameHub;
		private readonly IGameContext gameContext;
		private CancellationTokenSource invokeSource;

		public int Order => -1;

		public string[] Ids { get; } =
		{
			TutorialTrigger.BotContinue.ToString()
		};

		public TutorialBotContinueHandler(
			IGameHub gameHub, 
			IGameContext gameContext)
		{
			this.gameHub = gameHub;
			this.gameContext = gameContext;
		}

		public Task CloseAsync(ITutorialHintEntity hint)
		{
			if (gameContext.RuntimeData.IsEnded)
				return Task.CompletedTask;
			
			var cycle = hint.Conditions
				.Where(x => Ids.Contains(x.Id))
				.Select(x => int.TryParse(x.Meta, out var cycle) ? cycle : 1)
				.DefaultIfEmpty(1)
				.FirstOrDefault();
			
			var param = new RuntimeAiArg {Id = hint.Id, Cycle = cycle};
			var model = new CmdParamsModel (gameContext.Timer.RuntimeData.TimeHash, param);
			gameHub.PerformCommandAsync<PerformAiCmd>(model).Forget();
			return Task.CompletedTask;
		}
	}
}