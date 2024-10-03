using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.Cards
{
	public interface ICardView : IRuntimeObjectView
	{
		// Properies
		TargetTransform TargetTransform { get; set; }
		new IRuntimeGameCard RuntimeGameObject { get; }
		new IRuntimeCardData RuntimeData { get; }
		ICardStrategy Strategy { get; }
		IRuntimeLayout Layout { get; }
		IGlowView GlowView { get; }
		
		// State
		bool IsSelf { get; }
		bool IsLocked { get; }
		bool MarkedAsSelected { get; }

		// Commands
		void MarkAsSelected(bool value = true);
		void SetSize(float value);
		void SetLock(bool value);
		void SetLocalState(RuntimeState value);
		void Refresh();
	}
}