using Events;
using RR.UI.FrameSystem;
using UnityEngine;
using Vulcan.Data;

namespace UI
{
	public abstract class MonoEntityBaseView : BaseView
	{
		public abstract void SetUp(DataBase data);

		public abstract void Select(Color selectedColor);

		public abstract void Unselect();

		public abstract void SelectOnRequestingTarget(Color requestingColor);

		public abstract void SetPosition(Vector3 position, Quaternion quaternion);

		protected override void OnAwake()
		{
			GameBus.OpponentDisconnected.Subscribe(this, isDisconnected => CanvasGroup.blocksRaycasts = !isDisconnected).CallWhenInactive();
		}
	}
}