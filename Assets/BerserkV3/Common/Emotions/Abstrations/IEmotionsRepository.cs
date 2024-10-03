namespace BerserkV3.Generic.Emotions
{
	public interface IEmotionsRepository
	{
		void Add(Emotion value);

		Emotion Get(string id);

		bool Contains(string id);
	}
}