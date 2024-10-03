using System;
using BerserkV3.GameCore.Network;
using BerserkV3.Lobby.Network;
using BerserkV3.Startup.Network;
using RR.Core.DebugSystem;
using RR.Core.EventLayer;
using RR.Core.Extensions;
using UI;
using UnityEngine;
using Vulcan.Network.Resolver;
using Object = UnityEngine.Object;

namespace Events
{
	public class ErrorDispatcherDummy : MonoBehaviour
	{
	}

	[Obsolete ("Handle your errors trough your controllers.")]
	public class ErrorDispatcher : EventBus
	{
		private const int SERVER_MAINTENANCE_CODE = 503;
		private const int UNAUTHORIZED_CODE = 401;
		
		// this event used in internal logic to show warrnings 
		public static RREvent<string> OnInternalWarning;
		// this event used to catch all API errors with code not equal 2** or 3**
		public static RREvent<int, string> OnError;
		// this event used to catch exceptions while running API requests
		public static RREvent<Exception> OnException;

		private static ErrorDispatcherDummy dummy;
		private static bool canHandleGameErrors;

		static ErrorDispatcher()
		{
			InitFields<ErrorDispatcher>();

			dummy = new GameObject("ErrorDispatcherDummy").AddComponent<ErrorDispatcherDummy>();
			Object.DontDestroyOnLoad(dummy.gameObject);

			OnInternalWarning.Subscribe(dummy, HandleWarning);
			
			GameBus.OnContextUpdated.Subscribe(dummy, () => canHandleGameErrors = true);
			GameBus.OnTimerPaused.Subscribe(dummy, value => canHandleGameErrors = !value);

			GameAPI.OnException += HandleException;
			GameAPI.OnFail += HandleGameError;
		}
		
		private static void HandleException(Exception e)
		{
			OnException += e;
		}

		private static void HandleLobbyError(int code, string message)
		{
			if (code > 499)
			{
				HandleError(code, message);
				return;
			}

			HandleWarning(message);
		}

		private static void HandleGameError(int code, string message)
		{
			if (!canHandleGameErrors || GameBus.LocalContext.GameEnded)
				return;
			
			HandleError(code, message);
		}
		
		private static void HandleError(int code, string message)
		{
			switch (code)
			{
				case SERVER_MAINTENANCE_CODE:
					OnError.Publish(503, "Server under Maintenance.");

					break;
				
				case UNAUTHORIZED_CODE:
					break;

				default:
					CallbackByMessage(code, message);

					break;
			}
			
			RRLogger.Error($"[{"ErrorDispatcher".Bold().Red()}] {code} - {message}");
		}

		private static void CallbackByMessage(int code, string message)
		{
			if (message.EndsWith("token expired"))
				return;

			if (message.EndsWith("User doesn't exist"))
				return;

			if (string.IsNullOrEmpty(message))
			{
				OnError.Publish(code, "Something went wrong. Try again please.");
				return;
			}

			OnError.Publish(code, message);
		}

		private static void HandleWarning(string message)
		{
			ConfirmationDialog.Instance.Init()
				.SetMessage(message)
				.SetTitle("Information")
				.SetCancel()
				.Apply();
		}
	}
}