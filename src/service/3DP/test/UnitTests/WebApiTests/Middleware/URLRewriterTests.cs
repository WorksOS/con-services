using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.WebApi.Middleware;

namespace VSS.Productivity3D.WebApiTests.Middleware
{
  [TestClass]
  public class URLRewriterTests
  {
    [TestMethod]
    [DataRow("/abc/def/ghi")]
    [DataRow("/abc//def/ghi")]
    [DataRow("//abc")]
    [DataRow("/abc/def//ghi//")]
    public void RewriteMalformedPath_should_parse_out_invalid_characters(string url)
    {
      var request = new DefaultHttpContext().Request;
      request.Method = "GET";
      request.Path = new PathString(url);

      var context = new RewriteContext { HttpContext = request.HttpContext };

      URLRewriter.RewriteMalformedPath(context);

      Assert.IsFalse(request.Path.Value.Contains(@"//"));
    }
  }
}
