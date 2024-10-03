using System.Collections;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public interface IReconnectionView
	{
		void Show();
		void Close();
	}
	
	public partial class ReconnectionView : BaseView, IReconnectionView
	{
		private const string MESSAGE = "Connection";
		private const string TITLE_MESSAGE = "Information";
		private Coroutine timer;
		private bool showed;

		protected override void OnAwake()
		{
			base.OnAwake();
			Set(TitleText, TITLE_MESSAGE);
			Set(ContentText, MESSAGE);
			showed = ShowAtStart;
		}

		protected override void OnShown()
		{
			base.OnShown();
			
			if (showed)
				return;
			
			showed = true;
			
			if (timer != null)
				StopCoroutine(timer);
			
			timer = StartCoroutine(Timer());
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			
			if (!showed)
				return;
			
			showed = false;
			if (timer != null)
				StopCoroutine(timer);
			
			timer = null;
		}

		private IEnumerator Timer()
		{
			var maxDots = 3;
			while (Application.isPlaying)
			{
				Set(DotsText, new string('.', ((int)Time.time % maxDots) +1));
				yield return null;
			}
		}
	}
}