using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.UIKit;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.UI;
using BerserkV3.Lobby.Vulcanite.Abstractions;
using BerserkV3.Startup.Abstractions;
using BerserkV3.Startup.Authorization;
using BerserkV3.Startup.Authorization.Inventory.Models;
using Cysharp.Threading.Tasks;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using Object = UnityEngine.Object;

namespace BerserkV3.Startup.Applications
{
	public class FirstVulcaniteApplication : IFirstVulcaniteApplication
	{
		private readonly IInventoryApplication userInventory;
		private readonly IGameDatabase gameDatabase;
		private readonly ICustomisationItemRepository customisationRepository;
		private readonly IVulcaniteApplication vulcaniteApplication;
		private UniTaskCompletionSource<bool> completion;
		private CancellationTokenSource lifeTime;
		
		private static SelectFirstVulcaniteView Window => SelectFirstVulcaniteView.Instance;

		public FirstVulcaniteApplication(
			IGameDatabase gameDatabase,
			ICustomisationItemRepository customisationRepository,
			IVulcaniteApplication vulcaniteApplication, 
			IInventoryApplication userInventory)
		{
			this.gameDatabase = gameDatabase;
			this.customisationRepository = customisationRepository;
			this.vulcaniteApplication = vulcaniteApplication;
			this.userInventory = userInventory;
		}
		
		public void Dispose()
		{
			lifeTime?.Cancel();
			lifeTime?.Dispose();
			lifeTime = null;
			completion?.TrySetResult(false);
			completion = null;
		}

		public async UniTask<bool> SelectFirstVulcanteAsync()
		{
			try
			{
				if (vulcaniteApplication.Owned.Any(x => gameDatabase.GetHero(x.VulcaniteId).LevelAtRegistration == 1))
					return false;

				lifeTime?.Cancel();
				lifeTime?.Dispose();
				lifeTime = new CancellationTokenSource();
				completion = new UniTaskCompletionSource<bool>();
				await SetupAsync(lifeTime.Token);
				return await completion.Task;
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
				return false;
			}
			finally
			{
				await CloseWindowAsync(Window);
			}
		}

		private async UniTask SetupAsync(CancellationToken token)
		{
			Window.Clear();
			var maskArtUrl = "Vulcanite_Mask";
			var itemViews = new List<HexagoneItemView>();
			string selectedId = null;

			var customItem = customisationRepository.GetFirstEquipped(CustomisationType.AvatarFrame);
			var loadTasks = gameDatabase.AllHeroes().Where(x => x.LevelAtRegistration == 1).Select(async heroData =>
			{
				selectedId ??= heroData.Id;
				var hexView = Object.Instantiate(Window.HexagoneItemView, Window.HexagoneLayout.RectTransform);
				await hexView.InitAsync(heroData.Id, heroData.ArtUrl, customItem.PreviewURL, maskArtUrl, token);
				token.ThrowIfCancellationRequested();
				
				itemViews.Add(hexView);
				hexView.SetActiveLock(false);
				hexView.SetActiveBottomText(false);
				hexView.Show();
				hexView.OnClicked += _ =>
				{
					selectedId = heroData.Id;
					itemViews.ForEach(x => x.Select(x.Id == selectedId));
					OnHeroSelected(heroData);
				};
				
				if (heroData.Id == selectedId)
				{
					hexView.Select(heroData.Id == selectedId);
					OnHeroSelected(heroData);
				}
			});

			await UniTask.WhenAll(loadTasks).AttachExternalCancellation(token);
			Window.SetAcceptAction(() => AcceptAsync(selectedId).Forget(DefaultSharedLogger.Error));
			Window.HexagoneLayout.RefreshLayout();
			Window.Show();
		}

		private void OnHeroSelected(IHeroData heroData)
		{
			if (string.IsNullOrWhiteSpace(heroData?.Name))
			{
				DefaultSharedLogger.Error($"[{GetType().Name.Orange()}] Selected Hero with config id '{heroData?.Id}' doesn't exist.");
				return;
			}

			Window.SetHeaderText(heroData.Name.Replace(",", "\n<size=50%>"));
		}
		
		private async UniTask CloseWindowAsync(BaseView value)
		{
			if (value && value.VisibleState == VisibleState.Visible)
			{
				var tcs = new UniTaskCompletionSource();
				value.Close(() => tcs.TrySetResult());
				await tcs.Task;
			}
		}
		
		private async UniTask AcceptAsync(string selectedId)
		{
			try
			{
				await vulcaniteApplication.GiveVulcanite(selectedId);
				completion?.TrySetResult(true);
				completion = null;
			}
			finally
			{
				Dispose();
			}
		}
	}
}