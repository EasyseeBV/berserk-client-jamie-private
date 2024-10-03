using System.Linq;
using Berserk.Shared.Data.Game;
using RR.UI.FrameSystem;
using UnityEngine.UI;

namespace UI
{
	public class DeckItemView : BaseView, ILoopScrollElement<IDeckCardStack>
	{
		protected IDeckCardStack DeckCardStack { get; private set; }
		protected bool IsClone { get; private set; }
		public CardData CardData => DeckCardStack?.CardData;

		protected virtual void SetUp(IDeckCardStack deckCardStack, bool isClone)
		{
			if (DeckCardStack != null)
				DeckCardStack.OnStackChanged -= OnRefreshView;
			
			IsClone = isClone;
			DeckCardStack = deckCardStack;
			DeckCardStack.OnStackChanged += OnRefreshView;
			OnRefreshView(DeckCardStack);
			Show(noAnimation:true);
		}
		
		protected virtual void OnRefreshView(IDeckCardStack deckCardStack){}
		
		public virtual void SetScrollData(int index, IDeckCardStack deckCardStack, params object[] other)
		{
			SetUp(deckCardStack, other.OfType<bool>().FirstOrDefault());
		}

		protected virtual void OnDestroy()
		{
			if (DeckCardStack != null)
				DeckCardStack.OnStackChanged += OnRefreshView;
		}
	}
}