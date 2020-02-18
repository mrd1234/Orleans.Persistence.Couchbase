using System;

namespace Orleans.Persistence.Couchbase.Serialization
{
    public interface ISerializer
	{
		string Serialize(object raw);
		object Deserialize(string serializedData, Type type);
	}
}
