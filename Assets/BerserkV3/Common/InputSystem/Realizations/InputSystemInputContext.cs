using System;
using RR.Core.DebugSystem;
using UnityEngine.InputSystem;

namespace BerserkV3.Common.InputSystem
{
	public struct InputSystemInputContext : IInputContext
	{
		public string Id { get; }
		public bool Started => context.started;
		public bool Performed => context.performed;
		public bool Canceled => context.canceled;
		public Type ControlValueType => context.control.valueType;

		private InputAction.CallbackContext context;

		public InputSystemInputContext(InputAction.CallbackContext context, string id)
		{
			this.context = context;
			Id = id;
		}
		
		public TValue ReadValue<TValue>() where TValue : struct
		{
			return context.ReadValue<TValue>();
		}

		public bool TryReadValue<TValue>(out TValue result) where TValue : struct
		{
			result = default;
			if (context.valueType != typeof(TValue) || context.control.valueType != typeof(TValue))
				return false;
			
			try
			{
				result = ReadValue<TValue>();
				return true;
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return false;
			}
		}
	}
}