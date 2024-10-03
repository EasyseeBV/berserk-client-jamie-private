using Berserk.Shared.Data.Abstraction;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.TooltipPopup
{
	public interface ITooltipPopupDoubleSidedController
	{
		public UniTask DisplayAsync(IRuntimeData runtimeData, RectTransform container);

		public void Close();
	}
}