using System;
using RR.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BerserkV3.Lobby.UI
{
	public class LobbySettingsButtonWindow : UIWindowBase
	{
		[SerializeField] private Button button;

		public override void Hidden()
		{
			Clear();
			base.Hidden();
		}

		protected override void OnDisposed()
		{
			Clear();
			base.OnDisposed();
		}

		public void SetClickAction(Action value)
		{
			if (button)
				button.onClick.AddListener(() => value?.Invoke());
		}

		public void Clear()
		{
			if (button)
				button.onClick.RemoveAllListeners();
		}
	}
}