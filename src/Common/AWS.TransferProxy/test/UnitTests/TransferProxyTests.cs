using Moq;
using System;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using VSS.Common.Abstractions.Configuration;
using Xunit;

namespace VSS.AWS.TransferProxy.UnitTests
{
  public class TransferProxyTests
  {
    [Fact]
    public void TestLoginNoKey()
    {
      // We cannot use Access keys with S3 now, so we have to use assumed roles
      // This test must work without providing any credentials
      const string s3BaseUrl = "https://s3.us-west-2.amazonaws.com/";
      var s3Key = "Test-s3Key" + DateTime.Now.Ticks;
      
      var mockStore = new Mock<IConfigurationStore>();

      mockStore.Setup(x => x.GetValueString("AWS_BUCKET_NAME")).Returns("AWS_BUCKET_NAME");

      var transferProxy = new TransferProxy(mockStore.Object, new NullLogger<TransferProxy>(), "UnitTests");
      var key = transferProxy.GeneratePreSignedUrl(s3Key);
      Assert.NotNull(key);
      // The s3 Url will contain the key, and base url at the start.
      Assert.StartsWith(s3BaseUrl + s3Key, key);
    }

    [Fact]
    public void TestUpload()
    {
      const string originalFileContents = "test-data-please-ignore";
      var s3Key = "/unittests/Upload-test-s3Key" + DateTime.Now.Ticks + Guid.NewGuid();
      var result = TimeSpan.FromHours(1);
      var mockStore = new Mock<IConfigurationStore>();

      mockStore.Setup(x => x.GetValueString("AWS_BUCKET_NAME")).Returns("vss-exports-stg");
      mockStore.Setup(x => x.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY")).Returns(result);
      
      var transferProxy = new TransferProxy(mockStore.Object, new NullLogger<TransferProxy>(), "AWS_BUCKET_NAME");

      using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(originalFileContents)))
      {
        transferProxy.Upload(ms, s3Key);
      }

      var s3ResultStream = transferProxy.Download(s3Key).Result.FileStream;
      var resultMemoryStream = new MemoryStream();
      s3ResultStream.CopyTo(resultMemoryStream);

      resultMemoryStream.Seek(0, SeekOrigin.Begin);

      var text = Encoding.UTF8.GetString(resultMemoryStream.ToArray());
      Assert.Equal(originalFileContents, text);
    }

    [Theory]
    [InlineData(7)]
    [InlineData(0)]
    [InlineData(4)]
    public void Should_not_throw_When_reading_AWS_PRESIGNED_URL_EXPIRY(int expectedResult)
    {
      var result = TimeSpan.FromDays(expectedResult);
      var mockStore = new Mock<IConfigurationStore>();

      mockStore.Setup(x => x.GetValueString("AWS_BUCKET_NAME")).Returns("AWS_BUCKET_NAME");
      mockStore.Setup(x => x.GetValueTimeSpan("AWS_PRESIGNED_URL_EXPIRY")).Returns(result);

      // ReSharper disable once AssignmentIsFullyDiscarded
      _ = new TransferProxy(mockStore.Object, new NullLogger<TransferProxy>(), "AWS_BUCKET_NAME");
    }
  }
}
