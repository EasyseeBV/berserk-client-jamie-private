namespace BerserkV3.Common.SerializedHelper
{
	public class SerializeHelperAdapter
	{
		public static ISerializeHelper Service { get; private set; }

		public SerializeHelperAdapter(ISerializeHelper serializeHelper)
		{
			Service = serializeHelper;
		}
	}
}