using System;
using RR.Game.TutorialSystemV2.Abstraction;

namespace RR.Game.TutorialSystemV2.Realizations
{
	public class TutorialAdapter : IDisposable
	{
		public static ITutorialApplication Application { get; private set; }
		public static ITutorialTextFormatter TextFormatter { get; private set; }
		public static ITutorialEntitiesRepository EntitiesRepository { get; private set; }
		public static ITutorialProgressRepository ProgressRepository { get; private set; }
		public static ITutorialHandlersRepository HandlersRepository { get; private set; }
		public static ITutorialCameraProvider CameraProvider { get; private set; }

		public TutorialAdapter(
			ITutorialApplication application,
			ITutorialTextFormatter textFormatter,
			ITutorialEntitiesRepository entitiesRepository,
			ITutorialProgressRepository progressRepository,
			ITutorialHandlersRepository handlersRepository,
			ITutorialCameraProvider cameraProvider)
		{
			Application = application;
			TextFormatter = textFormatter;
			EntitiesRepository = entitiesRepository;
			ProgressRepository = progressRepository;
			HandlersRepository = handlersRepository;
			CameraProvider = cameraProvider;
		}

		public void Dispose()
		{
			Application = null;
			TextFormatter = null;
			EntitiesRepository = null;
			ProgressRepository = null;
			HandlersRepository = null;
		}
	}
}