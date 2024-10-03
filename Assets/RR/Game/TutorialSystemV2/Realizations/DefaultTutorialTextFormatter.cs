using RR.Game.TutorialSystemV2.Abstraction;

namespace RR.Game.TutorialSystemV2.Realizations
{
	public class DefaultTutorialTextFormatter : ITutorialTextFormatter
	{
		public string Format(string value)
		{
			return value;
		}
	}
}