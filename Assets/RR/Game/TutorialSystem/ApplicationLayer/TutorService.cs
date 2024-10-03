using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystem.Domain;
using RR.Game.TutorialSystem.Domain.Data;
using RR.Game.TutorialSystem.Event;

namespace RR.Game.TutorialSystem.Application
{
	public class TutorService
	{
		public event Action<Dictionary<string, bool>> OnSyncRequired;

		private readonly ITutorHintsRepository tutorHintsRepository;

		public TutorService(ITutorHintsRepository tutorHintsRepository)
		{
			this.tutorHintsRepository = tutorHintsRepository;
		}

		public void InvokeHintByPlace(string hintPlace)
		{
			var hints = tutorHintsRepository.GetHintsReadyForInvoke(hintPlace);
			hints.ForEach(hint => hint.DelayedInvoke());
		}

		public void CloseHint()
		{
			var hint = TutorBus.OnHintInvoked.Value;
			hint.Close();

			CompleteHint();
			SyncToServer();
			GoToNextHint();

			void CompleteHint()
			{
				var hints = tutorHintsRepository.FindAll(hint.Id).ToArray();

				if (hints.Length > 1)
					RRLogger.Error("There are more than one hints with id: " + hint.Id);

				hints.ForEach(dependentHint =>
				{
					dependentHint.MarkCompleted();
					tutorHintsRepository.SaveChanges(dependentHint);
				});
			}
			void SyncToServer()
			{
				if (hint.IsNeedSyncToServer)
				{
					var hints = tutorHintsRepository.GetHints();
					OnSyncRequired?.Invoke(hints.ToDictionary(x => x.Id, y => y.IsCompleted));
				}
			}
			void GoToNextHint()
			{
				var nextHint = tutorHintsRepository.Find(hint.NextTutorHintId);
				nextHint?.DelayedInvoke();
			}
		}

		public void MarkAllCompleted()
		{
			var hints = tutorHintsRepository
				.FindAll()
				.ToArray();

			hints.ForEach(hint =>
			{
				hint.MarkCompleted();
				tutorHintsRepository.SaveChanges(hint);
			});

			OnSyncRequired?.Invoke(hints.ToDictionary(x => x.Id, y => y.IsCompleted));
		}

		public bool CheckHint(string hintId)
		{
			var hint = tutorHintsRepository.Find(hintId);
			if (hint == null)
				throw new NullReferenceException(
					$"{nameof(hint)} not found in {nameof(tutorHintsRepository)} by id {hintId}");

			return tutorHintsRepository.Find(hintId).IsCompleted;
		}

		public bool IsCompleted()
		{
			return tutorHintsRepository.GetHints().All(hint => hint.IsCompleted);
		}

		public void ResetLocalProgress()
		{
			tutorHintsRepository.ResetProgress();
		}

		public void Restart()
		{
			ResetLocalProgress();
			var hints = tutorHintsRepository
				.FindAll()
				.ToArray();
			OnSyncRequired?.Invoke(hints.ToDictionary(x => x.Id, y => y.IsCompleted));
		}

		public void LoadFrom(ProgressData progressData)
		{
			tutorHintsRepository.SetTutorProgress(progressData);
		}
	}
}