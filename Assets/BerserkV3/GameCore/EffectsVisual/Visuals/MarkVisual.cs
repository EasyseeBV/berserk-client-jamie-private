using System.Collections.Generic;
using Berserk.Shared.Data.Enums;
using BerserkV3.GameCore.EffectsVisual.Attributes;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace BerserkV3.GameCore.EffectsVisual.Visuals
{
	[EffectVisual(EffectVisualKeyword.Mark)]
	public class MarkVisual : EffectVisual
	{
		private const float PADDING = 1f;
		private readonly List<VFXView> views = new();

		public override UniTask ApplyLongEffectAsync()
		{
			foreach (var target in Targets)
			{
				if (!target.RuntimeGameObject.TryGetAppliedEffect(EffectKeyword.Mark, out var effect))
					continue;

				var markCount = effect.RuntimeData.CurrentValue;
				for (var i = 0; i < markCount; i++)
				{
					var vfxView = VfxApplication.SpawnVfx(EffectVisualKeyword.Mark, target.SelfContainer);
					var vfxTransform = (RectTransform)vfxView.transform;
					var size = vfxTransform.rect.width;
					var pos = vfxTransform.localPosition;
					pos.x += i * ((size + PADDING) + PADDING); // horizontal layout
					pos.x -= (size * (markCount-1)) / 2f; // centering
					vfxTransform.localPosition = pos;
					views.Add(vfxView);
				}
			}

			return UniTask.CompletedTask;
		}

		public override UniTask ChangeEffectAsync()
		{
			views.ForEach(view =>
			{
				if (view) 
					view.DestroyInstance();
			});
			views.Clear();
			
			return ApplyLongEffectAsync();
		}

		public override UniTask ExpireLongEffectAsync()
		{
			SoftStop();
			return UniTask.CompletedTask;
		}

		public override void SoftStop()
		{
			views.ForEach(view =>
			{
				if (view) 
					view.SoftStop();
			});
			views.Clear();
			base.SoftStop();
		}
		public override void Dispose()
		{
			views.ForEach(view =>
			{
				if (view) 
					view.DestroyInstance();
			});
			views.Clear();
		}
	}
}