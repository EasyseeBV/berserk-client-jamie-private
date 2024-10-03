using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BerserkV3.Startup.Events;
using Cysharp.Threading.Tasks;
using GameCore;
using UnityEngine;

namespace Lobby.Dialogs
{
	public class LoadingView : MonoBehaviour
	{
		[SerializeField] private CanvasGroup canvasGroup = default;
		private readonly Queue<UniTask> processes = new();
		private bool processing;
		
		private void Awake()
		{
			StartupBus.AddLoadingTask.Subscribe(this, AddLoadingTask);
			StartupBus.AddLoadingUnitask.Subscribe(this, AddLoadingTask);
			SetVisible(false);
		}

		private void AddLoadingTask(Task task)
		{
			if (task != null)
				processes.Enqueue(task.AsUniTask());
			
			if (!processing)
				ProcessTasks().Forget();
		}

		private void AddLoadingTask(UniTask task)
		{
			processes.Enqueue(task); // WhenAll need to make another continuation
			if (!processing)
				ProcessTasks().Forget();
		}
		
		private async UniTask ProcessTasks()
		{
			SetVisible(true);
			while (processes.Count > 0 && Application.isPlaying)
			{
				try
				{
					var task = processes.Dequeue();
					while (task.Status == UniTaskStatus.Pending && Application.isPlaying)
						await UniTask.Yield();
				}
				catch
				{
					// ignored
				}
			}

			if (processes.Count > 0 && Application.isPlaying)
			{
				ProcessTasks().Forget();
				return;
			}
			
			SetVisible(false);
		}

		private void SetVisible(bool value)
		{
			processing = value;
			canvasGroup.alpha = value ? 1 : 0;
			canvasGroup.blocksRaycasts = value;
		}
	}
}