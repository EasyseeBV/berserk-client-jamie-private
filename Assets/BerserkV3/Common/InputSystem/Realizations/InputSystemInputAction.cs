using System;
using UnityEngine.InputSystem;

namespace BerserkV3.Common.InputSystem
{
	public class InputSystemInputAction : IInputAction, IDisposable
	{
		private readonly InputAction bindedAction;
		private readonly IInputContextProcessor contextProcessor;

		public bool Enabled { get; private set; }
		public string Id => bindedAction.name;
		public event Action<IInputContext> OnStarted;
		public event Action<IInputContext> OnPerformed;
		public event Action<IInputContext> OnCanceled;
		public event Action OnEnableChanged;

		public InputSystemInputAction(InputAction bindAction, IInputContextProcessor contextProcessor)
		{
			bindedAction = bindAction;
			this.contextProcessor = contextProcessor;
			if (bindedAction.enabled) // initialize if enabled
				Enable();
		}

		public TValue ReadValue<TValue>() where TValue : struct
		{
			return bindedAction.ReadValue<TValue>();
		}

		public void Enable()
		{
			if (Enabled)
				return;

			Enabled = true;
			bindedAction.Enable();
			bindedAction.started += OnBindedActionStarted;
			bindedAction.performed += OnBindedActionPerformed;
			bindedAction.canceled += OnBindedActionCanceled;
			OnEnableChanged?.Invoke();
		}
		
		public void Disable()
		{
			if (!Enabled)
				return;
			
			Enabled = false;
			bindedAction.Disable();
			bindedAction.started -= OnBindedActionStarted;
			bindedAction.performed -= OnBindedActionPerformed;
			bindedAction.canceled -= OnBindedActionCanceled;
			OnEnableChanged?.Invoke();
		}
		
		private void OnBindedActionCanceled(InputAction.CallbackContext context)
		{
			OnCanceled?.Invoke(contextProcessor.Process(new InputSystemInputContext(context, Id)));
		}

		private void OnBindedActionPerformed(InputAction.CallbackContext context)
		{
			OnPerformed?.Invoke(contextProcessor.Process(new InputSystemInputContext(context, Id)));
		}

		private void OnBindedActionStarted(InputAction.CallbackContext context)
		{
			OnStarted?.Invoke(contextProcessor.Process(new InputSystemInputContext(context, Id)));
		}

		public void Dispose()
		{
			Disable();
			OnEnableChanged = null;
			OnStarted = null;
			OnPerformed = null;
			OnCanceled = null;
		}
	}
}