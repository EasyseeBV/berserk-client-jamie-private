using NUnit.Framework;

namespace RR.Core.Tests
{
	public class StatTest
	{
		[Test]
		public void EqualityOperations()
		{
			var intStat1 = new IntStat(10);
			var intStat2 = new IntStat(10);

			Assert.IsTrue(intStat1 == intStat2);
			Assert.AreEqual(intStat1, intStat2);
			Assert.AreNotSame(intStat1, intStat2);
		}

		[Test]
		public void OverloadedOperations()
		{
			var intStat1 = new IntStat(10);
			var intStat2 = new IntStat(20);
			var intStat3 = new IntStat(20);

			Assert.IsTrue(intStat1 < 20);
			Assert.IsTrue(20 > intStat1);
			Assert.IsTrue(10 >= intStat1);
			Assert.IsTrue(intStat1 >= 10);

			Assert.IsTrue(intStat1 < intStat2);
			Assert.IsTrue(intStat2 > intStat1);

			Assert.IsTrue(intStat2 == intStat3);
			Assert.IsTrue(intStat2 >= intStat3);
			Assert.IsTrue(intStat2 <= intStat3);
			Assert.IsTrue(intStat3 >= intStat2);
			Assert.IsTrue(intStat3 <= intStat2);

			Assert.IsTrue(intStat3 + intStat2 < 100);
			Assert.IsTrue(intStat3 + intStat2 > 39);
			Assert.IsTrue(intStat3 + intStat1 >= 30);
			Assert.IsTrue(intStat3 + intStat1 <= 30);

			Assert.IsTrue(intStat1 + 20 <= 30);
			Assert.IsTrue(intStat1 + 20 < 31);
			Assert.IsTrue(intStat1 + 20 > 29);
			Assert.IsTrue(20 + intStat1 <= 30);
			Assert.IsTrue(20 + intStat1 < 31);
			Assert.IsTrue(20 + intStat1 > 29);

			var floatStat1 = new FloatStat(10);
			var floatStat2 = new FloatStat(20);
			var floatStat3 = new FloatStat(20);

			Assert.IsTrue(floatStat1 < 20);
			Assert.IsTrue(20 > floatStat1);
			Assert.IsTrue(10 >= floatStat1);
			Assert.IsTrue(floatStat1 >= 10);

			Assert.IsTrue(floatStat1 < floatStat2);
			Assert.IsTrue(floatStat2 > floatStat1);

			Assert.IsTrue(floatStat2 == floatStat3);
			Assert.IsTrue(floatStat2 >= floatStat3);
			Assert.IsTrue(floatStat2 <= floatStat3);
			Assert.IsTrue(floatStat3 >= floatStat2);
			Assert.IsTrue(floatStat3 <= floatStat2);

			Assert.IsTrue(floatStat3 + floatStat2 < 100);
			Assert.IsTrue(floatStat3 + floatStat2 > 39);
			Assert.IsTrue(floatStat3 + floatStat1 >= 30);
			Assert.IsTrue(floatStat3 + floatStat1 <= 30);

			Assert.IsTrue(floatStat1 + 20 <= 30);
			Assert.IsTrue(floatStat1 + 20 < 31);
			Assert.IsTrue(floatStat1 + 20 > 29);
			Assert.IsTrue(20 + floatStat1 <= 30);
			Assert.IsTrue(20 + floatStat1 < 31);
			Assert.IsTrue(20 + floatStat1 > 29);

			var doubleStat1 = new DoubleStat(10);
			var doubleStat2 = new DoubleStat(20);
			var doubleStat3 = new DoubleStat(20);

			Assert.IsTrue(doubleStat1 < 20);
			Assert.IsTrue(20 > doubleStat1);
			Assert.IsTrue(10 >= doubleStat1);
			Assert.IsTrue(doubleStat1 >= 10);

			Assert.IsTrue(doubleStat1 < doubleStat2);
			Assert.IsTrue(doubleStat2 > doubleStat1);

			Assert.IsTrue(doubleStat2 == doubleStat3);
			Assert.IsTrue(doubleStat2 >= doubleStat3);
			Assert.IsTrue(doubleStat2 <= doubleStat3);
			Assert.IsTrue(doubleStat3 >= doubleStat2);
			Assert.IsTrue(doubleStat3 <= doubleStat2);

			Assert.IsTrue(doubleStat3 + doubleStat2 < 100);
			Assert.IsTrue(doubleStat3 + doubleStat2 > 39);
			Assert.IsTrue(doubleStat3 + doubleStat1 >= 30);
			Assert.IsTrue(doubleStat3 + doubleStat1 <= 30);

			Assert.IsTrue(doubleStat1 + 20 <= 30);
			Assert.IsTrue(doubleStat1 + 20 < 31);
			Assert.IsTrue(doubleStat1 + 20 > 29);
			Assert.IsTrue(20 + doubleStat1 <= 30);
			Assert.IsTrue(20 + doubleStat1 < 31);
			Assert.IsTrue(20 + doubleStat1 > 29);
		}
	}
}