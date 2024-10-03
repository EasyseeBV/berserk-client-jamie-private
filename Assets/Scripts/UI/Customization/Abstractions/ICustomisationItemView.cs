using System.Threading;
using BerserkV3.Generic.Customisation;
using Cysharp.Threading.Tasks;

namespace UI
{
	public interface ICustomisationItemView
	{
		CustomisationItem Current { get; }
		
		UniTask SetupAsync(CustomisationItem item, CancellationToken token = default);
			
		void Deselect();
		
		void Select();
		
		void Dispose();
	}
}