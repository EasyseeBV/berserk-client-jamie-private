using NUnit.Framework;
using RR.Game.TutorialSystem.Domain;
using RR.Game.TutorialSystem.Domain.Data;

namespace RR.Game.Tests
{
	public class TutorialSystemTest
	{
		[Test]
		public void CheckAllowedToInvokeWithoutCondition()
		{
			var nullConditionHint = new TutorHintEntity(new TutorHintData
			{
				Id = "test",
				NextTutorHintId = "test2",
				DependentHintsIdForCompletion = new[] { "" },
				DelayAfterInvoke = 0,
				DelayBeforeInvoke = 0,
				PlaceUIName = "testUI",
				RequiredPause = false,
				Text = "infoText"
			}, false);

			Assert.AreEqual(true, nullConditionHint.IsAllowedToInvoke());

			//TODO: Add a case after the conditions are implemented
			//var conditionsCompletedHint = new TutorHintEntity(new TutorHintData()
			//{
			//	Id = "test2",
			//	NextTutorHintId = "",
			//	DependentHintsIdForCompletion = new string[] { "test", "test2" },
			//	DelayAfterInvoke = 0,
			//	DelayBeforeInvoke = 0,
			//	PlaceUIName = "",
			//	Conditions = new List<ConditionData>()
			//	{
			//		new ConditionData()
			//		{
			//			RequiredCompletedHintIds = 1
			//		}
			//	},
			//	RequiredPause = false,
			//	Text = "infoText2"
			//}, false);

			//Assert.AreEqual(true, conditionsCompletedHint.IsAllowedToInvoke());

			var conditionsNotCompletedHint = new TutorHintEntity(new TutorHintData
			{
				Id = "test2",
				NextTutorHintId = "",
				DependentHintsIdForCompletion = new[] { "test", "test2" },
				DelayAfterInvoke = 0,
				DelayBeforeInvoke = 0,
				PlaceUIName = "",
				RequiredPause = false,
				Text = "infoText2"
			}, false);

			Assert.AreEqual(false, conditionsNotCompletedHint.IsAllowedToInvoke());
		}
	}
}