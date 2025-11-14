using System.Collections.Generic;
using RR.Core.Extensions;
using SG;
using UnityEngine;
using UnityEngine.UI;
using Berserk.Shared.Data.Enums;

namespace UI
{
	public class LoopScrollRefresher
	{
		private readonly LoopScrollRect loopScrollRect;
		private static bool releasedPool;
		
		private readonly Faction deckFaction;

		public LoopScrollRefresher(LoopScrollRect scrollRect , Faction deckFaction)
		{
			this.deckFaction = deckFaction;
			
			// recreate prefabSource need to recreate pool instance
			var prefabSource = scrollRect.PrefabSource;
			var newPrefabSource = new LoopScrollPrefabSource
			{
				InitPoolSize = prefabSource.InitPoolSize, 
				Prefab = prefabSource.Prefab, 
				IsFromResources = prefabSource.IsFromResources, 
				PrefabNameFromResources = prefabSource.PrefabNameFromResources
			};
			scrollRect.PrefabSource = newPrefabSource;
			loopScrollRect = scrollRect;
			releasedPool = false;
		}
		
		public void ScrollToCard(List<IDeckCardStack> deckCards, IDeckCardStack deckCardStack)
		{
			var minIndex = 0;
			var maxIndex = Mathf.Max(minIndex, deckCards.Count - 1);
			var newIndex = deckCards.IndexOf(deckCardStack);
			var currIndex = Mathf.FloorToInt(Mathf.Abs(loopScrollRect.TotalCount > 0 ? loopScrollRect.verticalNormalizedPosition : 0) * maxIndex);
			InitScroll(deckCards);

			if (newIndex < 0)
			{
				ScrollToIndex(currIndex, maxIndex);
				return;
			}
			
			ScrollToIndex(newIndex, maxIndex);
		}

		public void Release()
		{
			if (loopScrollRect)
				loopScrollRect.ClearCells();
			
			if (releasedPool)
				return;
			
			releasedPool = true;
			ResourceManager.Instance.DestroyGameObject();
		}
		private void InitScroll(List<IDeckCardStack> deckCards)
		{
			loopScrollRect.Init(deckCards.ToArray(), loopScrollRect, deckFaction);
		}
		
		private void ScrollToIndex(int index, int maxIndex)
		{
			if (index < 0 || maxIndex == 0 || index > maxIndex)
				return;
			
			loopScrollRect.SrollToCell(index, -1);
		}
	}
}