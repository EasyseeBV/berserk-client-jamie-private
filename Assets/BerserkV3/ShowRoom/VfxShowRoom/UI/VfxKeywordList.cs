using System;
using RR.Core.Extensions;
using TMPro;
using UnityEngine;

namespace BerserkV3.ShowRoom.VfxShowRoom
{
	public class VfxKeywordList : MonoBehaviour
	{
		[SerializeField] private TMP_InputField showTimeInput;
		[SerializeField] private VfxKeywordItem itemPrefab;
		[SerializeField] private Transform itemContainer;
		
		public int GetShowTime()
		{
			return int.TryParse(showTimeInput.text, out var value) ? value : 10;
		}
		public void Clear()
		{
			itemContainer.DestroyChildrenExcept(itemPrefab.transform);
		}

		public VfxKeywordItem CreateItem()
		{
			return Instantiate(itemPrefab, itemContainer);
		}
	}
}