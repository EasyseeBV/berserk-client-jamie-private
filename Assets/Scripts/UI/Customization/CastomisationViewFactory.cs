using System;
using System.Collections.Generic;
using System.Linq;
using Berserk.Shared.Data.Customisation;
using RR.UI.FrameSystem;
using UnityEngine;
using Object = UnityEngine.Object;

namespace UI
{
	public class CustomisationViewFactory : ICustomisationViewFactory
	{
		private readonly List<CustomisationPage> contents;
		
		public CustomisationViewFactory(IEnumerable<CustomisationPage> contents)
		{
			this.contents = contents.ToList();
		}

		public ICustomisationItemView Create(CustomisationType type, Transform parent = null)
		{
			var page = contents.FirstOrDefault(x => x.Type == type);
			if (page == null)
				throw new InvalidOperationException($"Page for type : {type} is missing");
			
			parent = parent ? parent : page.Container;
			var view = Object.Instantiate(page.ItemPrefab, parent).GetComponent<ICustomisationItemView>();

			if (view is ICustomisationDraggableView draggableView)
			{
				draggableView.SetFactory(this);
				draggableView.SetDragMode(DragMode.Copy);
			}

			return view;
		}

		public T Create<T>(CustomisationType type, Transform parent = null) where T : ICustomisationItemView
		{
			return (T) Create(type, parent);
		}
	}
}