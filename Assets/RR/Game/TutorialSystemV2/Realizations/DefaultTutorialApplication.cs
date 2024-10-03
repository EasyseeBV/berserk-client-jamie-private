using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace RR.Game.TutorialSystemV2.Realizations
{

	public class DefaultTutorialApplication : ITutorialApplication, IDisposable
	{
		protected ITutorialEntitiesRepository EntitiesRepository { get; }
		protected ITutorialProgressRepository ProgressRepository { get; }
		protected ITutorialHandlersRepository HandlersRepository { get; }
		protected IList<ITutorialHintEntity> Active { get; }
		protected bool Initialized;

		public DefaultTutorialApplication(
			ITutorialEntitiesRepository entitiesRepository,
			ITutorialProgressRepository progressRepository,
			ITutorialHandlersRepository handlersRepository)
		{
			EntitiesRepository = entitiesRepository;
			ProgressRepository = progressRepository;
			HandlersRepository = handlersRepository;
			Active = new List<ITutorialHintEntity>();
		}

		public virtual void Dispose()
		{
			if (!Initialized)
				return;

			ResetAsync().Forget();
		}

		public virtual async Task ResetAsync()
		{
			Initialized = false;
			var temp = Active.ToArray();
			Active.Clear();
			
			foreach (var hint in temp)
				await HandleCloseAsync(hint);
			
			EntitiesRepository.ResetAll();
			ProgressRepository.ResetAll();
		}

		public virtual async Task InitAsync()
		{
			if (Initialized)
				return;
			
			Initialized = true;
			Active.Clear();
			await EntitiesRepository.InitAsync();
			await ProgressRepository.InitAsync(EntitiesRepository.GetAll().Select(x => x.Id));
			
			if (!ProgressRepository.IsCompleted())
				return;
			
			EntitiesRepository.ResetAll();
		}

		public virtual async Task InvokeAsync(string id)
		{
			if (!Initialized || string.IsNullOrEmpty(id) || ProgressRepository.IsCompleted(id))
				return;

			if (!TryGetPossibleHint(id, out var hint))
				return;

			if (hint.Data.DelayBeforeInvoke > 0)
				await Task.Delay(TimeSpan.FromSeconds(hint.Data.DelayBeforeInvoke));

			Pause(hint.Data.PauseRequired);

			if (!Active.Contains(hint))
				Active.Add(hint);

			await HandleInvokeAsync(hint);
		}

		public virtual async Task CloseAsync()
		{
			if (!Initialized || Active.Count == 0)
				return;

			var closeHints = Active.ToList();
			foreach (var hint in closeHints)
			{
				ProgressRepository.SetCompleted(hint.Id);
				hint.Data.CompleteDependedHints.ForEach(ProgressRepository.SetCompleted);
				await HandleCloseAsync(hint);
			}

			if (closeHints.Any(x => x.Data.IsSyncRequired))
				ProgressRepository.SaveAsync().Forget(); // It is not necessary to wait for synchronization with saving.

			var lastHint = closeHints.Last();
			if (lastHint.Data.DelayAfterClose > 0)
				await Task.Delay(TimeSpan.FromSeconds(lastHint.Data.DelayAfterClose));

			// instead List.Clear used this, if while close handling any hint added.
			closeHints.ForEach(h => Active.Remove(h));
			RRLogger.Log($"[{GetType().Name.Orange()}] Next hint : {lastHint.Data.NextId}");
			await InvokeAsync(lastHint.Data.NextId);
		}

		public virtual async Task RestartAsync()
		{
			Active.Clear();
			Initialized = false;
			ProgressRepository.ResetAll();
			await ProgressRepository.SaveAsync();
			await InitAsync();
		}

		public virtual async Task SkipAsync()
		{
			if (!Initialized || ProgressRepository.IsCompleted())
				return;

			ProgressRepository.CompleteAll();
			await ProgressRepository.SaveAsync();

			foreach (var hint in Active.ToArray())
				await HandleCloseAsync(hint);

			Active.Clear();
		}

		public virtual ITutorialHintEntity[] GetActive()
		{
			return Active.ToArray();
		}

		protected virtual async Task HandleCloseAsync(ITutorialHintEntity hint)
		{
			if (hint == null)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] While handle close, hint is missing.");
				return;
			}

			RRLogger.Log($"[{GetType().Name.Orange()}] Close : {hint.Id}, Possible Handlers : {string.Join(",", hint.TriggerIds)}");
			foreach (var handler in HandlersRepository.Get<ITutorialCloseHandler>(true, hint.TriggerIds))
			{
				RRLogger.Log($"[{GetType().Name.Orange()}] Close Handler : {handler}");
				try
				{
					await handler.CloseAsync(hint);
				}
				catch (Exception e)
				{
					RRLogger.Error($"[{GetType().Name.Orange()}] While Close Handler : {handler}, throw an exception : {e}");
				}
			}
		}

		protected virtual async Task HandleInvokeAsync(ITutorialHintEntity hint)
		{
			if (hint == null)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] While handle invoke, hint is missing.");
				return;
			}

			RRLogger.Log($"[{GetType().Name.Orange()}] Invoke : {hint.Id}, Possible Handlers : {string.Join(",", hint.TriggerIds)}");
			foreach (var handler in HandlersRepository.Get<ITutorialInvokeHandler>(true, hint.TriggerIds))
			{
				RRLogger.Log($"[{GetType().Name.Orange()}] Invoke Handler : {handler}");
				try
				{
					await handler.InvokeAsync(hint);
				}
				catch (Exception e)
				{
					RRLogger.Error($"[{GetType().Name.Orange()}] While Invoke Handler : {handler}, throw an exception : {e}");
				}
			}
		}

		protected virtual bool TryGetPossibleHint(string id, out ITutorialHintEntity result)
		{
			result = EntitiesRepository.GetAll().FirstOrDefault(hint =>
				!ProgressRepository.IsCompleted(hint.Id)
				&& (hint.Id == id || hint.TriggerIds.Contains(id)
					&& hint.Data.RequiredCompletedHints.All(ProgressRepository.IsCompleted)));

			return result != null;
		}

		protected virtual void Pause(bool value) {}
	}

}