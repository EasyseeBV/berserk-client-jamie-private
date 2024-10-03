namespace BerserkV3.GameCore.Cards
{
	public abstract class BaseStrategy : ICardStrategy
	{
		public bool Enabled { get; private set; }
		public bool IsAllowedExternal { get; private set; }
		public bool IsSelectionAllowed { get; private set; }
		public bool IsDragAllowed { get; private set; }
		public bool IsHoverAllowed { get; private set; }
		public bool IsPreviewAllowed { get; private set; }
		public abstract ICardView View { get; set; }

		public void Enable()
		{
			if (Enabled)
				return;

			Enabled = true;
			OnEnabled();
		}

		public void Disable()
		{
			if (!Enabled)
				return;

			Enabled = false;
			IsAllowedExternal = false;
			OnDisabled();
		}

		public void Refresh()
		{
			if (IsAllowedExternal)
				return;
			
			OnRefreshed();
		}

		public ICardStrategy AllowExternal(bool value)
		{
			if (IsAllowedExternal == value)
				return this;

			IsAllowedExternal = value;
			OnAllowExternalChanged();
			return this;
		}
		
		public ICardStrategy AllowPreview(bool value)
		{
			if (IsPreviewAllowed == value)
				return this;
			
			IsPreviewAllowed = value;
			OnAllowPreviewChanged();
			return this;
		}

		public ICardStrategy AllowHover(bool value)
		{
			if (IsHoverAllowed == value)
				return this;
			
			IsHoverAllowed = value;
			OnAllowHoverChanged();
			return this;
		}

		public ICardStrategy AllowSelection(bool value)
		{
			if (IsSelectionAllowed == value)
				return this;
			
			IsSelectionAllowed = value;
			OnAllowSelectionChanged();
			return this;
		}

		public ICardStrategy AllowDrag(bool value)
		{
			if (IsDragAllowed == value)
				return this;
			
			IsDragAllowed = value;
			OnAllowDragChanged();
			return this;
		}

		protected virtual void OnEnabled() {}

		protected virtual void OnDisabled()
		{
			AllowHover(false);
			AllowExternal(false);
			AllowSelection(false);
			AllowPreview(false);
			AllowDrag(false);
		}
		protected virtual void OnRefreshed() {}
		protected virtual void OnAllowExternalChanged() {}
		protected virtual void OnAllowPreviewChanged() {}
		protected virtual void OnAllowHoverChanged(){}
		protected virtual void OnAllowSelectionChanged(){}
		protected virtual void OnAllowDragChanged(){}
	}
}