using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.Cards
{
	public interface IHandCardLayout : IRuntimeLayout
	{
		CardHandArtView HandArtView { get; } 
		void SetTitleText(string value);
	}
}