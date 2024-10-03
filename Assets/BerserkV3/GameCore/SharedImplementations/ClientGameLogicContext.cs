using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Controllers;
using Berserk.Shared.GameCore.EffectSystem;
using Berserk.Shared.GameCore.EffectSystem.TargetSystem;
using Berserk.Shared.GameCore.LogicContext;

namespace BerserkV3.GameCore.SharedImplementations
{
	public class ClientGameLogicContext : IGameLogicContext
	{
		private readonly IGameContext gameContext;
		private readonly IGiveCardsServiceFactory giveCardsServiceFactory;
		private IGiveCardsService giveCardsService;
		
		public ICommandController CommandController { get; }
		public ILogicQueueController LogicQueueController { get; }
		public IGiveCardsService GiveCardsService => giveCardsService ??= giveCardsServiceFactory.Create(gameContext, this);
		public IRuntimeStateController RuntimeStateController { get; }
		public IRuntimeCardPositionController RuntimeCardPositionController { get; }
		public IEffectExecutor EffectExecutor { get; }
		public IEffectsFactory EffectsFactory { get; }
		public IRuntimeFactory RuntimeFactory { get; }
		public ITargetConditionRepository TargetConditionRepository { get; }
		public IExecutorConditionRepository ExecutorConditionRepository { get; }
		public IEffectPhaseProcessor EffectPhaseProcessor { get; }
		public ITurnController TurnController { get; }
		public ITargetResolver TargetResolver { get; }
		
		public ClientGameLogicContext(IGameContext gameContext, IGiveCardsServiceFactory giveCardsServiceFactory)
		{
			EffectExecutor = new ClientEffectExecutor();
			EffectsFactory = new ClientEffectsFactory();
			
			CommandController = new CommandController(gameContext, this);
			LogicQueueController = new LogicQueueController(gameContext);
			RuntimeStateController = new RuntimeStateController(gameContext, this);
			RuntimeCardPositionController = new RuntimeCardPositionController(gameContext, this);
			RuntimeFactory = new RuntimeFactory(gameContext, this);
			TargetConditionRepository = new TargetConditionRepository(gameContext);
			ExecutorConditionRepository = new ExecutorConditionRepository(gameContext);
			EffectPhaseProcessor = new EffectPhaseProcessor(gameContext, this);
			TurnController = new TurnController(gameContext, this);
			TargetResolver = new TargetResolver(TargetConditionRepository);
			this.gameContext = gameContext;
			this.giveCardsServiceFactory = giveCardsServiceFactory;
		}
		
		public void Dispose()
		{
			LogicQueueController?.Dispose();
			TurnController?.Dispose();
		}
	}
}