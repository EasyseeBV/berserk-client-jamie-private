using System;

namespace RR.Core.ResourceManagament
{
	public class ProgressMock : IProgress<float>
	{
		public void Report(float value) {}
	}
}