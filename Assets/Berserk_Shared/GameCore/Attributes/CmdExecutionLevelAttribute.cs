using System;
using Berserk.Shared.Data.Enums;

namespace Berserk.Shared.GameCore.Attributes
{
	public class CmdExecutionLevelAttribute : Attribute
	{
		public CmdExecutionLevel Level { get; set; }

		public CmdExecutionLevelAttribute(CmdExecutionLevel level)
		{
			Level = level;
		}
	}
}