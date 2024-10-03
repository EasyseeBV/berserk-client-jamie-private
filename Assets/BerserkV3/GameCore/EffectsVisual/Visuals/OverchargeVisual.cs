using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.Abstraction;
using Berserk.Shared.GameCore.Commands.Cmd;
using Berserk.Shared.GameCore.EffectSystem.Effects.RuntimeArgs;
using Berserk.Shared.GameCore.Models;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using BerserkV3.GameCore.Network.Abstraction;
using BerserkV3.GameCore.UI;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{

	[EffectVisual(EffectVisualKeyword.Overcharge)]
	public class OverchargeVisual : EffectVisual
	{
		private readonly IGameHub gameHub;
		private readonly IRuntimeTimer runtimeTimer;
		private readonly IOverchargeView overchargeView;
		private readonly IGameDatabase gameDatabase;
		private OverchargeRuntimeArg arg;
		private int current;

		public OverchargeVisual(
			IGameHub gameHub,
			IRuntimeTimer runtimeTimer,
			IOverchargeView overchargeView,
			IGameDatabase gameDatabase)
		{
			this.gameHub = gameHub;
			this.runtimeTimer = runtimeTimer;
			this.overchargeView = overchargeView;
			this.gameDatabase = gameDatabase;
		}

		public override async UniTask ApplyLongEffectAsync()
		{
			arg = Model.GetRuntimeArg<OverchargeRuntimeArg>();
			if (arg == null || arg.Count == 0)
				return;

			current = arg.Count;
			overchargeView.SetValueText(current.ToString());
			overchargeView.SetHeaderText(gameDatabase.GetLocalization("Client_Overcharge_Header"));
			overchargeView.SetInteractable(true);
			overchargeView.SetIncreaseAction(OnIncrease);
			overchargeView.SetDecreaseAction(OnDecrease);
			overchargeView.SetConfirmAction(PerformUserAction);
			overchargeView.Show();
			await base.ApplyLongEffectAsync();
		}

		public override UniTask ChangeEffectAsync()
		{
			arg = Model.GetRuntimeArg<OverchargeRuntimeArg>();
			if (arg == null || arg.Count == 0)
				return ExpireLongEffectAsync();
			
			current = Mathf.Clamp(current, 0, arg.Count);
			overchargeView.SetValueText(current.ToString());
			return base.ChangeEffectAsync();
		}

		public override async UniTask ExpireLongEffectAsync()
		{
			CloseView();
			await base.ExpireLongEffectAsync();
		}
		
		private void OnIncrease()
		{
			if (arg == null || current >= arg.Count) 
				return;
			
			current++;
			overchargeView.SetValueText(current.ToString());
		}
		
		private void OnDecrease()
		{
			if (current <= 1) 
				return;
			
			current--;
			overchargeView.SetValueText(current.ToString());
		}

		private async UniTask PerformUserAction()
		{
			CloseView();
			if (arg == null)
				return;
			
			arg.Count = Mathf.Clamp(current, 0, arg.Count);
			var model = new CmdParamsModel(runtimeTimer.RuntimeData.TimeHash, arg)
			{
				ExecutorObjectId = Model.ExecutorId,
			};
			
			await gameHub.PerformCommandAsync<AcceptOverchargeCmd>(model);
		}
		
		private void CloseView()
		{
			overchargeView.SetInteractable(false);
			overchargeView.Close();
		}
	}
}