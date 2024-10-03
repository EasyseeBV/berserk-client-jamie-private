using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.Data.Lobby;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.PreviewSystem;
using Cysharp.Threading.Tasks;
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
				return;
			
			base.OnRefreshView(deckCardStack);
			Set(LavaTxt, deckCardStack.CardData.Mana);
			SetLavaImage(!deckCardStack.CardData.SubTypes.Contains(SubType.Token));
			Set(DeckNameText, deckCardStack.CardData.Title);
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