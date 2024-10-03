using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BerserkV3.Common.AnalyticsSystem;
using BerserkV3.Lobby;
using BerserkV3.Startup.Authorization;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Realizations;
using Zenject;

namespace BerserkV3.Common.TutorialSystem
{

	public class BerserkTutorialApplication : DefaultTutorialApplication, IBerserkTutorialApplication
	{
		private readonly IInstantiator instantiator;
		private IDisposable commonHandlers;

		public BerserkTutorialApplication(
			IInstantiator instantiator,
			ITutorialEntitiesRepository entitiesRepository,
			ITutorialProgressRepository progressRepository,
			ITutorialHandlersRepository handlersRepository)
			: base(entitiesRepository, progressRepository, handlersRepository)
		{
			this.instantiator = instantiator;
		}

		public override async Task InitAsync()
		{
			if (Initialized)
				return;

			await base.InitAsync();

			if (!IsCompleted() && commonHandlers == null)
				commonHandlers = instantiator.Instantiate<TutorialCommonHandlersInstaller>();
		}
		
		public Task InvokeAsync(object id)
		{
			return base.InvokeAsync(id.ToString());
		}

		public bool IsCompleted()
		{
			return Initialized && ProgressRepository.IsCompleted();
		}

		public bool IsCompleted(object id, bool searchPossibleKeywords = false)
		{
			if (!Initialized || id == null)
				return false;

			if (!searchPossibleKeywords)
				return ProgressRepository.IsCompleted(id.ToString());

			return TryFindPossibleHints(id, out var hints) 
			       && hints.Any(entity => ProgressRepository.IsCompleted(entity.Id));
		}

		public override async Task SkipAsync()
		{
			if (!Initialized || ProgressRepository.IsCompleted())
				return;

			await base.SkipAsync();
			AnalyticsBus.SendCustomEvent.Publish(new TutorialSkippedModel(User.UserName));
			commonHandlers?.Dispose();
			commonHandlers = null;
		}

		public override void Dispose()
		{
			base.Dispose();
			commonHandlers?.Dispose();
			commonHandlers = null;
		}

		public override async Task ResetAsync()
		{
			await base.ResetAsync();
			commonHandlers?.Dispose();
			commonHandlers = null;
		}

		protected virtual bool TryFindPossibleHints(object id, out ITutorialHintEntity[] result)
		{
			result = Array.Empty<ITutorialHintEntity>();
			if (id == null)
				return false;
			
			var hintId = id.ToString();
			result = EntitiesRepository.GetAll()
				.Where(hint => hint.Id == hintId || hint.TriggerIds.Contains(hintId))
				.ToArray();
			
			return result.Length > 0;
		}
	}

}