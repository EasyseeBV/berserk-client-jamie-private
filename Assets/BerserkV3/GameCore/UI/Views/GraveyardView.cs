using System;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public interface IGraveyardView
	{
		bool IsShowed { get; }
		
		public RectTransform OpponentContainer { get; }
		
		public RectTransform SelfContainer { get; }
		
		event Action OnCloseClick; 
		
		void Show();
		
		void Close();
	}
	
	public partial class GraveyardView : BaseView, IGraveyardView
	{
		public bool IsShowed => VisibleState == VisibleState.Visible;
		public RectTransform OpponentContainer => CardsOpponentContainer;
		public RectTransform SelfContainer => CardsSelfContainer;
		public event Action OnCloseClick;

		protected override void OnAwake()
		{
			CloseBtn.onClick.AddListener(() => OnCloseClick?.Invoke());
		}

		private void OnDestroy()
		{
			OnCloseClick = null;
			CloseBtn.onClick.RemoveAllListeners();
		}
	}
}