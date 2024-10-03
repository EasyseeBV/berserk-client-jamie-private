using Events;
using Game.Entities;
using RR.Core.DebugSystem;
using System;
using Audio;
using UnityEngine;
using Vulcan.Audio;

namespace Game.Effect_System.Target
{
	[RequireComponent(typeof(ManualTargetPickerView))]
	public class ManualTargetPicker : MonoBehaviour
	{
		private ManualTargetPickerView view;

		private bool isDragging;
		
		public event Action OnSelectionEnd;
		
		public event Action OnSelectionCancel;

		private void Awake()
		{
			view = GetComponent<ManualTargetPickerView>();

			GameBus.OnPassingTurned.Subscribe(this, Cancel);
			GameBus.CurrentRound.Subscribe(this, Cancel);
		}

		private void LateUpdate()
		{
			view.Drag();

			// delay control until the player presses the button to dragging
			if (!isDragging && !IsHolding() && !IsHolding(1))
				return;

			isDragging = true;

			if (IsHolding(1))
			{
				Cancel();
				return;
			}

			if (IsHolding())
				return;
			
			Stop();
			OnSelectionEnd?.Invoke();
		}

		public void SetUp(IMonoEntity owner)
		{
			isDragging = false;
			view.SetUp(owner);

			AudioController.Play(Clip.Arrow_Start);
			RRLogger.Log("Arrow init drag");
		}

		private void Stop()
		{
			if(!view)
				return;
			
			isDragging = false;
			view.Close();
			RRLogger.Log($"Arrow end drag");
		}

		public void Cancel()
		{
			Stop();
			OnSelectionCancel?.Invoke();
		}
		
		private bool IsHolding(int count = 0)
		{
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				return (Input.touchCount > count) && Input.GetTouch(0).phase <= TouchPhase.Stationary;
#endif

			return Input.GetMouseButton(count);
		}

		private bool UseTouchInput()
		{
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
				return true;
#else
			return false;
#endif
		}
	}
}