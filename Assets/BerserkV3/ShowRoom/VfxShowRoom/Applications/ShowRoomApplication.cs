using System;
using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.Controllers;
using BerserkV3.GameCore.EffectsVisual.Abstractions;
using UnityEngine;
using UnityEngine.UI;
using Vulcan.Audio;
using Zenject;
using Object = UnityEngine.Object;

namespace BerserkV3.ShowRoom.VfxShowRoom
{
	public class ShowRoomApplication : IInitializable
	{
		private const string VFX_LIST_VIEW_PATH = "VfxKeywordList";
		private const string CARD_VIEW_PATH = "ShowRoomCard";
		private const int CARD_COUNT = 1;
		private readonly IInstantiator instantiator;
		private readonly IGameContainers gameContainers;
		private readonly IVfxApplication vfxApplication;
		private readonly List<Transform> showRoomCards;
		private VfxKeywordList listView;
		
		public ShowRoomApplication(
			IInstantiator instantiator, 
			IGameContainers gameContainers,
			IVfxApplication vfxApplication)
		{
			this.instantiator = instantiator;
			this.gameContainers = gameContainers;
			this.vfxApplication = vfxApplication;
			showRoomCards = new List<Transform>();
		}
		
		public void Initialize()
		{
			BuildShowRoomCards();
			BuildShowRoomVfx();
			AudioController.SetVolume(0.1f);
		}

		private void BuildShowRoomVfx()
		{
			listView = instantiator
				.InstantiatePrefabResourceForComponent<VfxKeywordList>(VFX_LIST_VIEW_PATH, gameContainers.GameContainer);
			listView.Clear();
			foreach (EffectVisualKeyword keyword in Enum.GetValues(typeof(EffectVisualKeyword)))
			{
				var item = listView.CreateItem();
				item.Id = keyword;
				item.SetText(keyword.ToString());
				item.OnClick += OnShowVfx;
			}
		}

		private void BuildShowRoomCards()
		{
			var layoutGroup = gameContainers.TableContainer.gameObject.AddComponent<HorizontalLayoutGroup>();
			layoutGroup.childForceExpandHeight = false;
			layoutGroup.childForceExpandWidth = false;
			layoutGroup.childAlignment = TextAnchor.MiddleCenter;
			layoutGroup.padding = new RectOffset(10, 10, 10, 10);
			layoutGroup.spacing = 210;
			
			for (var i = 0; i < CARD_COUNT; i++)
			{
				var cardView = instantiator.InstantiatePrefabResource(CARD_VIEW_PATH, gameContainers.TableContainer);
				showRoomCards.Add(cardView.transform);
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(gameContainers.TableContainer);
		}
		
		private void OnShowVfx(EffectVisualKeyword keyword)
		{
			foreach (var roomCard in showRoomCards)
			{
				var view = vfxApplication.SpawnVfx(keyword, roomCard);
				Object.Destroy(view.gameObject, listView.GetShowTime());
			}
		}
	}
}