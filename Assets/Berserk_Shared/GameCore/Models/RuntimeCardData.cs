using System;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Game;
using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.Models
{
	public class RuntimeCardData : RuntimeData, IRuntimeCardData
	{
		public event Action<RuntimeState, RuntimeState> OnStateChanged;
		public event Action<RuntimeState, RuntimeState> OnEarlyStateChanged;
		public bool IsToken { get; set; }

		public RuntimeState State { get; set; }
		public RuntimeState PreviousState { get; set; }
		public int RelativePositionX { get; set; } = -1;
		

		[JsonConstructor]
		public RuntimeCardData()
		{
		}

		public RuntimeCardData(IObjectData data) : base(data)
		{
			if (data is ICardData {SubTypes: {}} cardData)
				TurnToken(cardData.SubTypes.Contains(SubType.Token));
		}

		public void TurnToken(bool value)
		{
			IsToken = value;
		}

		public void SetRelativePositionX(int value)
		{
			RelativePositionX = value;
		}

		public void ResetRelativePositionX()
		{
			RelativePositionX = -1;
		}

		public void SetState(RuntimeState current, RuntimeState? prev = null)
		{
			SetStateWithoutNotify(current, prev);
			TriggerStateChanged();
		}

		public void SetStateWithoutNotify(RuntimeState current, RuntimeState? prev = null)
		{
			PreviousState = prev ?? State;
			State = current;
		}

		public void TriggerStateChanged()
		{
			OnEarlyStateChanged?.Invoke(PreviousState, State);
			OnStateChanged?.Invoke(PreviousState, State);
		}

		public override void Dispose()
		{
			base.Dispose();
			OnStateChanged = null;
			OnEarlyStateChanged = null;
			IsToken = default;
			State = default;
			PreviousState = default;
			RelativePositionX = default;
		}
	}
}
