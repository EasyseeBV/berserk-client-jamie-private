using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;

namespace BerserkV3.Common.ProgressDrawer
{
	public class ProgressDrawerApplication : DisposableWithCts, IProgressDrawer
	{
		private readonly IProgressDrawerView view;
		private readonly List<CustomProgress> tasks;

		public ProgressDrawerApplication(IProgressDrawerView view)
		{
			this.view = view;
			tasks = new List<CustomProgress>();
		}
		
		public override void Dispose()
		{
			base.Dispose();
			tasks.ForEach(x => x?.Dispose());
			tasks.Clear();
		}

		public async UniTask SetProgressTypeAsync(ProgressType value)
		{
			await CloseAsync(true);
			Reset();
			view.SetProgressType(value);
		}

		public void AddProgress(Progress<float> other = null)
		{
			CreateProgress(other);
		}

		public IProgress<float> CreateProgress(Progress<float> other = null)
		{
			if (tasks.Count <= 0)
				Reset();
			
			var task = new CustomProgress(other);
			task.OnProgressChanged += OnProgressChanged;
			tasks.Add(task);
			
			return task;
		}

		public void CancelProgress(IProgress<float> value)
		{
			if (value == null)
				return;
					
			if (value is not CustomProgress customProgress)
			{
				customProgress = tasks.FirstOrDefault(x => x.Other == value);
				if (customProgress == null)
				{
					DefaultSharedLogger.Error($"[{GetType().Name.Orange()}] Unknown progress : {value}");
					return;
				}
			}

			if (customProgress.Disposed || !tasks.Remove(customProgress))
			{
				DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] Progress already cancelled : {value}");
				return;
			}
			
			customProgress.Dispose();
			OnProgressChanged(customProgress);
		}

		public IEnumerable<IProgress<float>> GetExistProgress()
		{
			return tasks.ToArray();
		}

		public void SetProgressFormat(string value)
		{
			view?.ProgressBarLayout?.SetProgressFormat(value);
		}

		public async UniTask ShowAsync(params object[] args)
		{
			if (view?.ProgressBarLayout == null)
				return;
			
			await view.ShowAsync(args:args);
		}
		
		public async UniTask CloseAsync(bool force = false, params object[] args)
		{
			if (view == null)
				return;
			
			await view.CloseAsync(force, args:args);
			SetTooltip(string.Empty);
		}
		
		public void Reset()
		{
			tasks.ForEach(x => x?.Dispose());
			tasks.Clear();
			view?.ProgressBarLayout?.SetProgress(0f);
			SetProgressFormat(null);
			SetTooltip(null);
		}
		
		public void SetTooltip(string value)
		{
			view?.ProgressBarLayout?.SetActiveTooltip(!string.IsNullOrEmpty(value));
			view?.ProgressBarLayout?.SetTooltipText(value);
		}
		
		private void OnProgressChanged(CustomProgress sender)
		{
			var taskCopy = tasks.ToArray(); // to avoid changes when enumeration
			var totalProgress = taskCopy.Sum(x => x.Progress) / taskCopy.Length;
			view?.ProgressBarLayout?.SetProgress(totalProgress);

			if (sender is {Disposed: false, Progress: >= 1f})
				sender.Dispose();
			
			if (taskCopy.Length > 0 && totalProgress < 1f)
				return;

			CloseAsync().Forget();
			tasks.ForEach(x => x?.Dispose());
			tasks.Clear();
		}
	}
}