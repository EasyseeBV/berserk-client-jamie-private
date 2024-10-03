using System;
using UnityEngine;

namespace BerserkV3.Common.InputSystem.DragDropSystem
{
	public interface IDragDropInputHandler : IDisposable
	{
		/// <summary>
		/// Activity input
		/// </summary>
		bool Enabled { get; }
		
		/// <summary>
		/// Sensitive to start of drag
		/// </summary>
		float BeginThreshold { get; set; }
		
		bool DragInitiated { get; }
		
		/// <summary>
		/// When drag start position 
		/// </summary>
		Vector2? BeginPosition { get; }
		
		/// <summary>
		/// Current drag pointer position
		/// </summary>
		Vector2 Position { get; }

		/// <summary>
		/// When input starts dragging
		/// </summary>
		event Action OnStart;
		
		/// <summary>
		/// When drag the input position has changed
		/// </summary>
		event Action OnPreformed;
		
		/// <summary>
		/// When drag input ended
		/// </summary>
		event Action OnEnded;
		
		/// <summary>
		/// When drag input canceled
		/// </summary>
		event Action OnCanceled;

		/// <summary>
		/// Enable input activity
		/// </summary>
		void Enable();
		
		/// <summary>
		/// Disable input activity
		/// </summary>
		void Disable();
		
		/// <summary>
		/// Cancel current drag input
		/// </summary>
		void Cancel();
	}
}