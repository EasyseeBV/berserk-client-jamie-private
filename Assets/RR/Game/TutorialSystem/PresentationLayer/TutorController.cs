using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RR.Core.Components;
using RR.Core.DebugSystem;
using RR.Game.TutorialSystem.Application;
using RR.Game.TutorialSystem.Domain;
using RR.Game.TutorialSystem.Domain.Data;
using RR.Game.TutorialSystem.Event;
using RR.Game.TutorialSystem.InfrastructureLayer;
using UnityEngine;

namespace RR.Game.TutorialSystem.Presentation
{
	public abstract class TutorController : Singleton<TutorController>, IDisposable
	{
		protected ITutorHintsRepository tutorHintsRepository;
		private TutorService tutorService;

		public virtual void Dispose()
		{
			TutorBus.OnPlaceVisited.UnsubscribeAll();
			TutorBus.OnHintInvoked.Unsubscribe(PauseGame);
			TutorBus.OnHintClosed.Unsubscribe(UnpauseGame);
		}

		protected override void OnAwake()
		{
			try
			{
				tutorHintsRepository = new TutorHintsRepository();
				tutorService = new TutorService(tutorHintsRepository);
				tutorService.OnSyncRequired += SendProgressToServer;
			}
			catch (Exception ex)
			{
				RRLogger.Error(ex);
			}
		}

		public virtual async Task SetUpAsync()
		{
			try
			{
				var tutorData = await GetConfigFromServerAsync();
				if (tutorData != null)
					tutorHintsRepository.SetTutorData(tutorData);

				var progressData = await GetProgressFromServerAsync();
				if (progressData == null && IsCompleted())
				{
					RRLogger.Log("Local loading of the progress of the tutorial");
					Dispose();
					return;
				}

				//If progressData == null -> Create new local progress
				Fetch(progressData);
				if (IsCompleted())
					Dispose();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				if (IsCompleted())
				{
					Dispose();
					throw;
				}

				RRLogger.Error("Local loading of the progress of the tutorial");
			}
		}

		public void StartTutor()
		{
			//TODO: Add Tutor Implementation Without Singleton

			if (IsCompleted())
				return;

			Dispose();
			SubscribeToTransitions();
			SubscribeToChainHintsCalls();
		}

		private void SubscribeToChainHintsCalls()
		{
			TutorBus.OnHintInvoked.Subscribe(Instance, tutorHint => tutorHint.RequiredPause, PauseGame);
			TutorBus.OnHintClosed.Subscribe(Instance, tutorHint => tutorHint.RequiredPause, UnpauseGame);
		}

		public static bool CanAddTutorElement(string hintId)
		{
			return !Instance.tutorService.CheckHint(hintId);
		}

		public static void CloseHintManually()
		{
			Instance.tutorService.CloseHint();
		}

		public static void Fetch(ProgressData progressData)
		{
			Instance.tutorService.ResetLocalProgress();
			Instance.tutorService.LoadFrom(progressData);
		}

		public static void Restart()
		{
			Instance.tutorService.Restart();
		}

		public static void ResetLocalProgress()
		{
			Instance.tutorService.ResetLocalProgress();
		}

		protected static void Skip()
		{
			Instance.tutorService.MarkAllCompleted();
		}

		protected static bool IsCompleted()
		{
			return Instance.tutorService.IsCompleted();
		}

		/// <summary>
		///     Subscribe to windows opening events,
		///     Use <code> TryInvokeStarterHint(Place place); </code> when opening a window.
		///     <code> MenuView.Instance.Shown += () => TryInvokeStarterHint(Place.MainMenu.ToString()); </code>
		///     <code> TutorBus.OnPlaceVisited.Subscribe(Instance, TryInvokeStarterHint); </code>
		/// </summary>
		protected abstract void SubscribeToTransitions();

		protected void TryInvokeStarterHint(string placeName)
		{
			Instance.tutorService.InvokeHintByPlace(placeName);
		}

		protected virtual void PauseGame(TutorHintEntity obj)
		{
			Time.timeScale = 0f;
		}

		protected virtual void UnpauseGame(TutorHintEntity obj)
		{
			Time.timeScale = 1f;
		}

		protected virtual void SendProgressToServer(Dictionary<string, bool> completedTutorHints)
		{
		}

		protected abstract Task<ProgressData> GetProgressFromServerAsync();

		protected abstract Task<TutorData> GetConfigFromServerAsync();
	}
}