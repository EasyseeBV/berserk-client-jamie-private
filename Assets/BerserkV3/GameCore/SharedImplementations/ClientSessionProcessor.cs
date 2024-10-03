using System;
using System.Linq;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using BerserkV3.GameCore.Repository;
using RR.Core.Extensions;

namespace BerserkV3.GameCore.SharedImplementations
{
	public class ClientSessionProcessor : SessionProcessorBase
	{
		private readonly IGameRepository gameRepository;
		private bool installed;

		public ClientSessionProcessor(
			IGameContext context, 
			IGameLogicContext logicContext,
			IGameRepository gameRepository) 
			: base(context, logicContext)
		{
			this.gameRepository = gameRepository;
		}

		public override ISessionProcessor Build(params object[] args)
		{
			if (args.OfType<InitializeGame>().FirstOrDefault() is not {} data)
				throw new ArgumentException($"Initialization event is empty : {nameof(InitializeGame)}");

			if (Context.PlayerRepository.Count < data.RuntimePlayerDatas.Length)
			{
				var runtimePlayers = Enumerable
					.Range(0, data.RuntimePlayerDatas.Length - Context.PlayerRepository.Count)
					.Select(_ => new RuntimePlayer());
				
				Context.PlayerRepository.AddRange(runtimePlayers);
			}
			
			Context.Sync(data.RuntimeContextData);
			Context.PlayerRepository.Sync(data.RuntimePlayerDatas);
			Context.RandomGenerator.Sync(data.RuntimeGeneratorsData.RandomGenerator);
			Context.RuntimeIdGenerator.Sync(data.RuntimeGeneratorsData.IdGenerator);
			Context.OrderGenerator.Sync(data.RuntimeGeneratorsData.OrderGenerator);
			Context.Timer.Sync(data.RuntimeTimerData);
			LogicContext.TurnController.Init(false);
			gameRepository.Initialize(Context.PlayerRepository.GetOpposite(gameRepository.SelfId).UserId);
			return this;
		}
	}
}