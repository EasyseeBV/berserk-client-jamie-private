using UnityEngine;

namespace RR.Game.TutorialSystem.Domain.Data
{
	public class TutorDynamicArrowData
	{
		public Vector3 From { get; }
		public Vector3 To { get; }
		public TutorDynamicArrowData(Vector3 from, Vector3 to)
		{
			From = from;
			To = to;
		}
	}
}