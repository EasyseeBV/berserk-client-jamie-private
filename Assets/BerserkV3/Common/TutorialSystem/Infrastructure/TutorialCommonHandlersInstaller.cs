using System;
using System.Collections.Generic;
using BerserkV3.Common.TutorialSystem.CommonHandlers;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using Zenject;

namespace BerserkV3.Common.TutorialSystem
{
    public class TutorialCommonHandlersInstaller : IDisposable
    {
        private readonly ITutorialHandlersRepository handlersRepository;
        private readonly IList<ITutorialHandler> handlers;

        public TutorialCommonHandlersInstaller(
            IInstantiator instantiator,
            ITutorialHandlersRepository handlersRepository)
        {
            this.handlersRepository = handlersRepository;
            handlers = new List<ITutorialHandler>
            {
                instantiator.Instantiate<TutorialPopupHandler>(),
                instantiator.Instantiate<TutorialUnmaskHandler>(),
                instantiator.Instantiate<TutorialRaycastConditionHandler>(),
                instantiator.Instantiate<TutorialDelyedCloseConditionHandler>(),
                instantiator.Instantiate<TutorialAnalyticsHandler>()
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