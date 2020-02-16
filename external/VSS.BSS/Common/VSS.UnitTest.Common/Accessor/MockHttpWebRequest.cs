using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace VSS.UnitTest.Common.Contexts
{
  public class MockHttpWebRequest : IWebRequestCreate
  {
    private WebRequest nextRequest;
    public WebRequest Create(Uri uri)
    {
      return nextRequest;
    }

    public MockWebRequest CreateMockWebRequest(string response)
    {
      MockWebRequest request = new MockWebRequest(response);
      nextRequest = request;
      return request;
    }
  }

  public class MockWebRequest : WebRequest
  {
    MemoryStream requestStream = new MemoryStream();
    string responseStream;

    public override string Method { get; set; }
    public override string ContentType { get; set; }
    public override long ContentLength { get; set; }
    public override int Timeout { get; set; }
    
    /// <summary>Initializes a new instance of <see cref="MockWebRequest"/>
    /// with the response to return.</summary>
    public MockWebRequest(string response)
    {
      responseStream = response;
    }

    /// <summary>Returns the request contents as a string.</summary>
    public string ContentAsString()
    {
      return System.Text.Encoding.UTF8.GetString(requestStream.ToArray());
    }

    /// <summary>See <see cref="WebRequest.GetRequestStream"/>.</summary>
    public override Stream GetRequestStream()
    {
      return new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseStream));
    }

    /// <summary>See <see cref="WebRequest.GetResponse"/>.</summary>
    public override WebResponse GetResponse()
    {
      return new MockWebReponse(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(responseStream)));
    }
  }

  class MockWebReponse : WebResponse
  {
    Stream responseStream;

    /// <summary>Initializes a new instance of <see cref="MockWebReponse"/>
    /// with the response stream to return.</summary>
    public MockWebReponse(Stream responseStream)
    {
      this.responseStream = responseStream;
    }

    /// <summary>See <see cref="WebResponse.GetResponseStream"/>.</summary>
    public override Stream GetResponseStream()
    {
      return responseStream;
    }
  }
}
