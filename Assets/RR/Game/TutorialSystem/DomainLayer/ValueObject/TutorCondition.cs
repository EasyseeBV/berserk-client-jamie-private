namespace RR.Game.TutorialSystem.Domain.Data
{
	public class TutorCondition
	{
		private string[] requiredCompletedHintIds;

		public TutorCondition(params string[] requiredCompletedHintIds)
		{
			this.requiredCompletedHintIds = requiredCompletedHintIds;
		}
	}
}