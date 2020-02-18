using System;
using Newtonsoft.Json;

namespace Orleans.Persistence.Couchbase.Serialization
{
    public class JsonSerializer : ISerializer
	{
		private readonly JsonSerializerSettings settings;

		public JsonSerializer(JsonSerializerSettings settings)
		{
            this.settings = settings;
        }

        public string Serialize(object raw) => JsonConvert.SerializeObject(raw, settings);

        public object Deserialize(string serializedData, Type type) => JsonConvert.DeserializeObject(serializedData, type, settings);
	}
}