﻿using BeekmanLabs.UnitTesting;
using Common.Logging;
using Elasticsearch.Net;
using IntegrationEngine.Core.Storage;
using IntegrationEngine.Model;
using Moq;
using Nest;
using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;

namespace IntegrationEngine.Core.Tests.Storage
{
    public class ElasticsearchRepositoryTest : TestBase<ElasticsearchRepository>
    {
        Mock<StubElasticClient> ElasticClient { get; set; }
        Mock<ILog> Log { get; set; }

        [SetUp]
        public void Setup()
        {
            ElasticClient = new Mock<StubElasticClient>();
            Subject.ElasticClient = ElasticClient.Object;
            Log = new Mock<ILog>();
            Subject.Log = Log.Object;
        }

        [Test]
        public void ShouldReturnListOfDocumentsWithIdsFromElasticsearch()
        {
            var searchResponse = new Mock<StubSearchResponse<CronTrigger>>();
            var hits = new List<IHit<CronTrigger>>();
            var cronTrigger = new CronTrigger();
            var hit = new Mock<StubHit<CronTrigger>>();
            var expectedId = "1";
            hit.SetupGet(x => x.Source).Returns(() => cronTrigger);
            hit.SetupGet(x => x.Id).Returns(() => expectedId);
            hits.Add(hit.Object);
            searchResponse.SetupGet(x => x.Hits).Returns(() => hits);
            ElasticClient.Setup(x => x.Search<CronTrigger>(It.IsAny<Func<SearchDescriptor<CronTrigger>, SearchDescriptor<CronTrigger>>>()))
                .Returns(searchResponse.Object);


            var actual = Subject.SelectAll<CronTrigger>();

            Assert.That(actual, Is.Not.Empty);
            Assert.That(actual.First().Id, Is.EqualTo(expectedId));
            ElasticClient.Verify(x => x.Search(It.IsAny<Func<SearchDescriptor<CronTrigger>, SearchDescriptor<CronTrigger>>>()), Times.Once);
        }

        [Test]
        public void ShouldReturnNullIfDocumentIsNotFoundById()
        {
            var getResponse = new Mock<StubGetResponse<CronTrigger>>();
            ElasticClient.Setup(x => x.Get(It.IsAny<Func<GetDescriptor<CronTrigger>, GetDescriptor<CronTrigger>>>()))
                .Returns(getResponse.Object);

            var actual = Subject.SelectById<CronTrigger>("1");

            Assert.That(actual, Is.Null);
            ElasticClient.Verify(x => x.Get(It.IsAny<Func<GetDescriptor<CronTrigger>, GetDescriptor<CronTrigger>>>()), Times.Once);
        }

        [Test]
        public void ShouldReturnSingleDocumentGivenAnId()
        {
            var getResponse = new Mock<StubGetResponse<CronTrigger>>();
            var expectedId = "1";
            getResponse.SetupGet(x => x.Id).Returns(() => expectedId);
            getResponse.SetupGet(x => x.Source).Returns(() => new CronTrigger());
            ElasticClient.Setup(x => x.Get(It.IsAny<Func<GetDescriptor<CronTrigger>, GetDescriptor<CronTrigger>>>()))
                .Returns(getResponse.Object);

            var actual = Subject.SelectById<CronTrigger>(expectedId);

            Assert.That(actual.Id, Is.EqualTo(expectedId));
            ElasticClient.Verify(x => x.Get(It.IsAny<Func<GetDescriptor<CronTrigger>, GetDescriptor<CronTrigger>>>()), Times.Once);
        }

        [Test]
        public void ShouldInsertAndReturnDocument()
        {
            var expected = new CronTrigger() {
                Id = "1",
            };
            var getResponse = new Mock<StubGetResponse<CronTrigger>>();
            getResponse.SetupGet(x => x.Id).Returns(() => expected.Id);
            getResponse.SetupGet(x => x.Source).Returns(() => new CronTrigger());
            ElasticClient.Setup(x => x.Get<CronTrigger>(It.IsAny<Func<GetDescriptor<CronTrigger>, GetDescriptor<CronTrigger>>>()))
                .Returns(getResponse.Object);
            var indexResponse = new Mock<StubIndexResponse>();
            indexResponse.SetupGet(x => x.Id).Returns(() => expected.Id);
            ElasticClient.Setup(x => x.Index(expected, It.IsAny<Func<IndexDescriptor<CronTrigger>, IndexDescriptor<CronTrigger>>>()))
                .Returns(indexResponse.Object);

            var actual = Subject.Insert(expected);

            Assert.That(actual.Id, Is.EqualTo(expected.Id));
            ElasticClient.Verify(x => x.Index(expected, It.IsAny<Func<IndexDescriptor<CronTrigger>, IndexDescriptor<CronTrigger>>>()), Times.Once);
        }

        [Test]
        public void ShouldUpdateAndReturnDocument()
        {
            var expected = new CronTrigger() {
                Id = "1",
            };
            var getResponse = new Mock<StubGetResponse<CronTrigger>>();
            getResponse.SetupGet(x => x.Id).Returns(() => expected.Id);
            getResponse.SetupGet(x => x.Source).Returns(() => new CronTrigger());
            ElasticClient.Setup(x => x.Get<CronTrigger>(It.IsAny<Func<GetDescriptor<CronTrigger>, GetDescriptor<CronTrigger>>>()))
                .Returns(getResponse.Object);
            var updateResponse = new Mock<StubUpdateResponse>();
            updateResponse.SetupGet(x => x.Id).Returns(() => expected.Id);
            ElasticClient.Setup(x => x.Update<CronTrigger>(It.IsAny<Func<UpdateDescriptor<CronTrigger, CronTrigger>, UpdateDescriptor<CronTrigger, CronTrigger>>>()))
                .Returns(updateResponse.Object);

            var actual = Subject.Update(expected);

            Assert.That(actual.Id, Is.EqualTo(expected.Id));
            ElasticClient.Verify(
                x => x.Update<CronTrigger>(It.IsAny<Func<UpdateDescriptor<CronTrigger, CronTrigger>, UpdateDescriptor<CronTrigger, CronTrigger>>>()), 
                Times.Once);
        }

        [Test]
        public void ShouldDeleteDocument()
        {
            var id = "1";
            ElasticClient.Setup(x => x.Delete(It.IsAny<Func<DeleteDescriptor<CronTrigger>, DeleteDescriptor<CronTrigger>>>()));

            Subject.Delete<CronTrigger>(id);

            ElasticClient.Verify(x => x.Delete(It.IsAny<Func<DeleteDescriptor<CronTrigger>, DeleteDescriptor<CronTrigger>>>()), Times.Once);
        }

        [Test]
        public void ShouldShouldReturnTrueIfServerIsAvailable()
        {
            var elasticsearchResponse = new Mock<StubElasticsearchResponse>();
            elasticsearchResponse.SetupGet(x => x.Success).Returns(() => true);
            var pingResponse = new Mock<StubPingResponse>();
            pingResponse.SetupGet(x => x.ConnectionStatus).Returns(() => elasticsearchResponse.Object);
            ElasticClient.Setup(x => x.Ping(It.IsAny<PingRequest>())).Returns(pingResponse.Object);

            var actual = Subject.IsServerAvailable();

            Assert.That(actual, Is.True);
        }

        [Test]
        public void ShouldShouldReturnFalseIfServerIsUnavailable()
        {
            var elasticsearchResponse = new Mock<StubElasticsearchResponse>();
            elasticsearchResponse.SetupGet(x => x.Success).Returns(() => false);
            var pingResponse = new Mock<StubPingResponse>();
            pingResponse.SetupGet(x => x.ConnectionStatus).Returns(() => elasticsearchResponse.Object);
            ElasticClient.Setup(x => x.Ping(It.IsAny<PingRequest>())).Returns(pingResponse.Object);

            var actual = Subject.IsServerAvailable();

            Assert.That(actual, Is.False);
        }

        [Test]
        public void ShouldShouldReturnFalseIfServerIsUnavailableBecauseExceptionOccured()
        {
            ElasticClient.Setup(x => x.Ping(It.IsAny<PingRequest>())).Returns<PingResponse>(null);
            Log.Setup(x => x.Error(It.IsAny<Exception>()));

            var actual = Subject.IsServerAvailable();

            Assert.That(actual, Is.False);
            Log.Verify(x => x.Error(It.IsAny<Exception>()), Times.Once);
        }
    }
}
