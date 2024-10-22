using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Identity.Social;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.SceneService;
using BerserkV3.Common.SerializedHelper;
using BerserkV3.Common.TutorialSystem;
using BerserkV3.Common.Utils;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.UI;
using BerserkV3.Startup.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UIService;
using UnityEngine.Device;

namespace BerserkV3.Startup.Applications
{
	public enum GuestAction
	{
		GuestDetected,
		TutorialEnd,
		GameEnded,
		LobbyLoaded,
		AfterSignUp,
	}

	public class GuestApplication : IGuestApplication
	{
		private readonly ISerializeHelper serializeHelper;
		private readonly IBerserkTutorialApplication tutorialApplication;
		private readonly ISceneService sceneService;
		private readonly IUIService uiService;
		private readonly ISharedConfig sharedConfig;
		private static string DeviceId => SystemInfo.deviceUniqueIdentifier;

		public GuestApplication(
			IUIService uiService,
			ISharedConfig sharedConfig,
			ISerializeHelper serializeHelper,
			IBerserkTutorialApplication tutorialApplication,
			ISceneService sceneService)
		{
			this.uiService = uiService;
			this.sharedConfig = sharedConfig;
			this.serializeHelper = serializeHelper;
			this.tutorialApplication = tutorialApplication;
			this.sceneService = sceneService;
		}

		public void Reset()
		{
			if (!serializeHelper.HasKey(DeviceId)) 
				return;
			
			serializeHelper.Delete(DeviceId);
			serializeHelper.Save();
		}

		public bool IsSameGuest(string id)
		{
			if (string.IsNullOrWhiteSpace(id) || serializeHelper.HasKey(DeviceId))
				return false;

			var guestLocalData = serializeHelper.Get(DeviceId).ParseCustomData();

			return guestLocalData.TryGetValue(nameof(GuestAction.GuestDetected), out var guestId)
			       && guestId == id;
		}
		
		public async UniTask<bool> ExecuteActionAsync(GuestAction action, bool ignorGuest = false)
		{
			if (!User.IsAuthorized)
			{
				DefaultSharedLogger.Error($"[{GetType().Name.Orange()}] {nameof(GuestAction)}: {action}, The user is not authorized.");
				return false;
			}

			if (!ignorGuest && !User.IsAnonymous)
			{
				DefaultSharedLogger.Log($"[{GetType().Name.Orange()}] {nameof(GuestAction)}: {action}, User isn't guest.");
				return false;
			}

			var guestLocalData = serializeHelper.HasKey(DeviceId) 
					? serializeHelper.Get(DeviceId).ParseCustomData() 
					: new Dictionary<string, string>();

			if (User.IsAnonymous)
				TrySaveGuestData(GuestAction.GuestDetected, User.Id, guestLocalData);
			
			return action switch
			{
				GuestAction.TutorialEnd  => await HandleTutorialEndActionAsunc(guestLocalData),
				GuestAction.AfterSignUp  => await HandleAfterSignUpActionAsunc(guestLocalData),
				GuestAction.GameEnded    => await HandleGameEndedActionAsync(guestLocalData),
				GuestAction.LobbyLoaded  => await HandleLobbyLoadedActionAsync(guestLocalData),
				_ => throw new NotImplementedException($"[{GetType().Name.Orange()}] Unknown {nameof(GuestAction)} : {action}")
			};
		}

		private async UniTask<bool> HandleTutorialEndActionAsunc(Dictionary<string, string> guestLocalData)
		{
			if (!TrySaveGuestData(GuestAction.TutorialEnd, "true", guestLocalData))
				return false;
			
			return await HandleSignUpActionAsync("Would you like to create your game account to automatically skip the tutorial next time around?");
		}
		
		private async UniTask<bool> HandleAfterSignUpActionAsunc(Dictionary<string, string> guestLocalData)
		{
			if (!guestLocalData.ContainsKey(nameof(GuestAction.GuestDetected))
			    || !TrySaveGuestData(GuestAction.AfterSignUp, "true", guestLocalData))
				return false;
			
			await tutorialApplication.SkipAsync();
			
			return true;
		}
		
		private UniTask<bool> HandleGameEndedActionAsync(Dictionary<string, string> guestLocalData)
		{
			if (!guestLocalData.ContainsKey(nameof(GuestAction.GuestDetected))
				|| TrySaveGuestData(GuestAction.GameEnded, "*", guestLocalData))
				return UniTask.FromResult(true);
			
			guestLocalData[nameof(GuestAction.GameEnded)] += "*";
			SaveGuestData(guestLocalData);
			return UniTask.FromResult(true);
		}

		private async UniTask<bool> HandleLobbyLoadedActionAsync(Dictionary<string, string> guestLocalData)
		{
			if (guestLocalData.TryGetValue(nameof(GuestAction.GameEnded), out var games))
			{
				var gameEndedKey = string.Join("_", GuestAction.LobbyLoaded, GuestAction.GameEnded, games.Length);
				if (games.Length == 1 && TrySaveGuestData(gameEndedKey, "true", guestLocalData))
					return await HandleSignUpActionAsync($"{"Your progress aren’t saved right now.".Bold()}<br>Would you like to create your game account to save and more?");
			}

			return false;
		}
		
		private async UniTask<bool> HandleSignUpActionAsync(string message)
		{
			var tcs = new UniTaskCompletionSource<bool>();
			uiService
				.Begin<AuthSignUpGuestWindow>()
				.WithInit(window =>
				{
					window.SetEmailInputText("");
					window.SetUserNameInputText("");
					window.SetPasswordInputText("");
					window.SetHeaderText(message);
					window.SetGuestButtonText("Continue as a guest");
					window.SetLoginButtonText($"Already have an account? {"Log in".CustomColor("#F55D0D")}");
					window.SetContinueButtonText("REGISTER");
					window.SetGuestAction(Close);
					window.SetCancelAction(Close);
					window.SetContinueAction(() =>
					{
						tcs.TrySetResult(true);
						ProceedToSignUpAsync().Forget();
					});
					window.SetLoginAction(() =>
					{
						tcs.TrySetResult(true);
						ProceedToSignInAsync().Forget();
					});

					var socials = sharedConfig.AvailableSocials.Where(x => x.IsPlatformAvailable()).ToArray();
					var isSocialAvailable = socials.IsSocialsAvailable();
					if (isSocialAvailable)
					{
						foreach (var provider in socials)
						{
							window.SocialWidget.CreateButton(provider).Subscribe(() =>
							{
								tcs.TrySetResult(true);
								return ProceedToSignUpSocialAsync(provider);
							});
						}
					}

					window.SocialWidget.SetActive(isSocialAvailable);
					window.SetActiveSeparator(isSocialAvailable);
					
					return;
					void Close() => uiService.Begin<AuthSignUpGuestWindow>().Hide(() => tcs.TrySetResult(false));
				}).Show();
			
			return await tcs.Task;
		}
		
		private async UniTask ProceedToSignInAsync()
		{
			await uiService
				.Begin<AuthSignUpGuestWindow>()
				.WithInit(window =>
				{
					User.AddRedirection(
						new AuthRedirectionArg(nameof(AuthSignInState)),
						new AuthSignInArgs
						{
							Email = window.GetEmail(),
							Password = window.GetPassword(),
							AutoSignIn = false,
						});
				}).HideAsync();

			await User.LogOutAsync().AddLoadingTask();
			await sceneService.LoadAsync(Scene.StartUp).AddLoadingTask();
		}

		private async UniTask ProceedToSignUpAsync()
		{
			await uiService
				.Begin<AuthSignUpGuestWindow>()
				.WithInit(window =>
				{
					User.AddRedirection(
						new AuthRedirectionArg(nameof(AuthSignUpState)),
						new AuthSignUpArgs
						{
							Email = window.GetEmail(),
							UserName = window.GetUserName(),
							Password = window.GetPassword(),
							AutoSignUp = !string.IsNullOrWhiteSpace(window.GetEmail())
							             && !string.IsNullOrWhiteSpace(window.GetUserName())
							             && !string.IsNullOrWhiteSpace(window.GetPassword())
						});
				}).HideAsync();
			
			await User.LogOutAsync().AddLoadingTask();
			await sceneService.LoadAsync(Scene.StartUp).AddLoadingTask();
		}

		private async UniTask ProceedToSignUpSocialAsync(ExternalProvider provider)
		{
			await uiService.Begin<AuthSignUpGuestWindow>().HideAsync();
			await User.LogOutAsync().AddLoadingTask();
			
			User.AddRedirection(
				new AuthRedirectionArg(nameof(AuthSocialSignInState)),
				new AuthSocialSignInArgs
				{
					ReturnState = nameof(AuthSignUpState),
					Provider = provider
				});
			
			await sceneService.LoadAsync(Scene.StartUp).AddLoadingTask();
		}
		
		private bool TrySaveGuestData(object key, object value, Dictionary<string, string> data)
		{
			if (!data.TryAdd(key.ToString(), value.ToString()))
				return false;

			SaveGuestData(data);
			return true;
		}

		private void SaveGuestData(Dictionary<string, string> data)
		{
			serializeHelper.Patch(DeviceId, data.ToCustomData());
			serializeHelper.Save();
		}
	}
}