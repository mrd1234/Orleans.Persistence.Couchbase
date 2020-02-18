using System;
using System.Linq;

namespace Orleans.Persistence.Couchbase.IntegrationTests
{
    [Serializable]
    public class MockState
	{
		private static readonly Random Rand = new Random();

		public int NoHeads { get; set; }
		public string Name { get; set; }

		public static MockState Empty { get; } = new MockState();

		public static MockState Generate()
			=> new MockState
			{
				Name = string.Join(string.Empty, Enumerable.Range(0, 10).Aggregate(string.Empty, (s, i) => s += Rand.Next(10))),
				NoHeads = Rand.Next(8)
			};

		public override bool Equals(object obj)
		{
			var comparee = (MockState)obj;
			return comparee.NoHeads == NoHeads && comparee.Name == Name;
		}
	}
}
