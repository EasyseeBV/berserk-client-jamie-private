using System;
using System.Collections.Generic;
using Berserk.Shared.Data.UserInventory;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.Lobby.Abstractions;
using BerserkV3.Common.PreviewSystem;
using UnityEngine;

namespace BerserkV3.Lobby.UI
{
	public interface IDeckValue : IDisposable
	{
		float Value { get; }
		void Set(float value);
		void Refresh(IEnumerable<OwnedCard> deck);
	}

	public class DeckValueController : IDeckValue, IPreviewable
	{
		private readonly IDeckValueView deckValueView;
		private readonly IPreviewSystem previewSystem;
		private readonly IDeckValueService deckValueService;
		public float Value { get; private set; }

		public DeckValueController(
			IDeckValueView deckValueView,
			IPreviewSystem previewSystem,
			IDeckValueService deckValueService)
		{
			this.deckValueView = deckValueView;
			this.previewSystem = previewSystem;
			this.deckValueService = deckValueService;
			previewSystem.Registration(this);
			PreviewSettings = new PreviewSettings(PreviewType.DeckValueInfo);
			PreviewData = new DeckValueInfoData
			{
				Title = "Deck Value",
				Description = "Basic +0.5pt\n" +
				              "Common +1pt\n" +
				              "Rare +5pt\n" +
				              "Mythic +25pt\n" +
				              "Epic +50pt\n" +
				              "Legendary +250pt\n" +
				              "If the deck contains at least 1 actual NFT + 200pt\n" +
				              "If the deck contains 10+ NFTs + 300pt\n" +
				              "If the deck contains all NFTs – 2x value"
			};
		}

		public void Dispose()
		{
			previewSystem.UnRegistration(this);
			PreviewData?.Dispose();
		}

		public void Set(float value)
		{
			Value = value;
			deckValueView?.SetValueText($"deck value: {value:F2}");
		}

		public void Refresh(IEnumerable<OwnedCard> deck)
		{
			try
			{
				Set(deckValueService.CalculateDeckValue(deck));
			}
			catch (Exception e)
			{
				DefaultSharedLogger.Error(e);
			}
		}

		#region Previewable

		public GameObject TargetView => deckValueView.TargetView;
		public IPreviewData PreviewData { get; }
		public IPreviewSetting PreviewSettings { get; }

		public bool CanPreview() => true;

		#endregion Previewable
	}
}