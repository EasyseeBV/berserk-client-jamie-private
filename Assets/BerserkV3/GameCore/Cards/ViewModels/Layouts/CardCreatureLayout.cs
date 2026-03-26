using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore;
using Berserk.Shared.GameCore.LogicContext;
using Berserk.Shared.GameCore.Utils;
using BerserkV3.Common.TutorialSystem;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Core.DebugSystem;
using RR.Game.TutorialSystemV2.Realizations;
using TMPro;
using UI;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public readonly struct FaceId
	{
		public static FaceId Default { get; } = new(null);
		public string Key { get; }
		public int Order { get; }

		public FaceId(object id, int? order = null)
		{
			Key = id?.ToString() ?? "None";
			Order = order ?? 0;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public bool Equals(FaceId other)
		{
			return Key == other.Key && Order == other.Order;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Key, Order);
		}
	}
	
	public partial class CardCreatureLayout : BaseCardLayout, ICreatureLayout
	{
		private CancellationTokenSource updateFace;
		private bool previouseCommonRelease;
		private ArtType? loadedArtType;
		private FaceId? currentFaceId;
		private readonly List<FaceId> faceIdStack = new();
		private bool compactStatOffsetsCached;
		private Vector2 attackStatOriginalPosition;
		private Vector2 healthStatOriginalPosition;
			private Vector2 armorStatOriginalPosition;
			private Vector3 attackStatOriginalScale;
			private Vector3 healthStatOriginalScale;
			private Vector3 armorStatOriginalScale;
			private bool compactStatCanvasConfigured;
			private Canvas attackStatCanvas;
			private Canvas healthStatCanvas;
			private Canvas armorStatCanvas;
		
		protected virtual string TurnShineArtUrl => "Card_Shine_{0}";
		protected virtual string TargetShineArtUrl => TurnShineArtUrl;
		protected virtual string SelectionShineArtUrl => TurnShineArtUrl;
		protected virtual string BorderArtUrl => "Card_Border_{0}";
		protected virtual string MaskArtUrl => "Card_Mask_{0}";
		protected virtual string HealthArtUrl => $"Card_Attribute_Health";
		protected virtual string AttackArtUrl => $"Card_Attribute_Attack";
		protected virtual string ArmorArtUrl => $"Card_Attribute_Armor";

		public bool IsAllowedExternal { get; set; }
		public bool IsHealthVisible { get; protected set; } = true;
		public bool IsArmorVisible { get; protected set; }
		public bool IsAttackVisible { get; protected set; } = true;

			protected override async UniTask OnEnabledAsync(CancellationToken token)
			{
				CacheCompactStatOffsets();
				ConfigureCompactStatCanvases();
				SetupHintTargets();
				await UniTask.WhenAll(
				base.OnEnabledAsync(token),
				AddFaceAsync(FaceId.Default, token),
				LoadMainArt(RuntimeGameObject.Data.ArtType, RuntimeGameObject.Data.ArtUrl, token),
				AttackStatImage.LoadResourceAsync(AttackArtUrl, token, previouseCommonRelease),
				HealthStatImage.LoadResourceAsync(HealthArtUrl, token, previouseCommonRelease),
				ArmorStatImage.LoadResourceAsync(ArmorArtUrl, token, previouseCommonRelease));

			previouseCommonRelease = true;
			ApplyCompactStatLayout();
			Refresh();
			
			RuntimeData.Armor.OnChangedFrom += OnArmorChanged;
			RuntimeData.Attack.OnChangedFrom += OnAttackChanged;
			RuntimeData.Hp.OnChangedFrom += OnHpChanged;
		}
		
		public virtual void SetActiveArmor(bool value)
		{
			if (!ArmorStatImage)
				return;

			IsArmorVisible = value;
			SetActive(ArmorStatImage, value);
		}

		public virtual void SetArmor(int value)
		{
			if (!ArmorText)
				return;
			
			UpdateStatVisual(ArmorText, RuntimeData.Armor);
			Set(ArmorText, Mathf.Max(0, value));
		}

		public virtual void SetActiveHealth(bool value)
		{
			if (!HealthStatImage)
				return;

			IsHealthVisible = value;
			SetActive(HealthStatImage, value);
		}

		public virtual void SetHealth(int value)
		{
			if (!HealthText)
				return;
				
			UpdateStatVisual(HealthText, RuntimeData.Hp);
			Set(HealthText, Mathf.Max(0, value));
		}

		public virtual void SetActiveAttack(bool value)
		{
			if (!AttackStatImage)
				return;

			IsAttackVisible = value;
			SetActive(AttackStatImage, value);
		}

		public virtual void SetAttack(int value)
		{
			if (!AttackText)
				return;
				
			UpdateStatVisual(AttackText, RuntimeData.Attack);
			Set(AttackText, Mathf.Max(0, value));
		}

		public override void Refresh()
		{
			base.Refresh();
			ApplyCompactStatLayout();
			
			if (RuntimeData == null)
				return;
			
			OnArmorChanged(RuntimeData.Armor.Previous, RuntimeData.Armor);
			OnAttackChanged(RuntimeData.Attack.Previous, RuntimeData.Attack);
			OnHpChanged(RuntimeData.Hp.Previous, RuntimeData.Hp);
		}
		
		public async UniTask AddFaceAsync(FaceId faceId, CancellationToken token = default)
		{
			if (!faceId.Equals(FaceId.Default))
			{
				faceIdStack.Add(faceId);
			}
			
			faceId = faceIdStack.OrderBy(x => x.Order).DefaultIfEmpty(FaceId.Default).LastOrDefault();
			if (currentFaceId.HasValue && currentFaceId.Equals(faceId))
				return;
			
			updateFace?.Cancel();
			updateFace?.Dispose();
			updateFace = CancellationTokenSource.CreateLinkedTokenSource(token);
			token = updateFace.Token;
			var releasePreviousFace = !string.IsNullOrEmpty(currentFaceId?.Key);

			if (token.IsCancellationRequested)
				return;

			await UniTask.WhenAll(
					ArtBorderImage.LoadResourceAsync(string.Format(BorderArtUrl, faceId.Key), token, releasePreviousFace),
					ArtMaskImage.LoadResourceAsync(string.Format(MaskArtUrl, faceId.Key), token, releasePreviousFace),
					TurnGlowImage.LoadResourceAsync(string.Format(TurnShineArtUrl, faceId.Key), token, releasePreviousFace),
					TargetingGlowImage.LoadResourceAsync(string.Format(TargetShineArtUrl, faceId.Key), token, releasePreviousFace),
					SelectionGlowImage.LoadResourceAsync(string.Format(SelectionShineArtUrl, faceId.Key), token, releasePreviousFace))
				.AttachExternalCancellation(token)
				.SuppressCancellationThrow();

			if (token.IsCancellationRequested)
				return;
					
			currentFaceId = faceId;
		}
		
		public async UniTask RemoveFaceAsync(FaceId faceId)
		{
			faceIdStack.RemoveAll(x => x.Equals(faceId));
			await AddFaceAsync(FaceId.Default);
		}
		
		protected virtual void SetupHintTargets()
		{
			ArmorStatImage.SetHintTarget($"{TutorialTrigger.CreatureArmor}_{GetOwner()}_{RuntimeGameObject.Data.Id}").SetTransitionFactorSize().Init();
			HealthStatImage.SetHintTarget($"{TutorialTrigger.CreatureHealth}_{GetOwner()}_{RuntimeGameObject.Data.Id}").SetTransitionFactorSize().Init();
			AttackStatImage.SetHintTarget($"{TutorialTrigger.CreatureAttack}_{GetOwner()}_{RuntimeGameObject.Data.Id}").SetTransitionFactorSize().Init();
			selfContainer.SetHintTarget($"{TutorialTrigger.CreatureBounds}_{GetOwner()}_{RuntimeGameObject.Data.Id}").SetTransitionFactorSize().Init();
		}

			private void CacheCompactStatOffsets()
			{
			if (compactStatOffsetsCached)
				return;

			if (AttackStatImage != null)
			{
				attackStatOriginalPosition = AttackStatImage.rectTransform.anchoredPosition;
				attackStatOriginalScale = AttackStatImage.rectTransform.localScale;
			}

			if (HealthStatImage != null)
			{
				healthStatOriginalPosition = HealthStatImage.rectTransform.anchoredPosition;
				healthStatOriginalScale = HealthStatImage.rectTransform.localScale;
			}

			if (ArmorStatImage != null)
			{
				armorStatOriginalPosition = ArmorStatImage.rectTransform.anchoredPosition;
				armorStatOriginalScale = ArmorStatImage.rectTransform.localScale;
			}

				compactStatOffsetsCached = true;
			}

			private void ConfigureCompactStatCanvases()
			{
				if (compactStatCanvasConfigured)
					return;

				attackStatCanvas = EnsureStatCanvas(AttackStatImage, 200);
				healthStatCanvas = EnsureStatCanvas(HealthStatImage, 201);
				armorStatCanvas = EnsureStatCanvas(ArmorStatImage, 202);
				compactStatCanvasConfigured = true;
			}

			private static Canvas EnsureStatCanvas(Component statComponent, int sortingOrder)
			{
				if (!statComponent)
					return null;

				var canvas = statComponent.GetComponent<Canvas>();
				if (!canvas)
					canvas = statComponent.gameObject.AddComponent<Canvas>();

				canvas.overrideSorting = true;
				canvas.sortingOrder = sortingOrder;
				return canvas;
			}

			private void ApplyCompactStatLayout()
			{
				if (!compactStatOffsetsCached)
					return;

				var compact = BoardLayoutSettings.IsCompact();
				var minimal = BoardLayoutSettings.IsMinimal();
				// Minimal: nudge stats outward so they don't overlap card art or neighbors
			// Attack is bottom-LEFT (orig x:-95), Health is bottom-RIGHT (orig x:+95)
			var attackOffset = compact ? new Vector2(-12f, -10f) : Vector2.zero;
			var healthOffset = compact ? new Vector2(12f, -10f) : Vector2.zero;
			var armorOffset = compact ? new Vector2(-8f, 4f) : Vector2.zero;
			var scale = compact ? 0.78f : 1f;
				var statScale = Vector3.one * scale;
				ApplyStatCanvasState(attackStatCanvas, compact, 200);
				ApplyStatCanvasState(healthStatCanvas, compact, 201);
				ApplyStatCanvasState(armorStatCanvas, compact, 202);

				if (AttackStatImage != null)
				{
					AttackStatImage.rectTransform.SetAsLastSibling();
					AttackStatImage.rectTransform.anchoredPosition = attackStatOriginalPosition + attackOffset;
					AttackStatImage.rectTransform.localScale = Vector3.Scale(attackStatOriginalScale, statScale);
				}

				if (HealthStatImage != null)
				{
					HealthStatImage.rectTransform.SetAsLastSibling();
					HealthStatImage.rectTransform.anchoredPosition = healthStatOriginalPosition + healthOffset;
					HealthStatImage.rectTransform.localScale = Vector3.Scale(healthStatOriginalScale, statScale);
				}

				if (ArmorStatImage != null)
				{
					ArmorStatImage.rectTransform.SetAsLastSibling();
					ArmorStatImage.rectTransform.anchoredPosition = armorStatOriginalPosition + armorOffset;
					ArmorStatImage.rectTransform.localScale = Vector3.Scale(armorStatOriginalScale, statScale);
				}
			}

			private static void ApplyStatCanvasState(Canvas canvas, bool compact, int sortingOrder)
			{
				if (!canvas)
					return;

				canvas.overrideSorting = compact;
				canvas.sortingOrder = compact ? sortingOrder : 0;
			}

		protected override void OnDisabled()
		{
			base.OnDisabled();
			if (RuntimeData != null)
			{
				if (RuntimeData.Hp != null)
					RuntimeData.Hp.OnChangedFrom -= OnHpChanged;

				if (RuntimeData.Attack != null)
					RuntimeData.Attack.OnChangedFrom -= OnAttackChanged;

				if (RuntimeData.Armor != null)
					RuntimeData.Armor.OnChangedFrom -= OnArmorChanged;
			}

			updateFace?.Cancel();
			updateFace?.Dispose();
			updateFace = null;
			
			if (currentFaceId.HasValue)
			{
				TurnGlowImage.ReleaseResource();
				TargetingGlowImage.ReleaseResource();
				SelectionGlowImage.ReleaseResource();
				ArtMaskImage.ReleaseResource();
				ArtBorderImage.ReleaseResource();
			}
			
			if (previouseCommonRelease)
			{
				switch (loadedArtType)
				{
					case ArtType.Regular: ArtImage.ReleaseResource(); 
						break;
					case ArtType.Full: FullArtImage.ReleaseResource(); 
						break;
					default: 
						FullArtImage.ReleaseResource(); 
						ArtImage.ReleaseResource(); 
						break;
				}
				
				AttackStatImage.ReleaseResource();
				HealthStatImage.ReleaseResource();
				ArmorStatImage.ReleaseResource();
			}
			
			faceIdStack.Clear();
			currentFaceId = null;
			previouseCommonRelease = false;
			IsAllowedExternal = false;
			loadedArtType = null;
		}

		protected UniTask LoadMainArt(ArtType artType, string url, CancellationToken token)
		{
			loadedArtType = artType;
			switch (artType)
			{
				case ArtType.Regular:
					SetActive(FullArtImage, false);
					SetActive(ArtImage, true);
					if (previouseCommonRelease)
						FullArtImage.ReleaseResource();
					
					return ArtImage.LoadResourceAsync(url, token, previouseCommonRelease);
				
				case ArtType.Full:
					SetActive(FullArtImage, true);
					SetActive(ArtImage, false);
					if (previouseCommonRelease)
						ArtImage.ReleaseResource();
					
					return FullArtImage.LoadResourceAsync(url, token, previouseCommonRelease);
				
				default: throw new NotImplementedException($"Unknown {nameof(ArtType)} : {artType}");
			}
		}

		protected virtual void UpdateStatVisual(TextMeshProUGUI text, IntStat stat)
		{
			if (stat == null)
				throw new NullReferenceException("Int stat does not exist");
			
			if (!text)
			{
				RRLogger.Error($"Text for stat is missing");
				return;
			}

			if (!Settings)
			{
				RRLogger.Error($"cannot apply stat colors : Settings is missing");
				return;
			}
			
			var max = stat.TotalMax;
			var maxDefault = stat.BaseStat;
			
			var debuffed = max < maxDefault;
			var buffed = max > maxDefault && stat > maxDefault;
			
			var color = buffed ? Settings.BuffedColor
				: debuffed ? Settings.DebuffedColor
				: Settings.DefaultTextColor;
			
			Set(text, color);
		}

		protected virtual void OnArmorChanged(int from, int to)
		{
			if (IsAllowedExternal)
				return;
			
			SetActiveArmor(to > 0);
			SetArmor(to);
		}

		protected virtual void OnAttackChanged(int from, int to)
		{
			if (IsAllowedExternal)
				return;
			
			SetAttack(to);
		}

		protected virtual void OnHpChanged(int from, int to)
		{
			if (IsAllowedExternal)
				return;
			
			SetHealth(to);
		}
	}
}
