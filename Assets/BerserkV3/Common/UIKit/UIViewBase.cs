using RR.UIService;
using UnityEngine;

namespace BerserkV3.Common.UIKit
{
	/// <summary>
	///   <para> UIViewBase is a base class for all UI elements - views, widgets etc..</para>
	/// </summary>
	public class UIViewBase : MonoBehaviour
	{
		public virtual void SetActive(bool value)
		{
			gameObject.SetActive(value);
		}
		
		//TODO Add alpha control and gray control here when it will be required
		
#if UNITY_EDITOR
	    
		[ContextMenu(nameof(AutoAssignFields))]
		private void AutoAssignFields()
		{
			AutoComponentAssigner.AutoAssignComponents(this);
		}

		[ContextMenu(nameof(ClearAssignedFields))]
		private void ClearAssignedFields()
		{
			AutoComponentAssigner.ClearAssignedFields(this);
		}
#endif
	}
}