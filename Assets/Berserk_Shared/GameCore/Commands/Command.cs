using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Attributes;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Models;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.Commands
{
	public abstract class Command<T> : Command
	{
		protected T ArgsModel;

		public override Command Build(
			string userId,
			IGameContext gameContext, 
			IGameLogicContext gameLogicContext,
			CmdParamsModel model)
		{
			try
			{
				if (!string.IsNullOrEmpty(model.MetaJson))
					ArgsModel = JsonConvert.DeserializeObject<T>(model.MetaJson);
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}

			return base.Build(userId, gameContext, gameLogicContext, model);
		}
	}

	public abstract class Command
	{
		protected IRuntimePlayer RuntimePlayer;
		protected IGameContext GameContext;
		protected IGameLogicContext LogicContext;
		protected IRuntimeGameObject Executor;
		protected CmdParamsModel Model;
		
		public IRuntimeGameObject[] Targets;
		public string CommandId { get; private set; }
		public bool Executed { get; private set; }
		public bool IsNested { get; set; }
		protected bool Cancelled { get; private set; }
		protected string Meta;

		public virtual Command Build(
			string userId,
			IGameContext gameContext, 
			IGameLogicContext gameLogicContext,
			CmdParamsModel model)
		{
			GameContext = gameContext;
			LogicContext = gameLogicContext;
			RuntimePlayer = gameContext.PlayerRepository.Get(userId);

			Executor = gameContext.GameRuntimePool.Get(model.ExecutorObjectId);
			Targets = gameContext.GameRuntimePool.GetMany(model.TargetObjectsIds).ToArray();
			
			Model = model;
			CommandId = model.CommandId;
			Meta = model.MetaJson;
			
			// will throw in case command is written with error
			HandleAttribute();

			return this;
		}

		private void HandleAttribute()
		{
			var atb = (CmdRequiredTypeAttribute)Attribute
				.GetCustomAttribute(GetType(), typeof(CmdRequiredTypeAttribute));

			if (atb == null)
				return;

			if (atb.Targets != null && Targets.Any())
			{
				var isAllTargetsMatchType = Targets.All(x => atb.Targets.IsInstanceOfType(x));
				if (!isAllTargetsMatchType)
					throw new ArrayTypeMismatchException(
						$"{nameof(Targets)} [{Targets.First().GetType().Name}] item type of {nameof(Targets)} must be exactly of {atb.Targets.Name} type!");
			}

			if (atb.Executor != null && Executor != null)
			{
				var executorType = Executor.GetType();
				if (!atb.Executor.IsAssignableFrom(executorType))
					throw new ArrayTypeMismatchException(
						$"{nameof(Executor)} [{executorType.Name}] must be exactly of {atb.Executor.Name} type!");
			}
		}

		public void Execute()
		{
			if (Executor?.RuntimeData != null && Targets is {Length: > 0})
				LogicContext.TargetResolver.AddManualTargets(Executor.RuntimeData.Id, Targets);

			try
			{
				OnExecute();
				Executed = true;
			}
			finally // anyway rethrow and will handle remove targets
			{
				if (Executor?.RuntimeData != null)
					LogicContext.TargetResolver.RemoveManualTargets(Executor.RuntimeData.Id);
			}
		}

		public void Cancel()
		{
			if (Cancelled)
				return;

			Cancelled = true;
			
			if (Executor?.RuntimeData != null)
				LogicContext.TargetResolver.RemoveManualTargets(Executor.RuntimeData.Id);
			
			OnCancel();
		}

		protected abstract void OnExecute();

		protected virtual void OnCancel()
		{
			DefaultSharedLogger.Error($"Command.OnCancel : Not implemented for {GetType().Name}");
		}
	}
}