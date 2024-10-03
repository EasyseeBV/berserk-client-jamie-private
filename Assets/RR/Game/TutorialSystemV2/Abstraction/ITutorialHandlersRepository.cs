using System.Collections.Generic;
using RR.Game.TutorialSystemV2.Abstraction.Handlers;

namespace RR.Game.TutorialSystemV2.Abstraction
{
	/// <summary>
	/// Register your scene views or controllers here that will be executed when the tutorial runs..
	/// <see cref="ITutorialIdentity"/> to filter execution by ids.
	/// <see cref="ITutorialOrderable"/> to ordering an execution.
	/// </summary>
	public interface ITutorialHandlersRepository
	{
		void Register(ITutorialHandler value);

		void UnRegister(ITutorialHandler value);

		void ResetAll();

		IEnumerable<T> GetAll<T>() where T : ITutorialHandler;

		IEnumerable<ITutorialHandler> GetAll();

		IEnumerable<T> Get<T>(bool includeNonIdentity, params string[] ids) where T : ITutorialHandler;

		IEnumerable<ITutorialHandler> Get(bool includeNonIdentity, params string[] ids);

		IEnumerable<T> GetNonIdentity<T>() where T : ITutorialHandler;

		IEnumerable<ITutorialHandler> GetNonIdentity();
	}
}