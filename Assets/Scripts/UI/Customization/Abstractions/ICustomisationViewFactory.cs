using Berserk.Shared.Data.Customisation;
using UnityEngine;

namespace UI
{
	public interface ICustomisationViewFactory
	{
		ICustomisationItemView Create(CustomisationType type, Transform parent = null);
		T Create<T>(CustomisationType type, Transform parent = null) where T : ICustomisationItemView;
	}
}