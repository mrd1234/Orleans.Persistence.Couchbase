using System;
using Orleans.Persistence.Couchbase.Exceptions;
using Orleans.Runtime;

namespace Orleans.Persistence.Couchbase.Core
{
    public static class GrainKeyHelper
    {
        public static bool KeyMatches(object source, string primaryKey)
        {
            var key = GetPrimaryKeyAsString((IAddressable)source);

            return key == primaryKey;
        }

        private static string GetPrimaryKeyAsString(IAddressable grain)
        {
            switch (grain)
            {
                case IGrainWithGuidKey _:
                    return grain.GetPrimaryKey().ToString();
                case IGrainWithIntegerKey _:
                    return grain.GetPrimaryKeyLong().ToString();
                case IGrainWithStringKey _:
                    return grain.GetPrimaryKeyString();
            }

            throw new UnableToDeterminePrimaryKeyException($"Unable to determine primary key for {grain.GetType().Name}");
        }

        public static string GetPrimaryKeyAsString(GrainReference grainReference)
        {
            string result;

            try
            {
                result = grainReference.GetPrimaryKey().ToString();
            }
            catch (Exception)
            {
                try
                {
                    result = grainReference.GetPrimaryKeyLong().ToString();
                }
                catch (Exception)
                {
                    try
                    {
                        result = grainReference.GetPrimaryKeyString();
                    }
                    catch (Exception)
                    {
                        throw new UnableToDeterminePrimaryKeyException($"Unable to determine primary key from GrainReference {grainReference.ToKeyString()}");
                    }
                }
            }

            return result;
        }
    }
}