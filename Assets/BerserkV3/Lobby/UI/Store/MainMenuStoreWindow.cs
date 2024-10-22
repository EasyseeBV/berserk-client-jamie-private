using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Shop;
using BerserkV3.Common.UIKit;
using BerserkV3.Lobby.UI.Home.CommonWidgets;
using BerserkV3.Lobby.UI.Store.Abstractions;
using BerserkV3.Lobby.UI.Store.Models;
using BerserkV3.Lobby.UI.Store.Tabs;
using RR.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI.Store
{
	public class MainMenuStoreWindow : UIWindowBase
	{
		public event Action<ProductModel> OnBuyButtonPressed;
		
		[SerializeField] private ExtendedToggleGroup toggleGroup;
		[SerializeField] private TMP_Text headerLabel; 
		[SerializeField] private StoreTabView regularTabView;
		[SerializeField] private StoreFeaturedTabView featuredTabView;
		[SerializeField] private SlotTimerUIWidget slotTimerWidget;
		[SerializeField] private RegularTimerUIWidget regularTimerWidget;
		[SerializeField] private GameObject refreshTimerObject;
		[SerializeField] private Button buyRefreshButton;
		
		private List<string> tabNames = new List<string>();
		
		public ExtendedToggleGroup ToggleGroup => toggleGroup;
		public SlotTimerUIWidget SlotTimerWidget => slotTimerWidget;
		public RegularTimerUIWidget RefreshTimerWidget => regularTimerWidget;
		
		public IStoreTabView ActiveTabView { get; private set; }

		public void Init(List<StoreTabViewModel> tabModels, List<ProductModel> products, ProductModel mainProduct, int selectedTabIndex)
		{
			tabNames.Clear();
			foreach (var storeTab in tabModels)
				tabNames.Add(storeTab.Title);

			toggleGroup.AddOrRefresh(tabNames, selectedTabIndex);
			UpdateContent(tabModels[selectedTabIndex], selectedTabIndex);
		}

		public void UpdateContent(StoreTabViewModel storeTabViewModel, int selectedTabIndex)
		{
			var isFeatured = storeTabViewModel.IsFeatured;
			
			featuredTabView.SetActive(isFeatured);
			regularTabView.SetActive(!isFeatured);
			refreshTimerObject.SetActive(!isFeatured);

			if (isFeatured)
				ActiveTabView = featuredTabView;
			else
				ActiveTabView = regularTabView;
			
			SetHeader(tabNames[selectedTabIndex]);
		}

		private void SetHeader(string header)
		{
			headerLabel.text = header;
		}
	}
}