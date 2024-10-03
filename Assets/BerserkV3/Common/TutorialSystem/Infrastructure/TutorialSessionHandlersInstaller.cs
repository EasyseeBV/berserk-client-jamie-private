using System;
using System.Collections.Generic;
using BerserkV3.Common.TutorialSystem.SessionHandlers;
using RR.Core.Extensions;
using RR.Game.TutorialSystemV2.Abstraction;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;
using Zenject;

namespace BerserkV3.Common.TutorialSystem
{
    public class TutorialSessionHandlersInstaller : IDisposable
    {
        private readonly ITutorialHandlersRepository handlersRepository;
        private readonly IList<ITutorialHandler> handlers;
        
        public TutorialSessionHandlersInstaller(
            IInstantiator instantiator, 
            ITutorialHandlersRepository handlersRepository)
        {
            this.handlersRepository = handlersRepository;
            handlers = new List<ITutorialHandler>
            {
                instantiator.Instantiate<TutorialBotContinueHandler>(),
                instantiator.Instantiate<TutorialEffectPlayConditionHandler>(),
                instantiator.Instantiate<TutorialEffectAddConditionHandler>(),
                instantiator.Instantiate<TutorialHoverCardHandler>(),
                instantiator.Instantiate<TutorialMulliganReplaceCardsHandler>(),
                instantiator.Instantiate<TutorialPreviewCardHandler>(),
                instantiator.Instantiate<TutorialReadyToMulliganHandler>(),
                instantiator.Instantiate<TutorialMulliganFinishHandler>(),
                instantiator.Instantiate<TutorialSessionEndHandler>(),
                instantiator.Instantiate<TutorialReceiveEndGameHandler>(),
                instantiator.Instantiate<TutorialCloseWhenCommandExecuteConditionHandler>(),
                instantiator.Instantiate<TutorialLockCardsConditionHandler>(),
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