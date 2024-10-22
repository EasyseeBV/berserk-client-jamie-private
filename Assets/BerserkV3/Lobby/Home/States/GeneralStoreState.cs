using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Shop;
using BerserkV3.Common.PurchasingSystem.Abstractions;
using BerserkV3.Common.StateMachine;
using BerserkV3.Lobby.Home.Abstractions;
using BerserkV3.Lobby.Store.Abstractions;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Controllers;
using BerserkV3.Lobby.UI.Home.CommonWidgets.Models;
using BerserkV3.Lobby.UI.Store;
using BerserkV3.Lobby.UI.Store.Models;
using Cysharp.Threading.Tasks;
using RR.UIService;
using Zenject;

namespace BerserkV3.Lobby.Home.States
{
	public class GeneralStoreState : StateWithSubStates
	{
		public const string RefreshPrefix = "<sprite name=\"Symbol_Reload\"> REFRESH -";

		private readonly IUIService uiService;
		private readonly IStoreApplication storeApplication;
		private readonly IStoreRepository storeRepository;
		private readonly IPurchaseWindowApplication purchaseWindowApplication;

		private MainMenuStoreWindow storeWindow;
		private RegularTimerController regularTimerController;
		private SlotTimerController slotTimerController;
		private int selectedTabIndex = 0;
		private List<StoreTabViewModel> storeTabs = new List<StoreTabViewModel>();

		public GeneralStoreState(
			IStateMachine subStateMatchine,
			IInstantiator instantiator,
			IPurchasingApplication purchasingApplication,
			IUIService uiService,
			IStoreApplication storeApplication,
			IStoreRepository storeRepository, 
			IPurchaseWindowApplication purchaseWindowApplication)
			: base(subStateMatchine, instantiator)
		{
			this.uiService = uiService;
			this.storeApplication = storeApplication;
			this.storeRepository = storeRepository;
			this.purchaseWindowApplication = purchaseWindowApplication;
		}

		public override void OnEnter(params object[] args)
		{
			InitAsync().Forget();
		}

		public override void OnExit()
		{
			storeWindow.ToggleGroup.OnToggleSelected -= OnTabSelectionChanged;
			storeWindow.OnBuyButtonPressed -= HandleBuyButtonPressed;
			uiService.Begin<MainMenuStoreWindow>()
				.WithInit(ReleaseWindow)
				.Hide();
			
			base.OnExit();
			return;
			
			void ReleaseWindow(MainMenuStoreWindow window)
			{
				window.ToggleGroup.Release();
			}
		}

		private async UniTask InitAsync()
		{
			await storeApplication.FetchStoreAsync();
			storeTabs = storeRepository.GetTabsAndSubTabs().Keys.ToList(); // There is no subtabs now, so working with tabs directly.
			var selectedTab = storeTabs[selectedTabIndex];
			await storeApplication.FetchTabAsync(selectedTab.Id);
			var productsInTab = storeRepository.GetProductsInTab(selectedTab.Id).Values.ToList()[0];
			var mainProductsInTab = storeRepository.GetMainProductsInTab(selectedTab.Id);
			ProductModel mainProductInTab = null;
			if (mainProductsInTab.Count > 0)
				mainProductInTab = storeRepository.GetMainProductsInTab(selectedTab.Id).Values.ToList()[0];

			uiService.Begin<MainMenuStoreWindow>()
				.WithInit(window =>
				{
					window.ToggleGroup.Init();
					window.Init(storeTabs, productsInTab, mainProductInTab, selectedTabIndex);
					window.ToggleGroup.OnToggleSelected += OnTabSelectionChanged;
					window.OnBuyButtonPressed += HandleBuyButtonPressed;
				})
				.Show();

			storeWindow = uiService.Get<MainMenuStoreWindow>();
			InitTimers();
			UpdateTabsAsync();
		}

		private void InitTimers()
		{
			//MOCK CODE!!! Since server don;t have refresh timer now
			var mockEndTime = DateTime.Now.AddDays(6).AddHours(3);
			// MOCK CODE!!!

			Action onTimerEnd = RefreshTab;
			var regularTimerWidget = storeWindow.RefreshTimerWidget;
			var regularTimerModel = new TimerModel(mockEndTime, RefreshPrefix);
			regularTimerController = Instantiator.Instantiate<RegularTimerController>(
				new object[] { regularTimerModel, onTimerEnd, regularTimerWidget });

			var slotTimerWidget = storeWindow.SlotTimerWidget;
			var slotTimerModel = new TimerModel(mockEndTime, RefreshPrefix);
			slotTimerController = Instantiator.Instantiate<SlotTimerController>(
				new object[] { slotTimerModel, onTimerEnd, slotTimerWidget });
		}

		private void OnTabSelectionChanged(int selectedTabIndex)
		{
			if (this.selectedTabIndex == selectedTabIndex)
				return;

			this.selectedTabIndex = selectedTabIndex;

			UpdateTabsAsync().Forget();
		}

		private async UniTask UpdateTabsAsync()
		{
			var selectedTab = storeTabs[selectedTabIndex];
			await storeApplication.FetchTabAsync(selectedTab.Id);
			var productsInTab = storeRepository.GetProductsInTab(selectedTab.Id).Values.ToList()[0];
			var mainProductsInTab = storeRepository.GetMainProductsInTab(selectedTab.Id);
			ProductModel mainProductInTab = null;
			if (mainProductsInTab.Count > 0)
				mainProductInTab = storeRepository.GetMainProductsInTab(selectedTab.Id).Values.ToList()[0];

			storeWindow.ActiveTabView.CleanSubscribers();
			storeWindow.UpdateContent(selectedTab, selectedTabIndex);
			storeWindow.ActiveTabView.UpdateContent(selectedTab, productsInTab, mainProductInTab, HandleBuyButtonPressed);
		}

		private void HandleBuyButtonPressed(ProductModel productModel)
		{
			// no confirmation just purchase thru purchasing system in case of fiat
			if(productModel.DefaultPrice.WalletType == WalletType.Fiat)
				storeApplication.BuyItemWithHardCurrencyAsync(productModel.Id);
			else
				ShowConfirmationPopup(productModel);
		}

		private void ShowConfirmationPopup(ProductModel productModel)
		{
			purchaseWindowApplication.Init(productModel);
		}

		private void RefreshTab()
		{
			// TODO add refreshing when server will be ready
		}
	}
}