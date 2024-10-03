using System;
using System.Collections.Generic;
using System.Linq;

namespace BerserkV3.GameCore.SplineSystem
{
	public interface ISplineStorage
	{
		ISpline Get(SplineType splineType);
	}

	public class SplineStorage : ISplineStorage, IDisposable
	{
		private readonly Dictionary<SplineType, ISpline> storage;
		public SplineStorage(IEnumerable<ISpline> splines)
		{
			storage = splines.ToDictionary(x => x.SplineType);
		}

		public ISpline Get(SplineType splineType)
		{
			return storage.TryGetValue(splineType, out var spline) ? spline : default;
		}

		public void Dispose()
		{
			storage.Clear();
		}
	}
}