using System;
using System.Collections.Generic;
using System.Linq;
using RR.Core.DebugSystem;
using Zenject;

namespace BerserkV3.Common.InputSystem.SelectionSystem
{
	public class SelectionSystem : ISelectionSystem, IInitializable, IDisposable
	{
		private readonly IInputController<SelectionSystemActions> inputController;
		public event Action<ISelectable> OnSelected;
		public event Action<ISelectable> OnCanceled;
		public event Action<ISelectable> OnCanceledBySelection;
		public ISelectable Current { get; private set; }

		#region Concurent List Access

		private readonly object concurentList = new();
		private readonly List<ISelectable> selectables;

		private List<ISelectable> Selectables
		{
			get
			{
				lock (concurentList)
					return selectables;
			}
		}

		#endregion

		public SelectionSystem(IInputController<SelectionSystemActions> inputController)
		{
			this.inputController = inputController;
			selectables = new List<ISelectable>();
		}

		public void Initialize()
		{
			var selectionAction = inputController.Get(SelectionSystemActions.Select);
			var cancellationAction = inputController.Get(SelectionSystemActions.Cancel);

			selectionAction.OnPerformed += OnInputSelection;
			selectionAction.OnCanceled += OnInputSelection;

			cancellationAction.OnPerformed += OnInputCancellation;
			cancellationAction.OnCanceled += OnInputCancellation;
		}

		public void Dispose()
		{
			Selectables.Clear();
			OnSelected = null;
			OnCanceled = null;
		}

		public void Registration(ISelectable selectable)
		{
			try
			{
				if (selectable == null)
					throw new NullReferenceException($"Target {nameof(ISelectable)} is missing");

				if (Selectables.Contains(selectable))
					throw new InvalidOperationException($"{selectable} already registered");

				Selectables.Add(selectable);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void UnRegistration(ISelectable selectable)
		{
			try
			{
				if (selectable == null)
					throw new NullReferenceException($"Target {nameof(ISelectable)} is missing");

				if (!Selectables.Contains(selectable))
					return;

				if (Current == selectable)
					Cancel();

				Selectables.Remove(selectable);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Select(ISelectable selectable = null)
		{
			try
			{
				if (selectable != null && selectable != Current)
					CancelBySelection();

				if (selectable == null || !selectable.CanSelect())
					return;

				Current = selectable;
				OnSelected?.Invoke(selectable);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Cancel()
		{
			try
			{
				var mem = Current;
				Current = null;
				OnCanceled?.Invoke(mem);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Clear()
		{
			Selectables.Clear();
			Current = null;
			OnSelected = null;
			OnCanceled = null;
			OnCanceledBySelection = null;
		}

		private void CancelBySelection()
		{
			try
			{
				if (Current == null)
					return;

				var mem = Current;
				Current = null;
				OnCanceledBySelection?.Invoke(mem);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private ISelectable GetSelectable()
		{
			try
			{
				return Selectables.FirstOrDefault(x => x.CanSelect() && InputHelper.IsPointerOver(x.TargetView));
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return null;
			}
		}

		private void OnInputSelection(IInputContext context)
		{
			try
			{
				if (!context.Performed)
					return;

				Select(GetSelectable());
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		private void OnInputCancellation(IInputContext context)
		{
			try
			{
				if (!context.Performed)
					return;

				Cancel();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}
	}
}