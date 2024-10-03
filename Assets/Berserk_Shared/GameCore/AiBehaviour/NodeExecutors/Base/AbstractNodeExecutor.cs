using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.AiBehaviour.Controllers;
using Berserk.Shared.GameCore.AiBehaviour.Models;

namespace Berserk.Shared.GameCore.AiBehaviour.NodeExecutors
{
	public abstract class AbstractNodeExecutor
	{
		protected ISessionProcessor SessionProcessor { get; private set; }
		
		public void Build(ISessionProcessor sessionProcessor)
		{
			SessionProcessor = sessionProcessor;
			OnBuild();
		}
		
		public abstract BehaviourNodeState Execute(AiNode node, IAiBehaviourExecutor behaviourExecutor);
		
		protected virtual void OnBuild(){}
	}
}