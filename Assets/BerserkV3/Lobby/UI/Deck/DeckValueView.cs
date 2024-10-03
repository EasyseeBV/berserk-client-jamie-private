using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.Lobby.UI
{
	public interface IDeckValueView
	{
		GameObject TargetView { get; }
		void Show(bool noAnimation = true);
		void Close(bool noAnimation = true);
		void SetValueText(string value);
	}
	
	public partial class DeckValueView : BaseView, IDeckValueView
	{
		public GameObject TargetView => gameObject;
		
		public void Show(bool noAnimation = true)
		{
			base.Show(noAnimation : noAnimation);
		}

		public void Close(bool noAnimation = true)
		{
			base.Close(noAnimation : noAnimation);
		}

		public void SetValueText(string value)
		{
			DeckValueText.SetText(value);
		}
	}
}