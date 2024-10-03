using UnityEngine;

namespace Vulcan.VFX.Custom
{
	public class AddMoveEffect : MonoBehaviour
	{
		private void OnEnable()
		{
			transform.SetSiblingIndex(0);
		}
	}
}