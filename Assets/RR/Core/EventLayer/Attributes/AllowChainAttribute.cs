using System;

namespace RR.Core.EventLayer.Attributes
{
	/// <summary>
	/// Allow event call self from subscribers (beware of infinity loop)
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class AllowChainAttribute : Attribute
	{
	}
}