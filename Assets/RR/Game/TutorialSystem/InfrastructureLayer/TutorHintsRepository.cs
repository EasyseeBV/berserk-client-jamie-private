using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.Utilities;
using RR.Game.TutorialSystem.Domain;
using RR.Game.TutorialSystem.Domain.Data;

namespace RR.Game.TutorialSystem.InfrastructureLayer
{
	public class TutorHintsRepository : ITutorHintsRepository
	{
		private List<TutorHintEntity> hints = new();
		private ProgressData cachedProgressData;
		private TutorData tutorData;

		private string ProgressDataPath => "TutorProgressData";
		private string TutorConfigDataPath => "TutorData";

		public TutorHintsRepository()
		{
			Init();
		}

		private void Init()
		{
			SetTutorData(RRFile.LoadFromTextAsset<TutorData>(TutorConfigDataPath));
			SetTutorProgress(RRFile.Load<ProgressData>(ProgressDataPath));
		}

		public List<TutorHintEntity> GetHintsReadyForInvoke(string hintPlace)
		{
			return hints.Where(hint => hint.PlaceUIName.Same(hintPlace)
			                           && !hint.IsCompleted
			                           && hint.RequiredCompletedHintIds.All(hintId => hints
				                           .Find(requiredCompletedHint => requiredCompletedHint.Id == hintId)
				                           .IsCompleted))
				.ToList();
		}

		public List<TutorHintEntity> GetHints()
		{
			return hints;
		}

		public TutorHintEntity Find(string id)
		{
			return hints.Find(hint => hint.Id.Same(id));
		}

		public IEnumerable<TutorHintEntity> FindAll(params string[] hintsId)
		{
			if (hintsId == null || hintsId.Length == 0)
				return hints;

			return hints.FindAll(hint => hintsId
				.Any(x => x.Same(hint.Id)));
		}

		public void SaveChanges()
		{
			RRFile.Save(ProgressDataPath, cachedProgressData);
		}

		public void SaveChanges(TutorHintEntity hint)
		{
			if (!cachedProgressData.CompletedTutorHints.ContainsKey(hint.Id))
			{
				RRLogger.Error("Save failed. Tutor hint not found");
				return;
			}

			cachedProgressData.CompletedTutorHints[hint.Id] = hint.IsCompleted;
			SaveChanges();
		}

		public void ResetProgress()
		{
			RRFile.Delete(ProgressDataPath);
			SetTutorProgress(null);
		}

		public void SetTutorData(TutorData tutorData)
		{
			if (tutorData == null
			    || !tutorData.TutorHints.Any())
				throw new NullReferenceException($"{nameof(TutorData)} not found {TutorConfigDataPath}");

			this.tutorData = tutorData;
		}

		public void SetTutorProgress(ProgressData progressData)
		{
			AddHints();

			if (progressData == null || !progressData.CompletedTutorHints.Any())
				progressData = CreateNewProgress();

			progressData.CompletedTutorHints.Keys
				.Where(key => progressData.CompletedTutorHints[key])
				.ForEach(key => hints.Find(x => x.Id == key)?.MarkCompleted());
			cachedProgressData = progressData;

			void AddHints()
			{
				hints.Clear();
				hints.AddRange(tutorData.TutorHints.Select(hintData => new TutorHintEntity(hintData, false)));

				CheckUniqueId();

				void CheckUniqueId()
				{
					var set = new HashSet<string>();
					var allIdUnique = hints.All(hint => set.Add(hint.Id));
					if (!allIdUnique)
						RRLogger.Error("Duplicate hints found in the repository. Correct the config");
				}
			}
		}

		private ProgressData CreateNewProgress()
		{
			AddProgress();
			SaveChanges();

			return cachedProgressData;

			void AddProgress()
			{
				cachedProgressData = new ProgressData();
				hints.ForEach(hint => cachedProgressData.CompletedTutorHints.Add(hint.Id, false));
			}
		}
	}
}