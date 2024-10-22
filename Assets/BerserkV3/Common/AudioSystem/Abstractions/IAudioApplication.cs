namespace BerserkV3.Common.AudioSystem.Abstractions
{
	public interface IAudioApplication
	{
		float SoundVolume { get; }
		float MusicVolume { get; }
		
		void SetSoundVolume(float value01);
		void SetMusicVolume(float value01);
		
		void PlaySound(object id);
		void PlayMusic(object id);
	}
}