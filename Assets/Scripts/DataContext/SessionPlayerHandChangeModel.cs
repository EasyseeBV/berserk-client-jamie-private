namespace ServerCore.Infrastructure.Models
{
	public class SessionPlayerHandChangeModel : PlayCardModel
	{
		public InteractiveCardModel NextDeckCard { get; set; }
	}
}