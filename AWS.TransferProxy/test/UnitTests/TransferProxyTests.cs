using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using VSS.ConfigurationStore;

namespace VSS.AWS.TransferProxy.UnitTests
{
  [TestClass]
  public class TransferProxyTests
  {
    [TestMethod]
    [ExpectedException(typeof(Exception))]
    [DataRow(null)]
    [DataRow("")]
    public void Should_throw_When_AWS_ACCESS_KEY_isnt_present(string value)
    {
      var mockStore = new Mock<IConfigurationStore>();
      mockStore.Setup(x => x.GetValueString("AWS_ACCESS_KEY")).Returns(value);

      _ = new TransferProxy(mockStore.Object);
    }
    
    [TestMethod]
    [ExpectedException(typeof(Exception))]
    [DataRow(null)]
    [DataRow("")]
    public void Should_throw_When_AWS_SECRET_KEY_isnt_present(string value)
    {
      var mockStore = new Mock<IConfigurationStore>();

      mockStore.Setup(x => x.GetValueString("AWS_ACCESS_KEY")).Returns("AWS_ACCESS_KEY");
      mockStore.Setup(x => x.GetValueString("AWS_SECRET_KEY")).Returns(value);

      _ = new TransferProxy(mockStore.Object);
    }    

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    [DataRow(null)]
    [DataRow("")]
    public void Should_throw_When_AWS_BUCKET_NAME_isnt_present(string value)
    {
      var mockStore = new Mock<IConfigurationStore>();

      mockStore.Setup(x => x.GetValueString("AWS_ACCESS_KEY")).Returns("AWS_ACCESS_KEY");
      mockStore.Setup(x => x.GetValueString("AWS_SECRET_KEY")).Returns("AWS_SECRET_KEY");
      mockStore.Setup(x => x.GetValueString("AWS_BUCKET_NAME")).Returns(value);

      _ = new TransferProxy(mockStore.Object);
    }  

    [TestMethod]
    [DataRow(null, 7)]
    [DataRow("", 7)]
    [DataRow("0", 0)]
    [DataRow("4", 4)]
    public void Should_not_throw_When_reading_AWS_PRESIGNED_URL_EXPIRY(string value, int expectedResult)
    {
      var result = TimeSpan.FromDays(expectedResult);
      var mockStore = new Mock<IConfigurationStore>();

      mockStore.Setup(x => x.GetValueString("AWS_ACCESS_KEY")).Returns("AWS_ACCESS_KEY");
      mockStore.Setup(x => x.GetValueString("AWS_SECRET_KEY")).Returns("AWS_SECRET_KEY");
      mockStore.Setup(x => x.GetValueString("AWS_BUCKET_NAME")).Returns("AWS_BUCKET_NAME");
      mockStore.Setup(x => x.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY")).Returns(result);

      _ = new TransferProxy(mockStore.Object);
    }
  }
}
