using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public interface ITableRaycastPanel
	{
		GameObject TargetView { get; }
	}
	
	public class TableRaycastPanel : MonoBehaviour, ITableRaycastPanel
	{
		public GameObject TargetView => gameObject;
	}
}