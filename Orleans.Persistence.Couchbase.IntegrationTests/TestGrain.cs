using System.Threading.Tasks;
using Orleans.Providers;

namespace Orleans.Persistence.Couchbase.IntegrationTests
{
    [StorageProvider(ProviderName = "TestStorageProvider")]
	public class TestGrain : Grain<MockState>, ITestGrain
	{
		public Task<MockState> GetTheState() => Task.FromResult(State);

		public async Task SaveMe(MockState mockState)
		{
			State = mockState;
			await WriteStateAsync();
		}

		public Task Deactivate()
		{
			DeactivateOnIdle();
			return Task.CompletedTask;
		}

		public Task DeleteState() => ClearStateAsync();

		public Task WriteNullToState()
		{
			State = null;
			return WriteStateAsync();
		}
	}
}
