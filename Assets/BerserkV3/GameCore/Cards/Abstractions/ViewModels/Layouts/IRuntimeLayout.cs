using BerserkV3.GameCore.UI;
using UnityEngine;

namespace BerserkV3.GameCore.Cards
{
	public interface IRuntimeLayout : IRuntimeObjectView
	{
		Vector3 DefaultScale { get; }
		IGlowView GlowView { get; }
		bool IsSelf { get; set; }

		void SetInteractable(bool value);
		
		void SetAlpha(float value);

		void Refresh();
		
		float GetAlpha();
		void SetLavaTextColor(Color color);
	}
}