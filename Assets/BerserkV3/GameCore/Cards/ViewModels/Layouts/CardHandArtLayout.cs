using System;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Enums;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using RR.Core.ResourceManagament;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public partial class CardHandArtLayout : BaseView
	{
		[SerializeField] private ArtType artType;
		
		public ArtType ArtType => artType;
		private bool releasePrevious;
		
		public async UniTask<CardHandArtLayout> SetupAsync(CardDataAdapter data, CancellationToken updateToken)
		{
			var prefix = $"{data.ArtType}_{data.Quadrant}";
			var frameUri = "Frame";
			Refresh(data);
			
			await UniTask.WhenAll(
				ArtImage.LoadResourceAsync(data.ArtUrl, updateToken, releasePrevious),
				ShineImage.LoadResourceAsync($"Card_Shine", updateToken, releasePrevious),
				FrameImage.LoadResourceAsync($"{prefix}_{frameUri}", updateToken, releasePrevious),
				SetupLavaArtAsync(data.IsToken, updateToken),
				AttackImage.LoadResourceAsync($"{prefix}_Attack", updateToken, releasePrevious),
				ArmorImage.LoadResourceAsync($"{prefix}_Armor", updateToken, releasePrevious),
				HealthImage.LoadResourceAsync($"{prefix}_Health", updateToken, releasePrevious),
				RarityImage.LoadResourceAsync($"{nameof(Rarity)}_{data.Rarity}", updateToken, releasePrevious),
				OutlineImage.LoadResourceAsync($"{prefix}_Outline", updateToken, releasePrevious),
				LavaFrameImage.LoadResourceAsync($"{prefix}_Lava_{frameUri}", updateToken, releasePrevious),
				FactionFrameImage.LoadResourceAsync($"{prefix}_{nameof(Faction)}_{frameUri}", updateToken, releasePrevious),
				FactionImage.LoadResourceAsync($"{data.Factions.FirstOrDefault()}_Icon", updateToken, releasePrevious),
				SetupSeasonArtAsync(data.Season, $"{prefix}_{nameof(Season)}_{frameUri}", updateToken),
				SetupQuadrantArtAsync(data.Quadrant,$"{prefix}_{nameof(Quadrant)}_{frameUri}", updateToken),
				SetupExtraArtAsync($"{prefix}_Extra", updateToken),
				SetupAristArtAsync(data.Artist, "ArtistBrush", updateToken));

			releasePrevious = true;
			return this;
		}

		public void Refresh(CardDataAdapter data)
		{
			SetAttackText(data.Attack);
			SetArmorText(data.Armor);
			SetHealthText(data.Hp);
			SetManaText(data.Lava);
			SwitchType(data.Type);
			
			if (TitleText)
				Set(TitleText, data.Title);
			
			if (DescriptionText)
				Set(DescriptionText,data.Tooltip);
			
			if (EffectText)
				Set(EffectText, data.Description);
			
			if (RaceFactionText)
			{
				var raceAndFactions = $"{string.Join(" ", data.Races ?? Array.Empty<Race>()).Replace("_", " ")} - " +
				                      $"{string.Join(" ", data.Factions ?? Array.Empty<Faction>()).Replace("_", " ")}";
				Set(RaceFactionText, raceAndFactions);
			}

			if (ArtistText)
				Set(ArtistText, data.Artist);
		}

		public void SetActive(bool value)
		{
			gameObject.SetActive(value);
		}
		
		public void SetAttackText(int value)
		{
			if (AttackText)
				Set(AttackText, value);
		}
		
		public void SetArmorText(int value)
		{
			if (ArmorText)
				Set(ArmorText, value);
		}

		public void SetHealthText(int value)
		{
			if (HealthText)
				Set(HealthText, value);
		}

		public void SetManaText(int value)
		{
			if (LavaText)
				Set(LavaText, value);
		}

		public void SetActiveOutline(bool value)
		{
			if (OutlineImage)
				SetActive(OutlineImage, value);
		}

		public void SetTransparency(float space01)
		{
			CanvasGroup.alpha = space01;
		}

		public void SetOutlineColor(Color value)
		{
			if (OutlineImage)
				Set(OutlineImage, value);
		}
		
		public void SetActiveShine(bool value)
		{
			if (ShineImage)
				SetActive(ShineImage, value);
		}

		public void Release()
		{
			if (!releasePrevious)
				return;

			releasePrevious = false;
			ShineImage.ReleaseResource();
			FrameImage.ReleaseResource();
			ArtImage.ReleaseResource();
			LavaImage.ReleaseResource();
			AttackImage.ReleaseResource();
			ArmorImage.ReleaseResource();
			HealthImage.ReleaseResource();
			RarityImage.ReleaseResource();
			OutlineImage.ReleaseResource();
			LavaFrameImage.ReleaseResource();
			FactionFrameImage.ReleaseResource();
			FactionImage.ReleaseResource();
			SeasonFrameImage.ReleaseResource();
			SeasonImage.ReleaseResource();
			
			if (QuadrantFrameImage && QuadrantFrameImage.IsActive())
			{
				QuadrantFrameImage.ReleaseResource();
				QuadrantImage.ReleaseResource();
			}
			
			if (ExtraImage && ExtraImage.IsActive())
				ExtraImage.ReleaseResource();
			
			if (ArtistBrushImage && ArtistBrushImage.IsActive())
				ArtistBrushImage.ReleaseResource();
		}

		public UniTask SetupQuadrantArtAsync(Quadrant quadrant, string quadrantArtUrl, CancellationToken token)
		{
			SetActive(QuadrantFrameImage, quadrant != Quadrant.Neutral);
			
			if (quadrant != Quadrant.Neutral)
				return UniTask.WhenAll(QuadrantFrameImage.LoadResourceAsync(quadrantArtUrl, token, releasePrevious),
					QuadrantImage.LoadResourceAsync($"{quadrant}_Circle", token, releasePrevious));
			
			return UniTask.CompletedTask;
		}
		
		public UniTask SetupSeasonArtAsync(Season season, string seasonFrameUrl, CancellationToken token)
		{
			var hasSeason = season != Season.None;

			SetActive(SeasonFrameImage, hasSeason);
			SetActive(SeasonImage, hasSeason);

			if (hasSeason)
			{
				return UniTask.WhenAll(
					SeasonFrameImage.LoadResourceAsync(seasonFrameUrl, token, releasePrevious),
					SeasonImage.LoadResourceAsync($"{season}", token, releasePrevious));
			}

			return UniTask.CompletedTask;
		}

		public UniTask SetupLavaArtAsync(bool isToken, CancellationToken token)
		{
			SetActive(LavaText, !isToken);
			return LavaImage.LoadResourceAsync(isToken ? "Token": "Lava", token, releasePrevious);
		}

		public UniTask SetupExtraArtAsync(string extraArtUrl, CancellationToken token)
		{
			if (!ExtraImage)
				return UniTask.CompletedTask;
			
			SetActive(ExtraImage, artType == ArtType.Full);
			
			return artType == ArtType.Full ? 
				ExtraImage.LoadResourceAsync(extraArtUrl, token, releasePrevious) 
				: UniTask.CompletedTask;
		}

		public UniTask SetupAristArtAsync(string artist, string artistArtUrl, CancellationToken token)
		{
			SetActive(ArtistLayout, !string.IsNullOrEmpty(artist));
			
			return !string.IsNullOrEmpty(artist) ? 
				ArtistBrushImage.LoadResourceAsync(artistArtUrl, token, releasePrevious) 
				: UniTask.CompletedTask;
		}

		public void SetLavaTextColor(Color color)
		{
			LavaText.color = color;
		}
		public void SwitchType(ObjectType type)
		{
			if (!AttackImage || !HealthImage || !ArmorImage)
			{
				RRLogger.Error($"[{GetType().Name.Orange()}] Cannot switch type, target components is missing : {AttackImage}, {HealthImage}, {ArmorImage}");
				return;
			}
			
			switch (type)
			{
				case ObjectType.Building:
					SetActive(AttackImage, false);
					SetActive(HealthImage, false);
					SetActive(ArmorImage, true);
					break;
				case ObjectType.Creature:
					SetActive(AttackImage, true);
					SetActive(HealthImage, true);
					SetActive(ArmorImage, false);
					break;
				case ObjectType.Spell:
					SetActive(AttackImage, false);
					SetActive(HealthImage, false);
					SetActive(ArmorImage, false);
					break;
			}
		}

		private void OnDestroy()
		{
			Release();
		}
	}
}