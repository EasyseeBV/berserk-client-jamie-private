using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.AiBehaviour.Models;
using Berserk.Shared.GameCore.AiBehaviour.NodeExecutors;

namespace Berserk.Shared.GameCore.AiBehaviour.Controllers
{
	public interface IAiBehaviourExecutor
	{
		BehaviourNodeState ExecuteNode(AiNode node);
	}

	public class AiBehaviourExecutor : IAiBehaviourExecutor
	{
		private readonly Dictionary<AiBehaviourNodeType, AbstractNodeExecutor> executors;
		private readonly ISessionProcessor sessionProcessor;
		public AiBehaviourExecutor(ISessionProcessor sessionProcessor)
		{
			executors = new Dictionary<AiBehaviourNodeType, AbstractNodeExecutor>();
			this.sessionProcessor = sessionProcessor;
		}

		private AbstractNodeExecutor GetExecutor(AiBehaviourNodeType nodeType)
		{
			if (executors.TryGetValue(nodeType, out var executor))
				return executor;

			executor = AiBehaviourFactory.GetExecutor<AbstractNodeExecutor>(nodeType, sessionProcessor);
			executors[nodeType] = executor;
			return executor;
		}
		
		public void Execute(AiNode node)
		{
			ExecuteNode(node);
		}

		public BehaviourNodeState ExecuteNode(AiNode node)
		{
			if (node == null)
				throw new ArgumentNullException($"{nameof(node)} is null");

			if (sessionProcessor.Context.Timer.RuntimeData.State == TimerState.Ended)
				return BehaviourNodeState.Failure;

			var executor = GetExecutor(node.NodeType);
			if (executor == null)
				throw new ArgumentNullException($"Executor for {node.NodeType} not found");
			
			return executor.Execute(node, this);
		}
	}
}