using System.Linq;
using RR.Core.Extensions;
using Vulcan.Data;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;
using Berserk.Shared.Data.Customisation;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.PreviewSystem;
using BerserkV3.Common.UIKit;
using BerserkV3.Generic.Customisation;
using BerserkV3.Lobby.Applications;
using Cysharp.Threading.Tasks;
using Global;
using Lobby;
using RR.Core.ResourceManagament;
using UnityEngine.UI;
using HexagoneItemView = BerserkV3.Common.UIKit.HexagoneItemView;
using Object = UnityEngine.Object;

namespace UI
{
	public partial class DeckSelectVulcaniteDialog : View
	{
		[SerializeField] private HorizontalHexagoneLayout hexagoneLayout;

		private List<HexagoneItemView> itemViews;
		private OwnedVulcanite currentVulcanite;
		private Action<OwnedVulcanite> onAvatarSelected;
		private Quadrant currentLand;
		private List<Outline> flagOutlines;
		private List<RawImage> flagImages;
		private CancellationTokenSource loadingResources;
		private CancellationTokenSource render;
		
		protected override void OnAwake()
		{
			BackButton.Subscribe(Close);
			OKButton1.Subscribe(Select);

			Boreas.onClick.AddListener(() => SelectLand(Quadrant.Boreas));
			Arcadia.onClick.AddListener(() => SelectLand(Quadrant.Arcadia));
			Notus.onClick.AddListener(() => SelectLand(Quadrant.Notus));
			Hades.onClick.AddListener(() => SelectLand(Quadrant.Hades));

			Boreas.GetComponent<RawImage>().LoadResourceAsync($"{Quadrant.Boreas}_Flag").Forget();
			Arcadia.GetComponent<RawImage>().LoadResourceAsync($"{Quadrant.Arcadia}_Flag").Forget();
			Notus.GetComponent<RawImage>().LoadResourceAsync($"{Quadrant.Notus}_Flag").Forget();
			Hades.GetComponent<RawImage>().LoadResourceAsync($"{Quadrant.Hades}_Flag").Forget();

			var flags = new List<Button> { Boreas, Arcadia, Notus, Hades };
			flagOutlines = flags.Select(f => f.GetComponent<Outline>()).ToList();
			flagImages = flags.Select(f => f.GetComponent<RawImage>()).ToList();
		}

		public void InitAndShow(string vulcaniteId, Action<OwnedVulcanite> onAvatarSelected)
		{
			this.onAvatarSelected = onAvatarSelected;
			LobbyBus.OnUserDataRefreshed.SubscribeRaw(OnUserDataRefreshed);
			var vulcaniteData = GameDataBaseAdapter.Instance.GetHero(vulcaniteId);
			SelectLand(vulcaniteData.Quadrant);
			OnVulcaniteSelected(vulcaniteId);
			Show();
		}

		private void SelectLand(Quadrant quadrant)
		{
			flagOutlines.ForEach(f => f.enabled = IsCurrentFlag(f, quadrant));
			flagImages.ForEach(f => f.color = IsCurrentFlag(f, quadrant) ? Color.white : Color.gray);
			currentLand = quadrant;
			RenderAsync().Forget();

			static bool IsCurrentFlag(Object flag, Quadrant landType) => flag.name.Contains(landType.ToString());
		}

		private async UniTask RenderAsync()
		{
			render?.Cancel();
			render?.Dispose();
			render = new CancellationTokenSource();
			var updateToken = render.Token;
			
			var query = GameDataBaseAdapter.Instance.AllHeroes()
				.Where(x => x.Quadrant == currentLand)
				.OrderBy(x => x.LevelAtSync)
				.ToArray();

			itemViews ??= new List<HexagoneItemView>();
			var customItem = CustomisationServiceAdapter.Repository.GetFirstEquipped(CustomisationType.AvatarFrame);
			var loadTasks = query.Select(async (vulcaniteData, index) =>
			{
				var ownedHero = VulcaniteHandler.Owned.FirstOrDefault(v => v.VulcaniteId == vulcaniteData.Id);

				if (index >= itemViews.Count)
					itemViews.Add(Instantiate(HexagoneItemView, hexagoneLayout.transform));
				
				var itemView = itemViews[index];
				var itemAvailable = VulcaniteHandler.IsValidVulcanite(vulcaniteData.Id);
				var isRent = ownedHero?.IsRent ?? false;
				itemView.SetDynamicallyCreated(false); // used in pool items

				await itemView.InitAsync(vulcaniteData.Id, vulcaniteData.ArtUrl, customItem?.PreviewURL, "Vulcanite_Mask", updateToken);
				updateToken.ThrowIfCancellationRequested();
				
				itemView.SetActiveLock(!itemAvailable);
				itemView.SetActiveBottomText(itemAvailable);
				itemView.SetBottomText(vulcaniteData.LevelAtSync.ToRoman());
				itemView.Select(vulcaniteData.Id == currentVulcanite?.VulcaniteId);
				itemView.SetBorderColor(isRent ? Color.yellow : Color.white);
				
				if (itemAvailable)
					itemView.OnClicked += item =>
					{
						itemViews.ForEach(x => x.Select(x.Id == item.Id));
						OnVulcaniteSelected(item.Id);
					};
			});

			await UniTask.WhenAll(loadTasks);
			updateToken.ThrowIfCancellationRequested();
			
			for (var i = 0; i < itemViews.Count; i++)
			{
				var itemView = itemViews[i];
				if (i + 1 > query.Length)
				{
					itemView.Close(noAnimation:true);
					continue;
				}
				itemView.Show(noAnimation:true);
			}
			
			hexagoneLayout.RefreshLayout();
		}

		private void OnVulcaniteSelected(string vulcaniteId)
		{
			var gameDataBase = GameDataBaseAdapter.Instance;
			var ownedVulcanite = VulcaniteHandler.Owned.FirstOrDefault(v => v.VulcaniteId == vulcaniteId) ?? VulcaniteHandler.Owned.First();
			var vulcaniteData = gameDataBase.GetHero(ownedVulcanite.VulcaniteId);
			loadingResources?.Cancel();
			loadingResources?.Dispose();
			loadingResources = new CancellationTokenSource();
			PlayerImage.LoadResourceAsync(vulcaniteData.ArtUrl, loadingResources.Token).Forget();
			CoatImg.LoadResourceAsync($"{vulcaniteData.Quadrant}_Coat", loadingResources.Token).Forget();
			
			string[] parts = vulcaniteData.Name?.Split(',');
			Set(NameTxt, parts[0].Trim());
			Set(NameDescription, parts.Length > 1 ? parts[1].Trim() : "");
			Set(LandName, vulcaniteData.Quadrant.ToString());
			Set(LevelTxt, vulcaniteData.LevelAtSync.ToRoman());
			SetActive(DescriptionContainer, vulcaniteData.EffectsIds.Any());

			var effectsDescription = gameDataBase
				.GetEffects(vulcaniteData.EffectsIds)
				.Select(effectData => gameDataBase.GetKeyword(effectData.KeywordId).GetEffectDescription(effectData))
				.JoinToString("\n");
			
			Set(EffectTxt, effectsDescription);
			currentVulcanite = ownedVulcanite;
		}

		private void Select()
		{
			onAvatarSelected?.Invoke(currentVulcanite);
			Close();
		}
		
		protected override void OnClosed()
		{
			base.OnClosed();
			PreviewSystemAdapter.Instance.Close();
			itemViews.ForEach(x=> Destroy(x.gameObject));
			itemViews.Clear();
			PlayerImage.ReleaseResource();
			CoatImg.ReleaseResource();
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnUserDataRefreshed);
			loadingResources?.Cancel();
			loadingResources?.Dispose();
			loadingResources = null;
		}

		protected override void OnHidden()
		{
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnUserDataRefreshed);
		}
		
		private void OnUserDataRefreshed()
		{
			RenderAsync().Forget();
		}
		
		private void OnDestroy()
		{
			LobbyBus.OnUserDataRefreshed.Unsubscribe(OnUserDataRefreshed);
			BackButton.UnSubscribe(Close);
			OKButton1.Subscribe(Select);
			Boreas.onClick.RemoveAllListeners();
			Arcadia.onClick.RemoveAllListeners();
			Notus.onClick.RemoveAllListeners();
			Hades.onClick.RemoveAllListeners();
			PlayerImage.ReleaseResource();
			CoatImg.ReleaseResource();
			loadingResources?.Cancel();
			loadingResources?.Dispose();
			loadingResources = null;
		}
	}
}