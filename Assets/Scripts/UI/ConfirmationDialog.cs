using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using BerserkV3.Common.Utils;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;

namespace UI
{
	public partial class ConfirmationDialog : BaseView
	{
		public static LinkedList<ConfirmationDialogData> dialogStack = new LinkedList<ConfirmationDialogData>();

		public static ConfirmationDialogData currentDialogData;
		
		
		/// <summary>
		/// <para>To apply modifications on ConfirmationDialogData use ConfirmationDialogData.Apply()</para>
		/// <para>To get key use Key from ConfirmationDialogData</para>
		/// <para>By Default: All buttons enabled</para>
		/// <para>OKText : "Ok"</para>
		/// <para>CancelText : "Cancel"</para>
		/// <para>Title : "Are you sure?"</para>
		/// <para>Message : " "</para>
		/// </summary>
		
		public ConfirmationDialogData Init([CallerMemberName] string key = "")
		{
			key = $"{key}_{Guid.NewGuid()}";


			ConfirmationDialogData dialogData = new(key);
			
			OpenDialogData(dialogData);
			
			
			return dialogData;
		}

		public ConfirmationDialogData GetCurrentDialogData()
		{
			return currentDialogData;
		}

		public void DisplayDialogData(ConfirmationDialogData data)
		{
			Clear();
			
			
			SetTitle(data.Title);
			SetMessage(data.Message);
			SetOk(data.OkText);
			SetCancel(data.CancelText);
			SetClose(data.CloseText);
			SetOkColor(data.OkColor);
			SetCancelColor(data.CancelColor);
			SetCloseColor(data.CloseColor);
			SetResponse(data.Response);
			SetResponseOk(data.OkResponse);
			SetResponseCancel(data.CancelResponse);
			SetResponseClose(data.CloseResponse);
			SetAnyResponse(data.AnyResponse);
			
			CancelBtn.Subscribe(() => Close(data.Key));
			OkBtn.Subscribe(() => Close(data.Key));
			CloseBtn.Subscribe(() => Close(data.Key));

			
			Show();
		}

		private void OpenDialogData(ConfirmationDialogData dialogData)
		{
			if (currentDialogData != null)
			{
				dialogData.OnValueUpdate -= DisplayDialogData;
				dialogStack.Push(currentDialogData);
			}
            
			currentDialogData = dialogData; 
			dialogData.OnValueUpdate += DisplayDialogData;
			DisplayDialogData(dialogData);
		}
		
		
		private ConfirmationDialog SetMessage(string value)
		{
			Set(BodyMessageTxt, value);
			return this;
		}
		
		private ConfirmationDialog SetResponse(Action<bool> value)
		{
			CancelBtn.Subscribe(() => value?.Invoke(false));
			OkBtn.Subscribe(() => value?.Invoke(true));
			return this;
		}
		
		private ConfirmationDialog SetResponseOk(Action value)
		{
			OkBtn.Subscribe(() => value?.Invoke());
			return this;
		}
		
		private ConfirmationDialog SetResponseClose(Action value)
		{
			CloseBtn.Subscribe(() => value?.Invoke());
			return this;
		}
		
		private ConfirmationDialog SetResponseCancel(Action value)
		{
			CancelBtn.Subscribe(() => value?.Invoke());
			return this;
		}
		
		private ConfirmationDialog SetAnyResponse(Action value)
		{
			CancelBtn.Subscribe(() => value?.Invoke());
			OkBtn.Subscribe(() => value?.Invoke());
			CloseBtn.Subscribe(() => value?.Invoke());
			return this;
		}
		
		private ConfirmationDialog SetTitle(string value)
		{
			Set(TitleText, value);
			return this;
		}
		
		/// <summary>
		/// Empty or null will disable the button
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private ConfirmationDialog SetOk(string value = null)
		{
			SetActive(OkBtn, !string.IsNullOrEmpty(value));
			Set(OkBtn, value);
			return this;
		}
		
		/// <summary>
		/// Empty or null will disable the button
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private ConfirmationDialog SetCancel(string value = null)
		{
			SetActive(CancelBtn, !string.IsNullOrEmpty(value));
			Set(CancelTxt, value);
			return this;
		}
		
		/// <summary>
		/// Empty or null will disable the button
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		private ConfirmationDialog SetClose(string value = null)
		{
			SetActive(CloseBtn, !string.IsNullOrEmpty(value));
			Set(CloseTxt, value);
			return this;
		}

		private ConfirmationDialog SetOkColor(Color value)
		{
			OkBtn.targetGraphic.color = value;
			return this;
		}

		private ConfirmationDialog SetCancelColor(Color value)
		{
			CancelBtn.targetGraphic.color = value;
			return this;
		}

		private ConfirmationDialog SetCloseColor(Color value)
		{
			CloseBtn.targetGraphic.color = value;
			return this;
		}

		protected override void OnClosed()
		{
			base.OnClosed();
			Clear();
		}

		protected override void OnHidden()
		{
			base.OnHidden();
			Clear();
		}

		private void OnDestroy()
		{
			Clear();
		}

		private void Clear()
		{
			OkBtn.onClick.RemoveAllListeners();
			CancelBtn.onClick.RemoveAllListeners();
			CloseBtn.onClick.RemoveAllListeners();
		}
		
		public void Close(string key, Action onAnimationDone = null, bool noAnimation = false, bool concurrentAnimation = false)
		{
			if (currentDialogData != null && currentDialogData.Key == key)
			{
				currentDialogData = null;
				if(dialogStack.TryPop(out var newData))
				{
					onAnimationDone?.Invoke();
					OpenDialogData(newData);
				}
				else
				{
					Close(onAnimationDone, noAnimation, concurrentAnimation);
				}
			}
			else
			{
				dialogStack.Remove(dialogStack.FirstOrDefault(x => x.Key == key));
			}
		}
	}

	public class ConfirmationDialogData
	{
		public readonly string Key;

		public ConfirmationDialogData(string key)
		{
			Key = key;
		}

		public Action<ConfirmationDialogData> OnValueUpdate;

		public string Title { get; private set; } = "Are you sure?";
		public string Message { get; private set; }
		public string OkText { get; private set; } = "Ok";
		public string CancelText { get; private set; } = "Cancel";
		public string CloseText { get; private set; }
		public Color OkColor { get; private set; } = Color.green;
		public Color CancelColor { get; private set; } = Color.white;
		public Color CloseColor { get; private set; } = Color.red;
		public Action<bool> Response { get; private set; }
		public Action AnyResponse { get; private set; }
		public Action OkResponse { get; private set; }
		public Action CancelResponse { get; private set; }
		public Action CloseResponse { get; private set; }
		
		public ConfirmationDialogData SetMessage(string value)
		{
			Message = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		public ConfirmationDialogData SetResponse(Action<bool> value)
		{
			Response = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		public ConfirmationDialogData SetResponseOk(Action value)
		{
			OkResponse = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		public ConfirmationDialogData SetResponseClose(Action value)
		{
			CloseResponse = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		public ConfirmationDialogData SetResponseCancel(Action value)
		{
			CancelResponse = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		public ConfirmationDialogData SetAnyResponse(Action value)
		{
			AnyResponse = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		public ConfirmationDialogData SetTitle(string value)
		{
			Title = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		/// <summary>
		/// Empty or null will disable the button
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public ConfirmationDialogData SetOk(string value = null)
		{
			OkText = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		/// <summary>
		/// Empty or null will disable the button
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public ConfirmationDialogData SetCancel(string value = null)
		{
			CancelText = value;
			//OnValueUpdate?.Invoke();
			return this;
		}
		
		/// <summary>
		/// Empty or null will disable the button
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public ConfirmationDialogData SetClose(string value = null)
		{
			CloseText = value;
			//OnValueUpdate?.Invoke();
			return this;
		}

		public ConfirmationDialogData SetOkColor(Color value)
		{
			OkColor = value;
			//OnValueUpdate?.Invoke();
			return this;
		}

		public ConfirmationDialogData SetCancelColor(Color value)
		{
			CancelColor = value;
			//OnValueUpdate?.Invoke();
			return this;
		}

		public ConfirmationDialogData SetCloseColor(Color value)
		{
			CloseColor = value;
			//OnValueUpdate?.Invoke();
			return this;
		}

		/// <summary>
		/// If this data is displayed, updates view
		/// </summary>
		public void Apply()
		{
			OnValueUpdate?.Invoke(this);
		}
	}
}