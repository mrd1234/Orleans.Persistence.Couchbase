namespace Orleans.Persistence.Couchbase.UnitTests
{
    using System;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Extensions.Logging;
    using Moq;
    using Newtonsoft.Json;
    using NUnit.Framework;
    using Orleans.Persistence.Couchbase.Core;
    using Orleans.Persistence.Couchbase.Models;
    using Orleans.Persistence.Couchbase.Serialization;

    [TestFixture]
    [System.ComponentModel.Category("UnitTests")]
    public class CouchbaseGrainStorageTests
	{
		[Test]
		public async Task ReadStateAsyncShouldCallDataManagerReadAsync()
		{
            var mockDataManager = new Mock<ICouchbaseDataManager>();
            var mockLogger = new Mock<ILogger<CouchbaseGrainStorage>>();
            var mockSerialiser = new Mock<ISerializer>();

            mockDataManager.Setup(s => s.ReadAsync(It.Is<string>(i => i == "String"), It.IsAny<string>()))
                .ReturnsAsync(() => new ReadResponse { Document = "123", ETag = "456" }).Verifiable();

            mockSerialiser.Setup(s => s.Deserialize(It.Is<string>(i => i == "123"), It.IsAny<Type>())).Returns(() => "Deserialised").Verifiable();

            var sut = new CouchbaseGrainStorage(string.Empty, mockDataManager.Object, mockLogger.Object, mockSerialiser.Object);

            var grainState = new GrainState<string> { State = "" };
            await sut.ReadStateAsync(typeof(string).Name, null, grainState);

            grainState.State.Should().Be("Deserialised");
            grainState.ETag.Should().Be("456");
        }

        [Test]
        public async Task WriteStateAsyncShouldCallDataManagerWriteAsync()
        {
            var mockDataManager = new Mock<ICouchbaseDataManager>();
            var mockLogger = new Mock<ILogger<CouchbaseGrainStorage>>();
            var mockSerialiser = new Mock<ISerializer>();

            var serialised = JsonConvert.SerializeObject("Serialised");

            mockDataManager.Setup(
                    s => s.WriteAsync(
                        It.Is<string>(i => i == "String"),
                        It.IsAny<string>(),
                        It.Is<string>(i => i == serialised),
                        It.Is<string>(i => i == "456")))
                .ReturnsAsync(() => "eTag").Verifiable();

            mockSerialiser.Setup(s => s.Serialize(It.Is<string>(i => i == "123"))).Returns(() => serialised).Verifiable();

            var sut = new CouchbaseGrainStorage(string.Empty, mockDataManager.Object, mockLogger.Object, mockSerialiser.Object);

            var grainState = new GrainState<string> { State = "123", ETag = "456" };
            await sut.WriteStateAsync(typeof(string).Name, null, grainState);

            grainState.State.Should().Be("123");
            grainState.ETag.Should().Be("eTag");
        }

        [Test]
        public async Task ClearStateAsyncShouldCallDataManagerDeleteAsync()
        {
            var mockDataManager = new Mock<ICouchbaseDataManager>();
            var mockLogger = new Mock<ILogger<CouchbaseGrainStorage>>();
            var mockSerialiser = new Mock<ISerializer>();

            var sut = new CouchbaseGrainStorage(string.Empty, mockDataManager.Object, mockLogger.Object, mockSerialiser.Object);

            var grainState = new GrainState<string> { State = "123", ETag = "456" };
            await sut.ClearStateAsync(typeof(string).Name, null, grainState);

            mockDataManager.Verify(v => v.DeleteAsync(It.Is<string>(i => i == "String"), It.IsAny<string>(), It.Is<string>(i => i == "456")), Times.Once);
        }
	}
}
