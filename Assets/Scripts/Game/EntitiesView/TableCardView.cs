using DG.Tweening;
using RR.Core.DebugSystem;
using System;
using System.Linq;
using Audio;
using Berserk.Shared.Data.Enums;
using Cysharp.Threading.Tasks;
using RR.Core.ResourceManagament;
using RR.Core.Extensions;
using RR.Core.Serialization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Vulcan.Audio;
using Vulcan.Data;
using Vulcan.VFX;
using CardData = Vulcan.Data.CardData;

namespace UI
{
	public enum TableCardFaceId
	{
		Default,
		Taunt
	}
	public partial class TableCardView : EntityView, IPointerExitHandler, IPointerEnterHandler
	{
		[Serializable]
		private class CardFaceDictionary: UnitySerializedDictionary<TableCardFaceId, CardFace> { }
		[Serializable]
		private struct CardFace
		{
			public Sprite Mask;
			public Sprite Border;
		}
		
		public event Action OnSpawned;
		private static readonly float ANIM_DURATION = 0.35f;

		[SerializeField] private CardFaceDictionary _cardFaces;
		[SerializeField] private Color buffStatColor = default;
		[SerializeField] private Color debuffStatColor = default;

		private Tween hoverTween;
		private Tween moveTween;
		private bool spawned;
		private int sibling;

		private CardData cardData;

		public CardTooltipPanel CardTooltipPanel => TooltipPanel;

		public override void SetUp(DataBase data)
		{
			base.SetUp(data);
			Unselect();
			cardData = (CardData)data;
			
			UpdateImage();
			UpdateValues();
			
			transform.SetAsFirstSibling();
			spawned = false;
		}

		private void UpdateValues()
		{
			if(Data == null) return;
			UpdateStat(CardStatAtkTxt, Data.Attack);
			UpdateStat(CardStatHPTxt, Data.Hp);
			UpdateFace(GetFaceIdByEffectPriority());
			TooltipPanel
				.SetUp(Data.EffectsContainer.Values.Select(x => x.EffectData.EffectDescription).ToArray())
				.AttachTo(TooltipHolder, Vector3.zero)
				.Hide(noAnimation: true);
		}

		private void UpdateImage()
		{
			if (Data == null)
				return;
			CarArtImg.LoadResourceAsync(Data.ArtUrl).Forget();
		}

		public override void SetPosition(Vector3 pos, Quaternion rot)
		{
			base.SetPosition(pos, rot);
			var ease = spawned ? Ease.InOutCubic : Ease.InExpo;

			moveTween?.Kill();

			moveTween = transform.DOLocalMove(pos, 0.75f).SetEase(ease).OnComplete(() =>
			{
				moveTween = null;
				if (spawned)
					return;
				VFXController.Spawn(transform.position, EffectVisualKeyword.CardSpawn, Data.Id);
				AudioController.Play(Clip.TableCard_Spawn);
				spawned = true; // before action to avoid code-flow immediately called twice SetPosition..
				OnSpawned?.Invoke();
			});
		}

		protected override void SubscribeOnDataChange()
		{
			Data.Attack.OnChangedFrom += OnAttackChanged;
			Data.Hp.OnChangedFrom += OnHpChanged;
		}

		protected override void UnsubscribeOnDataChange()
		{
			Data.Attack.OnChangedFrom -= OnAttackChanged;
			Data.Hp.OnChangedFrom -= OnHpChanged;
		}

		public override void Select(Color selectedColor)
		{
			// General method EntityView
			TargetImg.gameObject.SetActive(true);
			TargetImg.color = selectedColor;
		}

		public override void Unselect()
		{
			SetActive(TargetImg, false);
		}
		
		public void UpdateFace(TableCardFaceId faceIdId)
		{
			if (_cardFaces == null || !_cardFaces.ContainsKey(faceIdId))
			{
				RRLogger.Error("TableCardView.UpdateFace : "+ faceIdId.ToString().Red() + " does not represent in collection");
				return;
			}
			CardBorder.sprite = _cardFaces[faceIdId].Border;
			CardArtMask.sprite = _cardFaces[faceIdId].Mask;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (eventData.dragging || isAttacking)
				return;

			sibling = RectTransform.GetSiblingIndex();

			TooltipPanel.transform.SetParent(transform.root);
			TooltipPanel.transform.localScale = Vector3.one;
			TooltipPanel.gameObject.SetActive(true);

			FullCardView.SetPosition(HorizontalShift.Leftmost);
			// FullCardView.Show(cardData, isShowToolTip: false);

			transform.SetAsLastSibling();

			hoverTween?.Kill();

			hoverTween = DOTween.Sequence()
				.Join(transform.DOScale(1.35f, ANIM_DURATION))
				.Play().OnComplete(() => hoverTween = null);
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			FullCardView.Hide();
			hoverTween?.Kill();

			if (!gameObject)
				return;

			TooltipPanel.transform.SetParent(transform);
			TooltipPanel.gameObject.SetActive(false);
			transform.SetSiblingIndex(sibling);

			hoverTween = transform.DOScale(1f, ANIM_DURATION / 2f).OnComplete(() => hoverTween = null);
		}

		private void UpdateStat(TextMeshProUGUI text, CardStat stat, bool hasVFX = false, int from = int.MinValue, int to = int.MinValue)
		{
			bool animate = (from != to && from != int.MinValue);
			bool debuffed = stat.GetMax() < stat.GetDefault;
			bool buffed = stat.GetMax() > stat.GetDefault;

			if (animate)
			{
				if (hasVFX)
					ShowVFXHpChange(from, to);
				text.DOCounter(from, to, 0.75f);
			}
			else
				text.SetText(stat.ToString());

			if (buffed)
				text.color = buffStatColor;
			else if (debuffed)
				text.color = debuffStatColor;
			else
				text.color = Color.white;
		}

		protected virtual TableCardFaceId GetFaceIdByEffectPriority()
		{
			if (Data == null)
			{
				return TableCardFaceId.Default;
			}

			if (Data.EffectsContainer.Has(EffectKeyword.Taunting))
			{
				return TableCardFaceId.Taunt;
			}
			return TableCardFaceId.Default;
		}
		
		private void OnAttackChanged(int from, int to)
		{
			UpdateStat(CardStatAtkTxt, cardData.Attack, false, from, to);
		}

		private void OnHpChanged(int from, int to)
		{
			UpdateStat(CardStatHPTxt, cardData.Hp, true, from, to);
		}

		protected override void OnDeath(DataBase dataBase)
		{
			base.OnDeath(dataBase);

			if (TooltipPanel != null && TooltipPanel.isActiveAndEnabled)
			{
				TooltipPanel.transform.SetParent(transform);
				TooltipPanel.gameObject.SetActive(false);
			}
		}

		protected override void OnDisable()
		{
			base.OnDisable();

			if (TooltipPanel != null && TooltipPanel.isActiveAndEnabled)
			{
				TooltipPanel.transform.SetParent(transform);
				TooltipPanel.gameObject.SetActive(false);
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			CarArtImg.ReleaseResource();

			if (Data != null)
			{
				Data.Attack.OnChangedFrom -= OnAttackChanged;
				Data.Hp.OnChangedFrom -= OnHpChanged;
			}
		}
	}
}