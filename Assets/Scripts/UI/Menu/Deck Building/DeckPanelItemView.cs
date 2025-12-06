using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.PreviewSystem;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.ResourceManagament;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Button))]
	public partial class DeckPanelItemView : DeckItemView, IPreviewable
	{
		private Button removeButton;
		private CancellationTokenSource updateArt;
		private bool isLavaSet;
		
		protected override void Start()
		{
			base.Start();
			removeButton = GetComponent<Button>();
			Subscribe(removeButton, HandleOnClick);
			SetLavaImage(true);
			PreviewSystemAdapter.Instance.Registration(this);
		}
		
		protected override void OnRefreshView(IDeckCardStack deckCardStack)
		{
			if (deckCardStack == null || deckCardStack.Count == 0)
			{
				RRLogger.Log("[OffFactionLava][DeckPanelItem] OnRefreshView: empty stack");
				return;
			}
			
			base.OnRefreshView(deckCardStack);

			var cardData = deckCardStack.CardData;
			if (cardData == null)
			{
				RRLogger.Log("[OffFactionLava][DeckPanelItem] OnRefreshView: CardData is null");
				return;
			}
			
			var effectiveLava = cardData.Mana;

			var deckPanel = GetComponentInParent<CurrentDeckPanel>();
			if (deckPanel != null)
			{
				RRLogger.Log("[OffFactionLava][DeckPanelItem] Found CurrentDeckPanel via GetComponentInParent");
			}
			else
			{
				deckPanel = Object.FindObjectOfType<CurrentDeckPanel>();
				if (deckPanel != null)
					RRLogger.Log("[OffFactionLava][DeckPanelItem] Found CurrentDeckPanel via FindObjectOfType");
				else
					RRLogger.Log("[OffFactionLava][DeckPanelItem] CurrentDeckPanel NOT FOUND, using base mana");
			}

			if (deckPanel != null)
			{
				effectiveLava = deckPanel.CalculateOffFactionLavaForCard(cardData);
				RRLogger.Log(
					$"[OffFactionLava][DeckPanelItem] Card='{cardData.Title}', Base={cardData.Mana}, Effective={effectiveLava}");
			}
			
			Set(LavaTxt, effectiveLava);
			SetLavaImage(!cardData.SubTypes.Contains(SubType.Token));
			Set(DeckNameText, cardData.Title);
			SetWarning(!deckCardStack.AllInStackValid);
			SetCount(deckCardStack.Count);
		}

		private void SetLavaImage(bool isLava)
		{
			if (isLavaSet != isLava)
			{
				isLavaSet = isLava;
				updateArt?.Cancel();
				updateArt?.Dispose();
				updateArt = new CancellationTokenSource();
				var lavaOrTokenUrl = isLavaSet ? "Lava" : "Token";
				LavaImage.LoadResourceAsync(lavaOrTokenUrl, updateArt.Token).Forget();
			}
			SetActive(LavaTxt, isLava);
		}

		private void SetWarning(bool value)
		{
			SetActive(OutlineImg, value);
		}

		private void SetCount(int count)
		{
			Set(CounterText, $"x{count}");
		}

		private OwnedCard StackSelector(OwnedCard[] stack)
		{
			return stack?.FirstOrDefault(x=> !x.IsValid()) ?? stack?.FirstOrDefault();
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if (removeButton)
				removeButton.onClick.RemoveAllListeners();
			
			PreviewSystemAdapter.Instance.UnRegistration(this);
			LavaImage.ReleaseResource();
			updateArt?.Cancel();
			updateArt?.Dispose();
			updateArt = null;
		}

		private void HandleOnClick()
		{
			PreviewSystemAdapter.Instance.Close();
			DeckCardStack?.RequestToRemove(DeckCardStack?.Get(StackSelector));
		}

		#region Previewable

		public GameObject TargetView => gameObject;

		public IPreviewData PreviewData => DeckCardStack?.CardData?.ToPreviewData();
		public IPreviewSetting PreviewSettings { get; } = new PreviewSettings(PreviewType.Fit);
		public bool CanPreview() => TargetView && DeckCardStack?.CardData != null;

		#endregion
	}
}
