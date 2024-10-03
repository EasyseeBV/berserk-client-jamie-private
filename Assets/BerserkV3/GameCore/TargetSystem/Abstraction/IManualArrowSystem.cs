using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BerserkV3.GameCore.UI;

namespace BerserkV3.GameCore.TargetSystem.Abstraction
{
	public interface IManualArrowSystem
	{
		PickInfo Current { get; }
		bool IsActive { get; }
		
		event Action OnArrowActivityChanged;

		Task<IList<IRuntimeObjectView>> GetTargetsAsync(PickInfo pickInfo);

		void Cancel();
	}
}