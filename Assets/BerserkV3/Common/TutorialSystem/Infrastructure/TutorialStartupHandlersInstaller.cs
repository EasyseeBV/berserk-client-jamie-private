using System;
using System.Collections.Generic;
using BerserkV3.Common.TutorialSystem.CommonHandlers;
using BerserkV3.Common.TutorialSystem.StartupHandlers;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using Zenject;

namespace BerserkV3.Common.TutorialSystem
{
	public class TutorialStartupHandlersInstaller : IDisposable
	{
		private readonly ITutorialHandlersRepository handlersRepository;
		private readonly IList<ITutorialHandler> handlers;

		public TutorialStartupHandlersInstaller(
			IInstantiator instantiator,
			ITutorialHandlersRepository handlersRepository)
		{
			this.handlersRepository = handlersRepository;
			handlers = new List<ITutorialHandler>
			{
				instantiator.Instantiate<TutorialWelcomeHandler>(),
				instantiator.Instantiate<TutorialSessionStartHandler>(),
				instantiator.Instantiate<TutorialFirstVulcaniteHandler>()
			};
			handlers.ForEach(handlersRepository.Register);
		}

		public void Dispose()
		{
			handlers.ForEach(handlersRepository.UnRegister);
			handlers.Clear();
		}
	}
}