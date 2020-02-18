using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Persistence.Couchbase.Serialization;
using Orleans.Runtime;
using Orleans.Storage;

namespace Orleans.Persistence.Couchbase.Core
{
    public class CouchbaseGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
	{
		private readonly string name;
        private readonly ILogger<CouchbaseGrainStorage> logger;
        private readonly ISerializer serializer;

        private ICouchbaseDataManager DataManager { get; }

        public CouchbaseGrainStorage(string name, ICouchbaseDataManager dataManager, ILogger<CouchbaseGrainStorage> logger, ISerializer serializer)
		{
			this.name = name;
			this.DataManager = dataManager ?? throw new ArgumentNullException(nameof(dataManager));
			this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public async Task ClearStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
			try
			{
                var grainTypeName = grainType.Split('.').Last();

                await this.DataManager.DeleteAsync(grainTypeName, grainReference?.ToKeyString(), grainState.ETag);
            }
			catch (Exception ex)
			{
				LogError("clearing", ex, grainType, grainReference, grainReference?.ToKeyString());
				throw;
			}
		}

		public async Task ReadStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
            var grainTypeName = grainType.Split('.').Last();

            var entityDataAndEtag = await this.DataManager.ReadAsync(grainTypeName, grainReference?.ToKeyString());
            if (entityDataAndEtag.Document != null)
            {
                grainState.State = serializer.Deserialize(entityDataAndEtag.Document, grainState.State.GetType());
                grainState.ETag = entityDataAndEtag.ETag;
            }
        }

		public async Task WriteStateAsync(string grainType, GrainReference grainReference, IGrainState grainState)
		{
			try
			{
                var grainTypeName = grainType.Split('.').Last();
                
                var entityData = serializer.Serialize(grainState.State);
                var eTag = grainState.ETag;
                var returnedEtag = await this.DataManager.WriteAsync(grainTypeName, grainReference?.ToKeyString(), entityData, eTag);
                grainState.ETag = returnedEtag;
            }
			catch (Exception ex)
			{
				LogError("writing", ex, grainType, grainReference, grainReference?.ToKeyString());
				throw;
			}
		}

		public void Participate(ISiloLifecycle lifecycle) => lifecycle.Subscribe(OptionFormattingUtilities.Name<CouchbaseGrainStorage>(name), ServiceLifecycleStage.ApplicationServices, Init, Close);

        private Task Init(CancellationToken ct) => DataManager.Initialise();

        private Task Close(CancellationToken ct)
		{
			this.DataManager.Dispose();
			return Task.CompletedTask;
		}

		private void LogError(string op, Exception ex, string grainType, GrainReference grainReference, string primaryKey)
			=> logger.LogError(
				ex,
				$"Error {op} grain state. GrainType = {grainType} Pk = {primaryKey} GrainId = {grainReference} from {this.DataManager.BucketName}",
				op,
				primaryKey,
				grainType,
				grainReference
			);
    }
}
