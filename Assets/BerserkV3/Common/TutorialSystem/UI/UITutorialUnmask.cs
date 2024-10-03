using System.Collections.Generic;
using System.Linq;
using Coffee.UIExtensions;
using RR.Core.Extensions;
using RR.UI.FrameSystem;
using UnityEngine;

namespace BerserkV3.Common.TutorialSystem
{

	public partial class UITutorialUnmask : BaseView
	{
		[SerializeField] private Unmask unmask;
		[SerializeField] private GameObject raycastFilter;

		private readonly List<Unmask> pool = new();

		protected override void OnAwake()
		{
			base.OnAwake();
			pool.Add(unmask);
		}

		public RectTransform GetUnmask(int index)
		{
			if (index >= pool.Count)
			{
				var unmaskCopy = Instantiate(unmask, transform);
				var unmaskCopyRect = unmaskCopy.GetComponent<RectTransform>();
				unmaskCopyRect.name = $"{unmask.name}_{index}";
				unmaskCopyRect.SetSiblingIndex(unmask.transform.GetSiblingIndex() + index);
				raycastFilter.AddComponent<UnmaskRaycastFilter>().targetUnmask = unmaskCopy;
				pool.Add(unmaskCopy);
				return unmaskCopyRect;
			}

			return pool[index].GetComponent<RectTransform>();
		}

		public void Clear()
		{
			if (transform && unmask && raycastFilter)
			{
				pool.Clear();
				pool.Add(unmask);
				raycastFilter
					.GetComponents<UnmaskRaycastFilter>()
					.Where(filter => filter.targetUnmask != unmask)
					.ForEach(filter => Destroy(filter));

				transform.DestroyChildrenExcept(unmask.transform, raycastFilter.transform);
			}
		}

		public void Enable(bool value)
		{
			gameObject.SetActive(value);
			
			if (!value)
				pool.ForEach(x => x.gameObject.SetActive(false));
		}
	}

}