using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;

namespace Berserk.Shared.GameCore.LogicContext
{
	public readonly struct UserAfkEvent : ISharedEvent
	{
		public IRuntimeTimerData RuntimeData { get; }

		public UserAfkEvent(IRuntimeTimerData runtimeData)
		{
			RuntimeData = runtimeData.Clone();
		}
	}
	
	public readonly struct NextTurnEvent : ISharedEvent
	{
		public IRuntimeTimerData RuntimeData { get; }

		public NextTurnEvent(IRuntimeTimerData runtimeData)
		{
			RuntimeData = runtimeData.Clone();
		}
	}
	
	public readonly struct TimerChangedEvent : ISharedEvent
	{
		public IRuntimeTimerData RuntimeData { get; }

		public TimerChangedEvent(IRuntimeTimerData runtimeData)
		{
			RuntimeData = runtimeData.Clone();
		}
	}
	
	public class RuntimeTimer : IRuntimeTimer
	{
		private readonly ISharedConfig config;
		private readonly ISharedTime sharedTime;
		private readonly IPlayerRepository playerRepository;
		private readonly ISharedEventsSource sharedEventsSource;
		public IRuntimeTimerData RuntimeData { get; private set; }

		public RuntimeTimer(
			ISharedConfig config, 
			ISharedTime sharedTime,
			IPlayerRepository playerRepository,
			ISharedEventsSource sharedEventsSource)
		{
			this.config = config;
			this.sharedTime = sharedTime;
			this.playerRepository = playerRepository;
			this.sharedEventsSource = sharedEventsSource;
		}

		public IRuntimeTimer Sync(IRuntimeTimerData runtimeData)
		{
			RuntimeData = runtimeData;
			return this;
		}
		
		public IRuntimeTimer SetState(TimerState state, bool notify = true)
		{
			RuntimeData.State = state;
			Reset(false);

			if (notify)
				sharedEventsSource.Publish(new TimerChangedEvent(RuntimeData));
			
			return this;
		}

		public IRuntimeTimer SetOwner(string userId, bool notify = true)
		{
			RuntimeData.OwnerId = userId;
			RuntimeData.TimeHash = RuntimeData.GetHashCode();
			
			if (notify)
				sharedEventsSource.Publish(new TimerChangedEvent(RuntimeData));
			
			return this;
		}

		public int GetFullTimeBasedOnState()
		{
			return RuntimeData.State switch
			{
				TimerState.Mulligan or TimerState.NotStarted => config.InMilliganTimerSec,
				TimerState.Game or TimerState.Ended  => config.InGameTimerSec,
				TimerState.Ready => config.InGameReadyTimerSec,
				_ => throw new ArgumentOutOfRangeException()
			};
		}

		public int GetTimeLeft()
		{
			var currentTime = RuntimeData.Paused ?? sharedTime.Current;
			return (int) Math.Ceiling(Math.Max(0, (RuntimeData.EndTime - currentTime).TotalSeconds - GetClientOffesetBasedOnState()));
		}

		public void NextTurn()
		{
			if (!CheckAfk())
				return;

			Reset(false);
			var otherUserId = playerRepository.GetOpposite(RuntimeData.OwnerId).RuntimeData.UserId;

			SetOwner(otherUserId, false);

			if (RuntimeData.Turn % 2 == 0)
				++RuntimeData.Round;

			++RuntimeData.Turn;

			RuntimeData.TimeHash = RuntimeData.GetHashCode();
			sharedEventsSource.Publish(new NextTurnEvent(RuntimeData));
		}

		public void Pause(bool notify = true)
		{
			RuntimeData.Paused = sharedTime.Current;
			RuntimeData.TimeHash = RuntimeData.GetHashCode();
			
			if (notify)
				sharedEventsSource.Publish(new TimerChangedEvent(RuntimeData));
		}

		public void Unpause(bool notify = true)
		{
			if (RuntimeData.Paused.HasValue)
			{
				var offset = sharedTime.Current - RuntimeData.Paused.Value;
				var endTime = RuntimeData.EndTime.Add(offset);
				var duration = endTime - sharedTime.Current;
				SetEndTime(endTime, (int)duration.TotalSeconds);
			}
			
			RuntimeData.Paused = null;
			RuntimeData.TimeHash = RuntimeData.GetHashCode();
			
			if (notify)
				sharedEventsSource.Publish(new TimerChangedEvent(RuntimeData));
		}

		public override string ToString()
		{
			return $"[{RuntimeData.OwnerId}]: {RuntimeData.State}, EndTime:{RuntimeData.EndTime}";
		}

		public void Reset(bool notify = true)
		{
			var duration = TimeSpan.FromSeconds(GetFullTimeBasedOnState());
			var endTime = sharedTime.Current.Add(duration);
			SetEndTime(endTime, (int) duration.TotalSeconds);
			
			if (RuntimeData.Paused.HasValue)
				RuntimeData.Paused = sharedTime.Current;
			
			RuntimeData.TimeHash = RuntimeData.GetHashCode();
			
			if (notify)
				sharedEventsSource.Publish(new TimerChangedEvent(RuntimeData));
		}

		private void SetEndTime(DateTime endTime, int duration)
		{
			RuntimeData.EndTime = endTime;
			RuntimeData.Duration = duration;
		}
		
		public int GetClientOffesetBasedOnState()
		{
			return RuntimeData.State switch
			{
				TimerState.Game => config.ClientsOffsetTimeSec,
				TimerState.Mulligan => config.ClientsOffsetTimeSec,
				_ => 0
			};
		}

		private bool CheckAfk()
		{
			if (string.IsNullOrEmpty(RuntimeData.OwnerId))
				return true;

			var ownerPlayerContext = playerRepository.Get(RuntimeData.OwnerId).RuntimeData;
			if (RuntimeData.Round - ownerPlayerContext.LastRoundWithActive < 3 || ownerPlayerContext.IsBot)
				return true;
			
			sharedEventsSource.Publish(new UserAfkEvent(RuntimeData));
			return false;
		}
	}
}