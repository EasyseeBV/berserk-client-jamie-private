using System;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands;
using Berserk.Shared.GameCore.Exceptions;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.GameCore.Controllers
{
	public readonly struct BeforeCommandExecutedEvent : ISharedEvent
	{
		public Command TargetCommand { get; }
		public BeforeCommandExecutedEvent(Command targetCommand)
		{
			TargetCommand = targetCommand;
		}
	}
	
	public readonly struct AfterCommandExecutedEvent : ISharedEvent
	{
		public Command TargetCommand { get; }
		public AfterCommandExecutedEvent(Command targetCommand)
		{
			TargetCommand = targetCommand;
		}
	}
	
	public class CommandController : ICommandController
	{
		private readonly IGameContext context;
		private readonly IGameLogicContext logicContext;

		public CommandController(IGameContext context, IGameLogicContext logicContext)
		{
			this.context = context;
			this.logicContext = logicContext;
		}

		public void Execute<T>(string userId, CmdParamsModel cmdParamsModel, bool isNested = false) where T : Command
		{
			Execute(userId, typeof(T).Name, cmdParamsModel, isNested);
		}
		
		public void Execute(string userId, string cmd, CmdParamsModel cmdParamsModel, bool isNested = false)
		{
			Command current = null;
			try
			{
				if (!isNested // If the parent command is executed, then the nested command is allowed to execute.
				    && !CmdExecutionLevel.WrapAllowed.IsCmdLevelAvailable(cmd) // check if command not allowed to round wrap.
				    && cmdParamsModel.TimeHash != context.Timer.RuntimeData.TimeHash) // check if command sent from old turn then cancel it.
					throw new InvalidOperationException($"Command can't execute time hash isn't correct: {cmdParamsModel.TimeHash}, expected : {context.Timer.RuntimeData.TimeHash}");
				
				current = CommandFactory.Create<Command>(cmd);
				current.IsNested = isNested;
				current.Build(userId, context, logicContext, cmdParamsModel);
				context.SharedEventsSource.Publish(new BeforeCommandExecutedEvent(current));
				current.Execute();
				context.SharedEventsSource.Publish(new AfterCommandExecutedEvent(current));
			}
			catch (InvalidActionException e)
			{
				logicContext.LogicQueueController.Add(new InvalidActionEvent(e.Value, cmdParamsModel.CommandId), userId);
				CancelCommand(e.Message, cmdParamsModel.CommandId, userId, isNested, current);
				return;
			}
			catch (Exception e)
			{
				CancelCommand(e.Message, cmdParamsModel.CommandId, userId, isNested, current);
				return;
			}

			ApproveCommand(cmdParamsModel.CommandId, userId, isNested);
		}

		private void ApproveCommand(string commandId, string userId, bool isNested)
		{
			if (isNested)
				return;
			
			logicContext.LogicQueueController.Add(new CommandApprove(commandId), userId);
			logicContext.LogicQueueController.SendAndClearLogicQueue();
		}
		
		private void CancelCommand(string message, string commandId, string userId, bool isNested, Command command = null)
		{
			if (!isNested)
				logicContext.LogicQueueController.Add(new CommandCancel(commandId, message), userId);
			
			if (command is {Executed: true} && command.CommandId == commandId)
				command.Cancel();
			
			if (!isNested)
				logicContext.LogicQueueController.SendAndClearLogicQueue();
		}
	}
}