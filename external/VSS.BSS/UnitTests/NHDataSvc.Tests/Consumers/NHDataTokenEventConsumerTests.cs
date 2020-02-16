using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using VSS.Nighthawk.NHDataSvc.Consumers;
using Moq;
using VSS.Hosted.VLCommon;
using System.Net.Http.Formatting;
using MassTransit;
using VSS.Nighthawk.NHDataSvc.Interfaces.Events;

namespace NHDataSvc.Tests.Consumers
{
  [TestClass]
  public class NHDataTokenEventConsumerTests
  {
    [TestMethod]
    public void ContextIsNullSoNothingHappens()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      var fakeResponse = new FakeResponseHandler();
      fakeResponse.AddFakeResponse(new Uri("http://example.org/test"), new HttpResponseMessage(HttpStatusCode.OK));
      HttpClient client = new HttpClient(fakeResponse);

      NHDataTokenEventConsumer consumer = new NHDataTokenEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
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

       var mockConsumeContext = new Mock<IConsumeContext<INewNhDataTokenEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns((INewNhDataTokenEvent)null);
      
      NHDataTokenEventConsumer consumer = new NHDataTokenEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
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

      var mockConsumeContext = new Mock<IConsumeContext<INewNhDataTokenEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new NewNhDataTokenEvent { NHDataObjectUrl = "InvalidURL", Id = 0});
      NHDataTokenEventConsumer consumer = new NHDataTokenEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
      consumer.Consume(mockConsumeContext.Object);
      mockConsumeContext.Verify(e => e.RetryLater(), Times.Once());
    }

    [TestMethod]
    public void CallingValidUrlThatReturnsEmptyNHDataWrappersWillDoNothing()
    {
      Mock<INHDataProcessor> processor = new Mock<INHDataProcessor>();
      var fakeResponse = new FakeResponseHandler();
      
      var response = new HttpResponseMessage(HttpStatusCode.OK);
      response.Content = new ObjectContent<List<NHDataWrapper>>(new List<NHDataWrapper>(), new JsonMediaTypeFormatter(), "application/json");

      fakeResponse.AddFakeResponse(new Uri("http://example.org/test/0"), response);
      HttpClient client = new HttpClient(fakeResponse);

      var mockConsumeContext = new Mock<IConsumeContext<INewNhDataTokenEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new NewNhDataTokenEvent { NHDataObjectUrl = "http://example.org/test", Id = 0 });
      NHDataTokenEventConsumer consumer = new NHDataTokenEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
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
      var wrappers = new List<NHDataWrapper>{new NHDataWrapper()};
      response.Content = new ObjectContent<List<NHDataWrapper>>(wrappers, new JsonMediaTypeFormatter(), "application/json");

      fakeResponse.AddFakeResponse(new Uri("http://example.org/test/0"), response);
      HttpClient client = new HttpClient(fakeResponse);

      var mockConsumeContext = new Mock<IConsumeContext<INewNhDataTokenEvent>>();
      mockConsumeContext.SetupGet(o => o.Message).Returns(new NewNhDataTokenEvent { NHDataObjectUrl = "http://example.org/test", Id = 0 });
      NHDataTokenEventConsumer consumer = new NHDataTokenEventConsumer(processor.Object, client, new List<MediaTypeFormatter>());
      consumer.Consume(mockConsumeContext.Object);
      mockConsumeContext.Verify(e => e.RetryLater(), Times.Never());
      processor.Verify(e => e.Process(It.IsAny<List<NHDataWrapper>>()), Times.Once());
    }

    private class NewNhDataTokenEvent : INewNhDataTokenEvent
    {
      public string NHDataObjectUrl { get; set; }

      public long Id { get; set; }
    }
  }

  // used to mock up httpclient
  public class FakeResponseHandler : DelegatingHandler
  {
    private readonly Dictionary<Uri, HttpResponseMessage> _FakeResponses = new Dictionary<Uri, HttpResponseMessage>();

    public void AddFakeResponse(Uri uri, HttpResponseMessage responseMessage)
    {
      _FakeResponses.Add(uri, responseMessage);
    }

    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
      if (_FakeResponses.ContainsKey(request.RequestUri))
      {
        return _FakeResponses[request.RequestUri];
      }
      else
      {
        return new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };
      }

    }
  }
}
