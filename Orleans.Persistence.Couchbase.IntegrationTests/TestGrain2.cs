using System.Threading.Tasks;
using Orleans.Providers;

namespace Orleans.Persistence.Couchbase.IntegrationTests
{
    [StorageProvider(ProviderName = "TestStorageProvider2")]
    public class TestGrain2 : Grain<MockState>, ITestGrain2
    {
        public Task<MockState> GetTheState() => Task.FromResult(State);

        public async Task SaveMe(MockState mockState)
        {
            State = mockState;
            await WriteStateAsync();
        }
    }
}