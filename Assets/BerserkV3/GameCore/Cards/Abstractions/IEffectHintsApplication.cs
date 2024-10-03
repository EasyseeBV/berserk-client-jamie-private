using System;
using Berserk.Shared.GameCore.Abstraction;
using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.Cards
{
	public interface IEffectHintsApplication : IDisposable
	{
		void Setup(IRuntimeGameObject runtimeGameObject, IRuntimeObjectView view);
		void Show();
		void Hide();
	}
}