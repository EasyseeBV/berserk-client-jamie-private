using System.Collections.Generic;
using System.Linq;
using RR.Core.Extensions;

namespace RR.Core.Utilities
{
	public static class Constants
	{
		public static readonly List<char> Numbers = "0123456789".ToCharArray().ToList();
		public static readonly List<char> Alphabet = Enumerable.Range('a', 26).Select(x => (char)x).ToList();
		public static readonly List<char> Specials = Enumerable.Range(33, 15).Select(x => (char)x).ToList();

		//todo: find a better class for this
		public static string GenerateRandomCode(int symbols = 6, bool useNumbers = true, bool useUpLower = true, bool useSpecials = false)
		{
			var items = Alphabet.ToList();

			if (useNumbers)
				items.AddRange(Numbers);

			if (!useUpLower)
				items = items.Select(char.ToUpper).ToList();

			if (useSpecials)
				items.AddRange(Specials);

			return string.Join("", items.Shuffle().Take(symbols).Shuffle());
		}
	}
}