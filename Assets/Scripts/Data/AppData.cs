using Audio;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using RR.UI.FrameSystem;

namespace Vulcan.Data
{
	public sealed class AppData
	{
		[JsonProperty("MusicVolume")]
		public float MusicVolume = 1f;
		
		[JsonProperty("AudioVolume")]
		private float audioVolume = 1f;

		[JsonConverter(typeof(StringEnumConverter))]
		public Clip LobbyMusic = Clip.MainTheme;

		[JsonIgnore]
		public float AudioVolume
		{
			get => audioVolume;
			set
			{
				audioVolume = value;
				UIManager.SetAudioVolume(audioVolume);
			}
		}
	}
}