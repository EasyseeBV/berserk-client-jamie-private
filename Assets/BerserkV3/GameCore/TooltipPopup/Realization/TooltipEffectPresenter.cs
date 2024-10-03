using System.Threading;
using Berserk.Shared.Data.Abstraction;
using BerserkV3.Common.InputSystem.DragDropSystem;
using BerserkV3.Common.InputSystem.HoveringSystem;
using BerserkV3.GameCore.Cards.EffectHints;
using BerserkV3.GameCore.TargetSystem.Abstraction;
using UnityEngine;

namespace BerserkV3.GameCore.TooltipPopup
{
	public class TooltipPopupPresenter : IHoverable, ITooltipPopupPresenter
    {
        private readonly IDragDropSystem dragDropSystem;
        private readonly IHoveringSystem hoveringSystem;
        private readonly IManualArrowSystem manualArrowSystem;
        private readonly ITooltipPopupController tooltipPopupController;
        
        private CancellationTokenSource displaySource;
        private CancellationToken token;
        private IRuntimeData targetRuntime; 
        private EffectOrigin targetOrigin; 
        private bool initialized;
 
        public TooltipPopupPresenter(
            IDragDropSystem dragDropSystem,
            IHoveringSystem hoveringSystem,
            IManualArrowSystem manualArrowSystem,
            ITooltipPopupController tooltipPopupController)
        {
            this.dragDropSystem = dragDropSystem;
            this.hoveringSystem = hoveringSystem;
            this.manualArrowSystem = manualArrowSystem;
            this.tooltipPopupController = tooltipPopupController;
        }
        
        public void Setup(IEffectsHintsView view, IRuntimeData runtimeData, EffectOrigin origin)
        {
            if (initialized)
                return;
            
            initialized = view != null;
            if (!initialized)
                return;
 
            targetOrigin = origin;
            targetRuntime = runtimeData;
            TargetView = origin switch 
            {
                EffectOrigin.Innate => view.InnateContainer, 
                EffectOrigin.Gained => view.GainedContainer
            };
            hoveringSystem.Registration(this);
            HoverableSetting = new HoveringSettings();
            HoverableSetting.UseScaleUpAnimation = false;
            hoveringSystem.OnHoverEnter += OnHoverEnter;
            hoveringSystem.OnHoverExit += OnHoverExit;
        }
 
        public void Dispose()
        {
            if (!initialized)
                return;
 
            initialized = false;
            hoveringSystem.OnHoverEnter -= OnHoverEnter;
            hoveringSystem.OnHoverExit -= OnHoverExit;
            hoveringSystem.UnRegistration(this);
            TargetView = default;
            HoverableSetting = default;
            displaySource?.Cancel();
            displaySource?.Dispose();
            displaySource = null;
            targetRuntime = null;
        }
 
        private void OnHoverEnter(IHoverable target)
        {
	        if (!initialized || target != this)
		        return;
	        
            displaySource?.Cancel();
            displaySource?.Dispose();
            displaySource = new CancellationTokenSource();
            token = displaySource.Token;

            tooltipPopupController.DisplayAsync(targetRuntime, targetOrigin, TargetView.transform, token);
        }
 
        private void OnHoverExit(IHoverable target)
        {
	        if (!initialized || target != this)
		        return;
	        
            displaySource?.Cancel();
            displaySource?.Dispose();
            displaySource = null;
            
	        tooltipPopupController.Close();
        }
        
        public GameObject TargetView { get; private set; }
        public IHoverableSetting HoverableSetting { get; private set; }
        public int SublingIndex { get; set; }
        public Vector3 Size { get; set; }
 
        public bool CanHover() => !manualArrowSystem.IsActive && !dragDropSystem.AnyDragged;
    }
}