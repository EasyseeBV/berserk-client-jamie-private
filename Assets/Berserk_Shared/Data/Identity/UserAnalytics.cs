namespace Berserk.Shared.Data.Identity
{

	public class UserAnalytics
	{
		public string FirstDeckCreated { get; set; }
		public bool FirstDeckCreatedSent { get; set; }
		public string FirstGameWon { get; set; }
		
		public bool FirstGameWonSent { get; set; }
		public string FirstGameLost { get; set; }
		public bool FirstGameLostSent { get; set; }
		public bool TutorialCompleted { get; set; }
	}

}