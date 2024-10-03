namespace BerserkV3.GameCore.Cards
{
	public interface ICardStrategy
	{
		bool Enabled { get; }
		bool IsAllowedExternal { get; }
		bool IsSelectionAllowed { get; }
		bool IsDragAllowed { get; }
		bool IsHoverAllowed { get; }
		bool IsPreviewAllowed { get; }
		ICardView View { get; }
		
		void Enable();
		void Disable();
		void Refresh();
		
		ICardStrategy AllowExternal(bool value);
		ICardStrategy AllowPreview(bool value);
		ICardStrategy AllowHover(bool value);
		ICardStrategy AllowSelection(bool value);
		ICardStrategy AllowDrag(bool value);
	}
}