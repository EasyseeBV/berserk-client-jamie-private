using System.Globalization;
using Lobby;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using System.Linq;
using System.Threading.Tasks;
using BerserkV3.Common.DataBase;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using UnityEngine;
using Vulcan.Application;
using Vulcan.Shop.Domain;

namespace UI
{
	public partial class LavaShopView : BaseView
	{
		[SerializeField] private string ownedText = "owned";
		[SerializeField] private int storeCapacity = 7;
		[SerializeField] private int topContentCapacity = 4;

		private IShopApplication shopApplication;
		private VulcaniteEntity currentVulcanite;

		protected override void OnAwake()
		{
			shopApplication = new ShopApplication();
			BackButton.Subscribe(Close);
			BuyButton.Subscribe(TryBuyAsync);
			SetActive(VulcanitePanel, false);
		}

		public async UniTask InitAndShowAsync()
		{
			if (shopApplication.RequiredRefresh)
			{
				await shopApplication.RefreshData().AddLoadingTask();
				
				Set(LavaValueTxt, shopApplication.GetBalance());
				SetUpVulcanitesPanel();
			}

			Show();
		}

		private void SetUpVulcanitesPanel()
		{
			TopContent.DestroyChildrenExcept(VulcanitePanel.transform);
			BottomContent.DestroyChildren();

			var shopVulcanites = shopApplication.GetShopVulcanites();

			shopVulcanites.ForEach((vulcanite, index) =>
			{
				if (index >= storeCapacity)
				{
					RRLogger.Error("The store needs an extension of the counter \n" +
						$"Number of items: {shopVulcanites.Count()}");
					return;
				}

				var parentContent = index < topContentCapacity ? TopContent : BottomContent;

				//TODO: Make a new vulcaniteButton accept the essence of the vulcanite, not its data
				var data = GameDataBaseAdapter.Instance.GetHero(vulcanite.Id);
				var vulcaniteButton = Instantiate(VulcanitePanel, parentContent.transform)
				.SetUp(data, vulcanite.Level, SelectVulcanite);

				vulcaniteButton.gameObject.SetActive(true);
				vulcaniteButton.SetAsSelected(false);
				vulcanite.Dispose();
				vulcanite.OnSelected += result => vulcaniteButton.SetAsSelected(result);

				if (index == 0)
					SelectVulcanite(vulcanite.Id);
			});
		}

		private void SelectVulcanite(string vulcaniteId)
		{
			//add shop vulcanite repository

			var vulcanites = shopApplication.GetShopVulcanites();
			var vulcaniteData = GameDataBaseAdapter.Instance.GetHero(vulcaniteId);
			vulcanites.ForEach(x => x.Unselect());
			currentVulcanite = vulcanites.FirstOrDefault(v => v.Id == vulcaniteId);
			currentVulcanite.Select();

			PlayerImage.LoadResourceAsync(vulcaniteData.ArtUrl).Forget();
			CoatImg.LoadResourceAsync($"{vulcaniteData.Quadrant}_Coat").Forget();
			Set(NameTxt, vulcaniteData.Name?.Replace(",", "\n<size=50%>"));
			Set(LandName, vulcaniteData.Quadrant.ToString());
			Set(LevelTxt, vulcaniteData.LevelAtSync.ToRoman());
			Set(EffectTxt, currentVulcanite.GetEffectsInfoText());
			SetActive(DescriptionContainer, vulcaniteData.LevelAtRegistration == 0);
			SetUpBuyButton();
		}

		private async void TryBuyAsync()
		{
			await shopApplication.TryByuAsync(currentVulcanite);
			
			Set(LavaValueTxt, shopApplication.GetBalance());
			SetUpBuyButton();
		}

		private void SetUpBuyButton()
		{
			var purchaseAvailable = shopApplication.PurchaseAvailable(currentVulcanite);
			var isOwned = currentVulcanite.IsOwned;
			
			SetInteractable(BuyButton, purchaseAvailable);
			SetActive(BuyText, !isOwned);
			SetActive(LavaText, !isOwned);

			var text = !isOwned
				? currentVulcanite.Price.ToString(CultureInfo.InvariantCulture)
				: ownedText;

			Set(BuyButton, text);
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			PlayerImage.ReleaseResource();
			CoatImg.ReleaseResource();
		}

		private void OnDestroy()
		{
			PlayerImage.ReleaseResource();
			CoatImg.ReleaseResource();
		}
	}
}