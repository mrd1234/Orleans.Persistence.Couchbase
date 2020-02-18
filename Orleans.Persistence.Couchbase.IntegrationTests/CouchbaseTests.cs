namespace Orleans.Persistence.Couchbase.IntegrationTests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    [Category("AcceptanceTests")]
    public class CouchbaseTests : TestBase<SiloBuilderConfigurator, ClientBuilderConfigurator>
    {
		public CouchbaseTests()
		{
			Initialize(3).Wait();
		}

		[Test]
		public async Task NewGrainShouldHaveNoState()
		{
			var state = await Cluster.GrainFactory.GetGrain<ITestGrain>("1").GetTheState();
			Assert.AreEqual(MockState.Empty, state);
		}

		[Test]
		public async Task ExistingGrainShouldHaveState()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain>("2");
			var mock = MockState.Generate();

			await grain.SaveMe(mock);
			await grain.Deactivate();

			var state = await grain.GetTheState();
			Assert.AreEqual(mock, state);
		}

		[Test]
		public async Task DeleteStateShouldRemoveState()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain>("3");
			var mock = MockState.Generate();

			await grain.SaveMe(mock);
			var state = await grain.GetTheState();
			Assert.AreEqual(mock, state);

			await grain.DeleteState();
			state = await grain.GetTheState();
			Assert.AreEqual(MockState.Empty, state);

			await grain.Deactivate();

			state = await grain.GetTheState();
			Assert.AreEqual(MockState.Empty, state);
		}

		[Test]
		public async Task MultipleWritesShouldEachUpdateState()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain>("5");
			var mock = MockState.Generate();

			await grain.SaveMe(mock);

            await grain.Deactivate();

			var state = await grain.GetTheState();
			Assert.AreEqual(mock, state);

			var mock2 = MockState.Generate();
            await grain.SaveMe(mock2);

            await grain.Deactivate();

			var state2 = await grain.GetTheState();
			Assert.AreEqual(mock2, state2);
		}

		[Test]
		public async Task GrainWithSecondStorageProviderShouldUseCorrectProvider()
		{
			var grain = Cluster.GrainFactory.GetGrain<ITestGrain2>("6");
			var mock = MockState.Generate();

			await grain.SaveMe(mock);

			var state = await grain.GetTheState();
			Assert.AreEqual(mock, state);
		}
	}
}
