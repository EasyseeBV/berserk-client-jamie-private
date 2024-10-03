using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.GameCore.Utils;
using DG.Tweening;
using RR.Core.DebugSystem;
using RR.Core.Extensions;
using Zenject;

namespace BerserkV3.Common.InputSystem.HoveringSystem
{
	public class HoveringSystem : IHoveringSystem, IDisposable, IInitializable
	{
		private readonly IInputController<HoveringSystemActions> inputController;

		private class HoveredInfo
		{
			public int FromSublingIndex;
			public bool Hovered;
			public Tween Hovering;
		}
		private readonly Dictionary<IHoverable, HoveredInfo> hoverables;
		public event Action<IHoverable> OnHoverEnter;
		public event Action<IHoverable> OnHoverExit;
		public bool Enabled => disableQueue <= 0;
		private bool onceCancelled;
		private int disableQueue;
		
		public HoveringSystem(IInputController<HoveringSystemActions> inputController)
		{
			this.inputController = inputController;
			hoverables = new Dictionary<IHoverable, HoveredInfo>();
		}
		
		public void Initialize()
		{
			inputController.GetAll().ForEach(input =>
			{
				input.OnStarted += OnHoveringInput;
				input.OnPerformed += OnHoveringInput;
				input.OnCanceled += OnHoveringInput;
			});
		}
		
		public void Dispose()
		{
			OnHoverEnter = null;
			OnHoverExit = null;
		}

		public void Registration(IHoverable hoverable)
		{
			try
			{
				if (hoverable == null)
					throw new NullReferenceException($"Target {nameof(IHoverable)} is missing");

				if (hoverables.ContainsKey(hoverable))
					throw new InvalidOperationException($"{hoverable} already registered");

				hoverables.Add(hoverable, new HoveredInfo {Hovered = false});
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void UnRegistration(IHoverable hoverable)
		{
			try
			{
				if (hoverable == null)
					throw new NullReferenceException($"Target {nameof(IHoverable)} is missing");

				if (!hoverables.ContainsKey(hoverable))
					return;

				Cancel(hoverable);
				hoverables.Remove(hoverable);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Enable(bool value)
		{
			disableQueue = Math.Max(0, disableQueue + (value ? -1 : 1));
		}

		public void Start(IHoverable hoverable)
		{
			try
			{
				if (hoverable == null || !Enabled)
					return;

				if (!hoverable.HoverableSetting.Enabled)
					return;

				if (!hoverables.TryGetValue(hoverable, out var hoveredInfo) || hoveredInfo is not {Hovered: false})
					return;

				hoverables.ToArray()
					.Where(x => x.Value.Hovered)
					.ForEach(x => Cancel(x.Key));

				if (!hoverable.HoverableSetting.UseScaleUpAnimation)
				{
					hoveredInfo.Hovered = true;
					OnHoverEnter?.Invoke(hoverable);
					return;
				}

				hoveredInfo.FromSublingIndex = hoverable.SublingIndex;
				hoveredInfo.Hovering?.Kill();

				if (hoverable.HoverableSetting.ChangeSublingIndex)
					hoverable.SublingIndex = hoverable.HoverableSetting.MaxSublingIndex;

				hoveredInfo.Hovering = GetHoveringTween(hoverable);
				hoveredInfo.Hovered = true;
				OnHoverEnter?.Invoke(hoverable);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Cancel(IHoverable hoverable = null)
		{
			try
			{
				if (hoverable == null)
				{
					hoverables
						.ToArray()
						.Where(x => x.Value.Hovered && x.Key != null)
						.ForEach(x => Cancel(x.Key));
					return;
				}

				if (!hoverables.TryGetValue(hoverable, out var hoveredInfo) || hoveredInfo is not {Hovered: true})
					return;

				if (!hoverable.HoverableSetting.UseScaleUpAnimation)
				{
					hoveredInfo.Hovered = false;
					OnHoverExit?.Invoke(hoverable);
					return;
				}

				hoveredInfo.Hovering?.Kill();

				if (hoverable.HoverableSetting.ChangeSublingIndex)
					hoverable.SublingIndex = hoveredInfo.FromSublingIndex;

				hoveredInfo.Hovering = GetHoveringTween(hoverable, false);
				hoveredInfo.Hovered = false;
				OnHoverExit?.Invoke(hoverable);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}

		public void Clear()
		{
			Cancel();
			hoverables.Clear();
			OnHoverEnter = null;
			OnHoverExit = null;
		}

		private Tween GetHoveringTween(IHoverable hoverable, bool start = true)
		{
			try
			{
				if (hoverable == null)
					return default;

				return DOTween.To(() => hoverable.Size
						, value => hoverable.Size = value
						, start
							? hoverable.HoverableSetting.DefaultSize * hoverable.HoverableSetting.MaxSize
							: hoverable.HoverableSetting.DefaultSize
						, hoverable.HoverableSetting.Duration)
					.SetAutoKill(true);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				return null;
			}
		}
		
		private bool GetHoverable(out IHoverable hoverable)
		{
			try
			{
				return hoverables.Keys.ToArray().TryGet(x => x.HoverableSetting.Enabled && x.CanHover() && InputHelper.IsPointerOver(x.TargetView), out hoverable);
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
				hoverable = null;
				return false;
			}
		}
		
		private void OnHoveringInput(IInputContext context)
		{
			try
			{
				if (!Enabled)
				{
					if (onceCancelled)
						return;

					onceCancelled = true;
					Cancel();
					return;
				}

				var hasHoverable = GetHoverable(out var hoverable);
				if ((context.Started || context.Performed) && hasHoverable)
				{
					Start(hoverable);
					return;
				}

				if (context.Canceled || !hasHoverable)
					Cancel();
			}
			catch (Exception e)
			{
				RRLogger.Error(e);
			}
		}
	}
}