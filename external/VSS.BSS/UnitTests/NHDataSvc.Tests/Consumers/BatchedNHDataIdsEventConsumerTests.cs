using MassTransit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using VSS.Hosted.VLCommon;
using VSS.Nighthawk.NHDataSvc.Consumers;
using VSS.Nighthawk.NHDataSvc.Interfaces.Events;

namespace NHDataSvc.Tests.Consumers
{
  [TestClass]
  public class BatchedNHDataIdsEventConsumerTests
  {
    [TestMethod]
    public void ContextIsNullSoNothingHappens()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      var fakeResponse = new FakeResponseHandler();
      fakeResponse.AddFakeResponse(new Uri("http://example.org/test"), new HttpResponseMessage(HttpStatusCode.OK));
      HttpClient client = new HttpClient(fakeResponse);

      BatchedNHDataIdsEventConsumer consumer = new BatchedNHDataIdsEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
      consumer.Consume(null);
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Never());
    }

    [TestMethod]
    public void MessageIsNullSoNothingHappens()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      var fakeResponse = new FakeResponseHandler();
      fakeResponse.AddFakeResponse(new Uri("http://example.org/test"), new HttpResponseMessage(HttpStatusCode.OK));
      HttpClient client = new HttpClient(fakeResponse);

      var mockConsumeContext = new Mock<IConsumeContext<IBatchedNHDataIdsEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns((IBatchedNHDataIdsEvent)null);

      BatchedNHDataIdsEventConsumer consumer = new BatchedNHDataIdsEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
      consumer.Consume(mockConsumeContext.Object);
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Never());
    }

    [TestMethod]
    public void CallingInvalidUrlThrowsExceptionSoWillRetryLater()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      var fakeResponse = new FakeResponseHandler();
      fakeResponse.AddFakeResponse(new Uri("http://example.org/test"), new HttpResponseMessage(HttpStatusCode.OK));
      HttpClient client = new HttpClient(fakeResponse);

      var mockConsumeContext = new Mock<IConsumeContext<IBatchedNHDataIdsEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new BatchedNHDataIdsEvent { URL = "http://example.org/test", Ids = new long[] { 0 } });
      BatchedNHDataIdsEventConsumer consumer = new BatchedNHDataIdsEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
      consumer.Consume(mockConsumeContext.Object);
      mockConsumeContext.Verify(e => e.RetryLater(), Times.Once());
    }

    [TestMethod]
    public void CallingValidUrlWithNoIdsWillDoNothing()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      var fakeResponse = new FakeResponseHandler();

      var response = new HttpResponseMessage(HttpStatusCode.OK);
      response.Content = new ObjectContent<List<NHDataWrapper>>(new List<NHDataWrapper>(), new JsonMediaTypeFormatter(), "application/json");

      fakeResponse.AddFakeResponse(new Uri("http://example.org/test"), response);
      HttpClient client = new HttpClient(fakeResponse);

      var mockConsumeContext = new Mock<IConsumeContext<IBatchedNHDataIdsEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new BatchedNHDataIdsEvent { URL = "http://example.org/test", Ids = new long[0] });
      BatchedNHDataIdsEventConsumer consumer = new BatchedNHDataIdsEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
      consumer.Consume(mockConsumeContext.Object);
      mockConsumeContext.Verify(e => e.RetryLater(), Times.Never());
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Never());
    }

    [TestMethod]
    public void CallingValidUrlThatReturnsEmptyNHDataWrappersWillDoNothing()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      var fakeResponse = new FakeResponseHandler();

      var response = new HttpResponseMessage(HttpStatusCode.OK);
      response.Content = new ObjectContent<List<NHDataWrapper>>(new List<NHDataWrapper>(), new JsonMediaTypeFormatter(), "application/json");

      fakeResponse.AddFakeResponse(new Uri("http://example.org/test"), response);
      HttpClient client = new HttpClient(fakeResponse);

      var mockConsumeContext = new Mock<IConsumeContext<IBatchedNHDataIdsEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new BatchedNHDataIdsEvent { URL = "http://example.org/test", Ids = new long[] { 0 } });
      BatchedNHDataIdsEventConsumer consumer = new BatchedNHDataIdsEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
      consumer.Consume(mockConsumeContext.Object);
      mockConsumeContext.Verify(e => e.RetryLater(), Times.Never());
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Never());
    }

    [TestMethod]
    public void CallingValidUrlThatReturnsNHDataWrappersWillProcessOnce()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      var fakeResponse = new FakeResponseHandler();

      var response = new HttpResponseMessage(HttpStatusCode.OK);
      var wrappers = new List<NHDataWrapper> { new NHDataWrapper() };
      response.Content = new ObjectContent<List<NHDataWrapper>>(wrappers, new JsonMediaTypeFormatter(), "application/json");

      fakeResponse.AddFakeResponse(new Uri("http://example.org/test"), response);
      HttpClient client = new HttpClient(fakeResponse);

      var mockConsumeContext = new Mock<IConsumeContext<IBatchedNHDataIdsEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new BatchedNHDataIdsEvent { URL = "http://example.org/test", Ids = new long[]{0} });
      BatchedNHDataIdsEventConsumer consumer = new BatchedNHDataIdsEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
      consumer.Consume(mockConsumeContext.Object);
      mockConsumeContext.Verify(e => e.RetryLater(), Times.Never());
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Once());
    }

    private class BatchedNHDataIdsEvent : IBatchedNHDataIdsEvent
    {
      public string URL { get; set; }

      public long[] Ids { get; set; }

      public DateTime TimeStamp { get; set; }
    }
  }
}
