using System;
using Berserk.Shared.Data.Abstraction;
using Berserk.Shared.GameCore.Utils;

namespace Berserk.Shared.Data.Customisation
{
	[Serializable]
	public class CustomisationData : IConfigData
	{
		public string Id { get; set; }
		
		public bool IsDefault { get; set; }

		public CustomisationType Type { get; set; }

		public string AssetData { get; set; }
		
		public string PreviewUrl { get; set; }
		
		public string Title { get; set; }

		public override string ToString()
		{
			return this.ReflectionFormat();
		}
	}
}