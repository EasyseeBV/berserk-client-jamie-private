using Newtonsoft.Json;

namespace Berserk.Shared.GameCore.LogicEvents
{
	public class PlayEmotion : LogicEvent
	{
		public string SenderId { get; }
		public string EmotionId { get; }
		
		[JsonConstructor]
		public PlayEmotion(string senderId, string emotionId)
		{
			SenderId = senderId;
			EmotionId = emotionId;
		}
	}
}