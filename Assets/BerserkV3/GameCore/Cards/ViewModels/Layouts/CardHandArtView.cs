using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Berserk.Shared.Data.Enums;
using Berserk.Shared.GameCore.LogicContext;
using BerserkV3.Common.Utils;
using Cysharp.Threading.Tasks;
using RR.Core.DebugSystem;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.GameCore.UI
{
	public class CardHandArtView : BaseView
	{
		[SerializeField] protected List<CardHandArtLayout> ArtLayouts;
		private CardHandArtLayout currentLayout;
		
		public UniTask SetupAsync(CardDataAdapter cardData, CancellationToken token = default)
		{
			SetupSeasonView(cardData.ArtType);
			return currentLayout.SetupAsync(cardData, token);
		}

		public void Refresh(CardDataAdapter data)
		{
			if (!currentLayout)
			{
				DefaultSharedLogger.Error("First setup the card view");
				return;
			}
			
			currentLayout.Refresh(data);
		}

		public void Release()
		{
			ArtLayouts.ForEach(seasonCardView=>
			{
				if (!seasonCardView)
					return;
				
				seasonCardView.Release();
				seasonCardView.SetActive(false);
			});
		}
		
		public void SetActive(bool value)
		{
			if (!gameObject) 
				return;
			gameObject.SetActive(value);
		}
		
		public void SetWarning(bool value)
		{
			SetActiveOutline(value);
			SetOutlineColor(Color.red);

		}
		
		public void SetTransparency(float space01)
		{
			if (currentLayout)
				currentLayout.SetTransparency(space01);
		}

		public void SetActiveOutline(bool value)
		{
			if (currentLayout)
				currentLayout.SetActiveOutline(value);
		}

		public void SetOutlineColor(Color value)
		{
			if (currentLayout)
				currentLayout.SetOutlineColor(value);
		}
		
		public void SetActiveShine(bool value)
		{
			if (currentLayout)
				currentLayout.SetActiveShine(value);
		}
		
		public void SetAttackText(int value)
		{
			if (currentLayout)
				currentLayout.SetAttackText(value);
		}
		
		public void SetArmorText(int value)
		{
			if (currentLayout)
				currentLayout.SetArmorText(value);
		}

		public void SetHealthText(int value)
		{
			if (currentLayout)
				currentLayout.SetHealthText(value);
		}

		public void SetManaText(int value)
		{
			if (currentLayout)
				currentLayout.SetManaText(value);
		}

		private void SetupSeasonView(ArtType artType)
		{
			currentLayout = default;
			foreach (var view in ArtLayouts)
			{
				if (view.ArtType == artType)
				{
					currentLayout = view;
					view.SetActive(true);
					continue;
				}
				
				view.SetActive(false);
			}
			
			if (currentLayout) 
				return;
			
			currentLayout = ArtLayouts.First();
			RRLogger.Error($"Missing {artType}, setup as default : {currentLayout.ArtType}");
		}

		public void SetLavaTextColor(Color color)
		{
			currentLayout.SetLavaTextColor(color);
		}
		
	}
}