using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.UI;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public abstract class BaseCardLayout : RuntimeObjectView, IRuntimeLayout
	{
		private bool isSelf;
		
		protected RuntimeLayoutSettings Settings { get; private set; }
		
		public Vector3 DefaultScale { get; private set; }
		
		public IGlowView GlowView { get; private set; }

		public bool IsSelf
		{
			get => isSelf;
			set
			{
				isSelf = value;
				GlowView?.Setup(value);
			}
		}
		
		protected override void OnAwake()
		{
			base.OnAwake();
			GlowView = GetComponent<IGlowView>() ?? new GlowViewMock();
			DefaultScale = selfContainer.localScale;
			Settings = Resources.Load<RuntimeLayoutSettings>(nameof(RuntimeLayoutSettings));
		}
		
		public void SetInteractable(bool value)
		{
			CanvasGroup.interactable = value;
			CanvasGroup.blocksRaycasts = value;
		}

		public void SetAlpha(float value)
		{
			CanvasGroup.alpha = value;
		}

		public virtual void Refresh() {}

		public float GetAlpha()
		{
			return CanvasGroup.alpha;
		}

		public virtual void SetLavaTextColor(Color color) {}

		private void OnDestroy()
		{
			Disable();
		}

		protected override void OnDisabled()
		{
			base.OnDisabled();
			if (selfContainer)
				selfContainer.localScale = DefaultScale;
			
			GlowView?.Disable();
		}

		protected Owner GetOwner()
		{
			return isSelf ? Berserk.Shared.Data.Enums.Owner.Self : Berserk.Shared.Data.Enums.Owner.Opponent;
		}
	}
}