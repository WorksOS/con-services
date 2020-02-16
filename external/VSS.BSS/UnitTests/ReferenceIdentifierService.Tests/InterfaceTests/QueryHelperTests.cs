using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Helpers;
using VSS.Nighthawk.ReferenceIdentifierService.Interfaces.Implementations.Helpers;

namespace VSS.Nighthawk.ReferenceIdentifierService.Tests.InterfaceTests
{
  [TestClass]
  public class QueryHelperTests
  {
    [TestInitialize]
    public void TestInitialize()
    {
      Mock<IHttpClientWrapper> _mockIHttpClientWrapper = new Mock<IHttpClientWrapper>();
      Mock<IHttpResponseWrapper> _mockIHttpResponseWrapper = new Mock<IHttpResponseWrapper>();
      IQueryHelper _queryHelper = new QueryHelper(_mockIHttpClientWrapper.Object);
    }
  }
}
