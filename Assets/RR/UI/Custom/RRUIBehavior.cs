using RR.Core.Extensions;

namespace RR.UI.Custom
{
	public class RRUIBehavior : RRBehavior
	{

#if UNITY_EDITOR

		[Sirenix.OdinInspector.Button(Sirenix.OdinInspector.ButtonSizes.Medium), Sirenix.OdinInspector.PropertySpace()]
		private void AddPngLoader()
		{
			gameObject.GetOrAddComponent("RR.Network.PNGLoader");
		}
#endif
	}
}