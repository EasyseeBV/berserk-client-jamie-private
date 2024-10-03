using System;
using Berserk.Shared.Data.Enums;
using Zenject;

namespace BerserkV3.GameCore.Controllers.Graveyard
{
	public interface IGraveyardPileController : IInitializable, IDisposable
	{
		Owner Owner { get; }
	}
}