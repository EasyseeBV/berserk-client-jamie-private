using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Serialization;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Controllers;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.LogicEvents;
using Berserk.Shared.GameCore.Models;
using Berserk.Shared.GameCore.Utils;
using Berserk.Shared.SignalR.Common;
using Berserk.Shared.SignalR.Enums;
using Berserk.Shared.SignalR.Lobby;
using BerserkV3.Common.Network;
using BerserkV3.Common.Network.SignalR;
using BerserkV3.GameCore.LocalStateValidator;
using BerserkV3.GameCore.LogicEventsProcessor;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.Prediction.Abstraction;
using BerserkV3.Generic.UndoSystem;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Applications;
using BerserkV3.Startup.Authorization;
using BestHTTP.SignalRCore;
using BestHTTP.SignalRCore.Messages;
using Cysharp.Threading.Tasks;
using GameCore;
using Newtonsoft.Json;
using RR.Core.Extensions;
using Zenject;
using Command = Berserk.Shared.GameCore.Commands.Command;

namespace BerserkV3.GameCore.Network
{
	public class GameHub: BaseSignalHub, IGameHub, IInitializable
	{
		protected override string AuthToken => User.AccessToken;
		protected override string ServerAddress  => URLs.HubUrl;
		public override string HubName => "game";

		private readonly IInstantiator instantiator;
		private readonly IGuestApplication guestApplication;
		private readonly IPredictProcessor predictProcessor;
		private readonly ISignalBuffer<ILogicEvent> signalBuffer;
		private readonly IGameLogicEventsProcessor logicEventsProcessor;
		private readonly ILocalContextValidator localContextValidator;
		private readonly ISessionProcessor sessionProcessor;
		private readonly IUndoSystem undoSystem;
		private bool clientInitialized;

		public GameHub(
			IGuestApplication guestApplication,
			IPredictProcessor predictProcessor,
			ISignalBuffer<ILogicEvent> signalBuffer,
			IGameLogicEventsProcessor logicEventsProcessor,
			ILocalContextValidator localContextValidator,
			ISessionProcessor sessionProcessor,
			IUndoSystem undoSystem)
		{
			this.guestApplication = guestApplication;
			this.predictProcessor = predictProcessor;
			this.signalBuffer = signalBuffer;
			this.logicEventsProcessor = logicEventsProcessor;
			this.localContextValidator = localContextValidator;
			this.sessionProcessor = sessionProcessor;
			this.undoSystem = undoSystem;
		}
		
		public void Initialize()
		{
			sessionProcessor.Context.SharedEventsSource.Subscribe<BeforeCommandExecutedEvent>(RegisterUndoBeforeCommandExecuted, CancellationToken.None);
		}
		
		public override void Dispose()
		{
			base.Dispose();
			clientInitialized = false;
		}

		// TODO outgoing signals must be buffered before the connection is re-established.
		public async UniTask PerformCommandAsync<T>(CmdParamsModel model = null, bool predict = false) where T : Command
		{
			if (!Initialized)
				return;

			var cmdName = typeof(T).Name;
			GameCoreBus.OnCommandExecute.Publish(cmdName);
			DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] " +
			                        $"Try send {cmdName.Orange()} {JsonConvert.SerializeObject(model)}");
			model ??= new CmdParamsModel(sessionProcessor.Context.Timer.RuntimeData.TimeHash);
			logicEventsProcessor.ProcessUnQueueAsync(new CommandWait(model.CommandId)).Forget();
			
			if (predict)
				predictProcessor.PerformCmd<T>(model);
			
			await SendAsync("PerformCommand", cmdName, JsonConvert.SerializeObject(model, SharedSerializationHelper.SerializeSettings));

			DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] Sent successful {cmdName.Green()}");
		}

		public async UniTask ReadyToInitializeAsync()
		{
			DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] Try send ReadyToInitialize");
			await SendAsync("ReadyToInitialize").AsUniTask();
			DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] Sent successful ReadyToInitialize");
		}
		
		private bool HandleInternalSignalTypes(Message message)
		{
			if (message.type is MessageTypes.Ping or MessageTypes.Completion)
				return false;

			if (message.type != MessageTypes.Close)
			{
				return false;
			}
			

			if (sessionProcessor.Context.RuntimeData == null)
			{
				RestartRequired();
				return true;
			}

			if (!sessionProcessor.Context.RuntimeData.IsEnded)
				RestartRequired();
			
			return true; 
		}
		
		protected override void HandleMessage(Message message)
		{
			if (HandleInternalSignalTypes(message))
				return;

			if (!Enum.TryParse(message.target, out SignalType signalType))
				return;
			
			if (message.arguments == null || message.arguments.Length == 0)
			{
				var errorMessage = $"Not fount argument in message - {message}";
				DefaultSharedLogger.Error(errorMessage);
				return;
			}
			
			var argument = (string)message.arguments[0];
			
			switch (signalType)
			{
				case SignalType.DropConnection :
					if (sessionProcessor.Context.RuntimeData == null)
					{
						RestartRequired();
						return;
					}
					
					if (!sessionProcessor.Context.RuntimeData.IsEnded)
						RestartRequired();
					return;
				
				case SignalType.Error :
					var errorMessage = argument;
					try
					{
						var errorModel = JsonConvert.DeserializeObject<ErrorModel>(argument);
						errorMessage = errorModel!.Message;
					}
					catch (Exception)
					{
						// ignored
					}

					DefaultSharedLogger.Error($"[{GetType().Name.ToUpper().Cyan()}] ERROR: {errorMessage}, json : {argument}");
					return;
				
				case SignalType.Ping :
					DefaultSharedLogger.Log($"[{GetType().Name.ToUpper().Cyan()}] INFO: {argument}");
					return;

				case SignalType.LogicEvents :
					HandleLogicEvents(argument);
					return;
				
				case SignalType.Reauthorization :
					var reauthSignal = JsonConvert.DeserializeObject<ReauthorizeSignal>(argument);
					GameCoreBus.OnReauthorizationRequired.Publish(reauthSignal.Message);
					return;
				
				default: 
					DefaultSharedLogger.Error($"[{GetType().Name.ToUpper().Cyan()}] " +
					                          $"Unhandled SignalType {message.target}, json : {argument}");
					return;
			}
		}

		private void HandleLogicEvents(string json)
		{ 
			try
			{
				var logicEvents = JsonConvert.DeserializeObject<List<ILogicEvent>>(json, SharedSerializationHelper.GetDeserializeSettingsByType<ILogicEvent>());
				if (logicEvents.Any(x => x == null))
				{
					DefaultSharedLogger.Error(
						$"{GetType().Name.ToUpper().Yellow()} Received messages: has empty events!".Red() +
						$"\n Json : \n{json}");
					logicEvents.RemoveAll(x => x == null);
				}

				DefaultSharedLogger.Log($"{GetType().Name.ToUpper().Yellow()} Received messages: - {logicEvents.JoinToString("\n")}");
					
				if (clientInitialized && !localContextValidator.Validate(logicEvents))
				{
					RestartRequired();
					return;
				}
					
				// while the server is sending the Init logic, the signals will be added to the buffer
				// while the buffer is being resolved, it is necessary to add incoming events to the buffer
				if (!clientInitialized || signalBuffer.Unpacking)
				{
					var hasInitEvent = logicEvents.OfType<InitializeGame>().Any();
					if (!clientInitialized && !hasInitEvent) // can't add events which older than the context
						return;
						
					foreach (var logicEvent in logicEvents) // add all messages to a buffer for further processing.
						signalBuffer.Add(logicEvent);

					if (clientInitialized || !hasInitEvent) // when the client has received the initialization message, just wait.
						return;
						
					clientInitialized = true;
					signalBuffer.Execute();
					return;
				}

				TryHandleEndGame(logicEvents);
				predictProcessor.ProcessPredictedQueue(logicEvents);
				logicEventsProcessor.Process(logicEvents.ToArray());
			}
			catch (Exception exception)
			{
				DefaultSharedLogger.Error($"Error processing logic\n{exception}\n{json}");
			}
		}
		
		private void TryHandleEndGame(ICollection<ILogicEvent> logicEvents)
		{
			var endGameEvents = logicEvents.OfType<EndGame>().ToList();
			if(!endGameEvents.Any()) 
				return;
			
			sessionProcessor.Context.End();
			if (User.IsAnonymous && sessionProcessor.Context.RuntimeData.MatchMode != MatchMode.Tutorial)
			{
				guestApplication.ExecuteActionAsync(GuestAction.GameEnded).Forget(DefaultSharedLogger.Error);
			}
			
			if (endGameEvents.All(x => x.Reason != GameEndReason.Surrender || User.Id != x.LooserId))
				return;

			var handleUnqueueEvents = logicEvents
				.OfType<ChangedPlayerStatistic>()
				.Union<ILogicEvent>(endGameEvents)
				.OrderBy(x=> x.Order)
				.ToList();
			
			foreach (var logicEvent in handleUnqueueEvents)
			{
				logicEvents.Remove(logicEvent);
				logicEventsProcessor.ProcessUnQueueAsync(logicEvent);
			}
		}

		protected override void OnConnected(HubConnection connection)
		{
			base.OnConnected(connection);
			if (clientInitialized)
				undoSystem.Undo();
		}

		private void RegisterUndoBeforeCommandExecuted(BeforeCommandExecutedEvent data)
		{
			if (data.TargetCommand != null && !string.IsNullOrEmpty(data.TargetCommand.CommandId))
				undoSystem.Add(data.TargetCommand.CommandId, data.TargetCommand.Cancel);
		}
	}
}