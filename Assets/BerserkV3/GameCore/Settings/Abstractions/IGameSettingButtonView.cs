using System;

namespace BerserkV3.GameCore.Settings
{
	public interface IGameSettingButtonView
	{
		event Action OnClick;
	}
}