using System.Collections.Generic;
using RR.Game.TutorialSystem.Domain.Data;

namespace RR.Game.TutorialSystem.Domain
{
	public interface ITutorHintsRepository
	{
		List<TutorHintEntity> GetHintsReadyForInvoke(string hintPlace);

		List<TutorHintEntity> GetHints();

		TutorHintEntity Find(string id);

		IEnumerable<TutorHintEntity> FindAll(params string[] dependentHintsIdForCompletion);

		void SaveChanges(TutorHintEntity hint);

		void ResetProgress();

		void SetTutorProgress(ProgressData progressData);
		void SetTutorData(TutorData tutorData);
	}
}