using System.Collections.Generic;
using RR.UI.FrameSystem;
using Vulcan.Data;

namespace UI
{
	public partial class GraveyardView : BaseView
	{
		protected override void OnAwake()
		{
			// CloseBtn.Subscribe(Close);
			// HandCardView.gameObject.SetActive(false);
		}

		public void InitAndShow(IEnumerable<CardData> cards)
		{
			// CardsGrid.DestroyChildrenExcept(HandCardView.transform);
			// cards.ForEach(data =>
			// {
			// 	Instantiate(HandCardView, CardsGrid)
			// 		.Init()
			// 		.UpdateValues(ContentRepository.GetCopyOfCard(data.Id))
			// 		.gameObject
			// 		.SetActive(true);
			// });
			//
			// Show();
		}
	}
}