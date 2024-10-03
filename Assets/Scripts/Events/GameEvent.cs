using System.Collections;

namespace Events
{
	/// <summary>
	///     Used by <see cref="GameBus.ActionQueue" />
	/// </summary>
	public class GameEvent
	{
		public GameEventType Type;
		public long Timestamp;
		public IEnumerator Action;

		public GameEvent(GameEventType type, IEnumerator action, long timestamp)
		{
			Type = type;
			Timestamp = timestamp;
			Action = action;
		}
	}

	public enum GameEventType
	{
		ModifyEntity,
		PlayCard,
		AddCard,
		RemoveCard,
		ChangeHandCard,
		NextRound,
		Resign,
		ResolveAI,
		PerformAction
	}
}