using System;
using BerserkV3.GameCore.Settings;
using RR.Core.Extensions;
using RR.UI.FrameSystem;

namespace BerserkV3.GameCore.UI
{
	public partial class GameSettingButtonView : BaseView, IGameSettingButtonView
	{
		public event Action OnClick;

		protected override void OnAwake()
		{
			base.OnAwake();
			Subscribe(Button, () => OnClick?.Invoke());
		}

		private void OnDestroy()
		{
			if(Button.Value())
				Button.onClick.RemoveAllListeners();

			OnClick = null;
		}
	}
}