using System.Threading.Tasks;

namespace Orleans.Persistance.Couchbase.IntegrationTests
{
    public interface ITestGrain2 : IGrainWithStringKey
	{
		Task<MockState> GetTheState();
		Task SaveMe(MockState mockState);
	}
}
