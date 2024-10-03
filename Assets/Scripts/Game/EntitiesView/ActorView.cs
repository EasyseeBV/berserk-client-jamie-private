using System;
using System.Linq;
using Audio;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events;
using RR.Core.ResourceManagament;
using RR.Core.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vulcan.Audio;
using Vulcan.Data;
using Vulcan.Network.Context;

namespace UI
{
	public partial class ActorView : EntityView
	{
		[SerializeField] private Button avatarButton;
		private Color buttonDisabledColor;
		private Color buttonNormalColor;

		private Vector3 initScale;
		private Vector3 initPosition;

		public event Action OnAbilityButtonActivated;
		private Tweener animationTween;

		protected override void OnAwake()
		{
			base.OnAwake();
			buttonNormalColor = TextTMP.color;
			buttonDisabledColor = buttonNormalColor;
			buttonDisabledColor.a = 0.5f;
			InitAbilityButton();
		}

		public override void SetUp(DataBase data)
		{
			base.SetUp(data);

			SetActive(Avatar_Panel, Data.Hp > 0);
			Set(NameTxt, GetAnonymousName());
			
			var actorData = data as ActorData;
			BordersImage.LoadResourceAsync(actorData?.AvatarBorderUrl).Forget();
			AvatarImage.LoadResourceAsync(data.ArtUrl).Forget();
			
			UpdateArmor(0);
			position = entityTransform.localPosition;
			initScale = Avatar_Panel.localScale;
			initPosition = Avatar_Panel.localPosition;
			avatarButton.onClick.RemoveAllListeners();

			if (data.Owner == Berserk.Shared.Data.Enums.Owner.Self)
			{
				// avatarButton.onClick.AddListener(() => EmotionsFieldView.Instance.Build());
				return;
			}

			BotIndicator.enabled = ActorsContextResolver.Opponent.IsControlledByAI;
			GameBus.OpponentDisconnected.Subscribe(this, flag => DisconnectIndicator.enabled = flag);

			return;

			string GetAnonymousName()
			{
				var player = ActorsContextResolver.GetPlayer(data.Owner);
				var selfPlayer = ActorsContextResolver.GetPlayer(Berserk.Shared.Data.Enums.Owner.Self);
				var anonymousName = string.IsNullOrEmpty(data.Title) ||
				                    data.Owner == Berserk.Shared.Data.Enums.Owner.Opponent && data.Title.Same(selfPlayer.UserName)
					? "Ricardo-bot-" + player.UserName.ToMd5Hash()
					: data.Title;

				var removeCharIndex = data.Owner == Berserk.Shared.Data.Enums.Owner.Self
					? 19
					: 11;

				return anonymousName.Length > removeCharIndex
					? anonymousName.Remove(removeCharIndex)
					: anonymousName;
			}
		}

		public override void Select(Color selectedColor)
		{
			// General method EntityView
			SetActive(TargetImg, true);
			Set(TargetImg, selectedColor);
		}

		public override void Unselect()
		{
			// General method EntityView
			SetActive(TargetImg, false);
		}

		public void UpdateArmor(int value)
		{
			SetActive(ArmorContainer, value > 0);
			Set(ArmorValueText, value.ToString());
		}

		public override void SelectOnRequestingTarget(Color color)
		{
			Set(AvatarShineImg, color);
		}

		public void OnTurnChange(bool myTurn)
		{
			SetActive(AvatarShineImg, myTurn);
		}

		public void OnDie()
		{
			SetActive(Avatar_Panel, false);
		}

		protected override void SubscribeOnDataChange()
		{
			Data.Hp.OnChangedFrom += OnHpChanged;
			Data.Attack.OnChanged += OnAttackChanged;
			Data.Lava.OnChanged += OnManaChanged;
		}

		protected override void UnsubscribeOnDataChange()
		{
			Data.Hp.OnChangedFrom -= OnHpChanged;
			Data.Attack.OnChanged -= OnAttackChanged;
			Data.Lava.OnChanged -= OnManaChanged;
		}

		protected override void TakeHitAnimation()
		{
			animationTween?.Kill();
			animationTween = Avatar_Panel
				.DOShakePosition(0.75f, 12)
				.OnComplete(() => Avatar_Panel.localPosition = initPosition)
				.OnKill(() => Avatar_Panel.localPosition = initPosition);
			AudioController.Play(Clip.TableCard_Damage);
		}

		protected override void TakeHealAnimation()
		{
			animationTween?.Kill();
			animationTween = Avatar_Panel
				.DOScale(0.75f, .75f)
				.SetLoops(2, LoopType.Yoyo)
				.OnComplete(() => Avatar_Panel.localScale = initScale)
				.OnKill(() => Avatar_Panel.localScale = initScale);
			AudioController.Play(Clip.TableCard_Heal);
		}

		private void OnAttackChanged(int attack)
		{
			SetActive(CardStat_AtkImg, attack > 0);
			Set(CardStatAtkTxt, attack.ToString());
		}

		private void OnHpChanged(int from, int to)
		{
			UpdateStat(HPValueText, true, from, to);
		}

		private void OnManaChanged(int value)
		{
			if (Data.Owner == Berserk.Shared.Data.Enums.Owner.Self)
				PlayerManaPanel.VisualizeMana(value, Data.Lava.GetMax());
			else
				OpponentManaPanel.VisualizeMana(value, Data.Lava.GetMax());
		}

		private void UpdateStat(TextMeshProUGUI text, bool hasVFX = false, int from = int.MinValue,
			int to = int.MinValue)
		{
			var animate = from != to && from != int.MinValue;

			if (animate && hasVFX)
				ShowVFXHpChange(from, to);

			Set(text, to.ToString());
		}

		private void InitAbilityButton()
		{
#if UNITY_ANDROID || UNITY_IOS // mobile platforms with touch
			var abilityButtonlongPress = AbilityButton.gameObject.AddComponent<ButtonLongPress>();
			abilityButtonlongPress.SubscribeOnClick(() =>
			{
				if (AbilityButton.interactable)
				{
					OnAbilityButtonActivated?.Invoke();
				}
			});
			abilityButtonlongPress.SubscribeOnLongPress(() =>
			{
				TooltipPanel.SetPositionForMobile(SystemInfo.deviceType == DeviceType.Handheld);
				TooltipPanel.Show();
			});
			abilityButtonlongPress.SubscribeOnLongPressFinished(() =>
			{
				TooltipPanel.Hide();
			});

#else // desktop platforms with mouse

			AbilityButton.Subscribe(() => OnAbilityButtonActivated?.Invoke());
			var abilityEventTrigger = AbilityButton.GetComponent<EventTrigger>();

			abilityEventTrigger.triggers.First(t => t.eventID == EventTriggerType.PointerEnter)
				.callback.AddListener(_ =>
				{
					TooltipPanel.SetPositionForMobile(SystemInfo.deviceType == DeviceType.Handheld);
					TooltipPanel.Show();
				});

			abilityEventTrigger.triggers.First(t => t.eventID == EventTriggerType.PointerExit)
				.callback.AddListener(_ => TooltipPanel.Hide());

#endif
		}

		public void AdjustAbilityButton(EffectData effect, bool canUse)
		{
			var hasEffect = effect != null;
			SetActive(AbilityButton, hasEffect);
			SetAbilityInteractable(canUse);

			if (!hasEffect) return;
			var spellText = effect.Phase != EffectPhase.OnAbilityButtonPress ? "P" : "A";
			Set(TextTMP, spellText);
			TooltipPanel.SetUp(effect.EffectDescription);
		}

		public void SetAbilityInteractable(bool value)
		{
			SetInteractable(AbilityButton, value);
			Set(TextTMP, value
				? buttonNormalColor
				: buttonDisabledColor);
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			AvatarImage.ReleaseResource();
			BordersImage.ReleaseResource();

			if (Data != null)
			{
				Data.Hp.OnChangedFrom -= OnHpChanged;
				Data.Attack.OnChanged -= OnAttackChanged;
				Data.Lava.OnChanged -= OnManaChanged;
			}
		}
	}
}