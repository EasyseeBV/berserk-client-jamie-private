namespace Vulcan.Shop.Domain
{
	public class EffectInfo
	{
		public string Name { get; }
		public string Description { get; }
		public int Value { get; }

		public EffectInfo(string name, string description, int value)
		{
			Name = name;
			Description = description;
			Value = value;
		}
	}
}