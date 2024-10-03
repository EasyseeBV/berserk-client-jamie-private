namespace BerserkV3.Generic.Emotions
{
	public interface IEmotionsFactory
	{
		Emotion Create(string id);
	}
}