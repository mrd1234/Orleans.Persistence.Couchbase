using System.Threading.Tasks;

namespace Orleans.Persistance.Couchbase.IntegrationTests
{
    public interface ITestGrain : IGrainWithStringKey
	{
		Task<MockState> GetTheState();
		Task SaveMe(MockState mockState);
		Task Deactivate();
		Task DeleteState();
		Task WriteNullToState();
	}
}
