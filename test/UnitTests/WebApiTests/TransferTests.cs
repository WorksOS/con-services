using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies;
using VSS.MasterData.Proxies.Interfaces;

namespace VSS.Productivity3D.WebApiTests
{
  [TestClass]
  public class TransferTests
  {
    private static IServiceProvider serviceProvider;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, ErrorCodesProvider>()
        .AddTransient<ITransferProxy, TransferProxy>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      var jobId = "Test_Job_1";
      s3Key = $"3dpm/{jobId}.zip";
    }

    [TestMethod]
    public void CanUploadFileToS3()
    {
      var result = @"{
    ""exportData"": ""UEsDBBQAAAgIAEsQe0tesHMI2AEAANUIAAAIAAAAVGVzdC5jc3bNlM1um0AQgO+V+g4op1aabPeH5Sc3Sn4q1SDLRpZ6stawjVcB1gXiqn21HvpIfYUu1CZSAlVRKsVcZhdmhk/fDvz68TNRhYRQ5nm8Lrp4ZeJVLveiUbo067mo6/i+2MgKZqJuFiJTetaU6Te4lLW6LWNhGkQi3apSwnInZba+K95tu+Sb+TLSmQQTgzRNdG76JaK6bXuG+r5sYCVylc11DTP1uelqwmjV5bSx3UeX827fxg7gcP+6kl/WH75366DYrYs/rZOtSu9KWdfmVQeqGykqWKmNXDaikV1BIk1F+voVxYSex3p/jrmF/QtuXzAHMQ8DoZgjwjBQj1LECQbue4i75glgiIK5hQmchUFihUvuvD8DijDMdNpZsxbJRwi1qGppvcGIYq94Cww+ydpUA8cmlTAX+UC6tbnaJAwLuZemBmLdWMFul6tUbHIJg5juA6btPsJ0qDuGySZi2m3BMzD5gE2PH2xye9ymPc0mRu5jzGtdfRVV9k+YQ4feY7Z6T+PQ+cCh95hs9NB9RKbatJ9jc2g2j5iOwRyx+fKzyRDxj7Ppj39C7jSbBNH/ZPMp5knOZo/J2QHTYadss8c8bZuee7T5l9/7y9vsMV06bpNPw/Qn2PwNUEsBAhQAFAAACAgASxB7S16wcwjYAQAA1QgAAAgAAAAAAAAAAAAgAAAAAAAAAFRlc3QuY3N2UEsFBgAAAAABAAEANgAAAP4BAAAAAA=="",
    ""resultCode"": 0,
    ""Code"": 0,
    ""Message"": ""success""
  }";
      var exportResult = JsonConvert.DeserializeObject<ExportResult>(result);
      var transfer = serviceProvider.GetRequiredService<ITransferProxy>();
      transfer.Upload(new MemoryStream(exportResult.ExportData), s3Key);
    }

    [TestMethod]
    public async Task CanDownloadFileFromS3()
    {
      var transfer = serviceProvider.GetRequiredService<ITransferProxy>();
      var result = await transfer.Download(s3Key);
      using (var reader = new BinaryReader(result.FileStream))
      {
        var exportResult = WebApi.Models.Report.ResultHandling.ExportResult.Create(reader.ReadBytes((int)result.FileStream.Length), 0);
        Assert.AreEqual(0, exportResult.Code);
        Assert.AreEqual("success", exportResult.Message);
        Assert.AreEqual(0, exportResult.ResultCode);
        Assert.IsNotNull(exportResult.ExportData);
        Assert.IsTrue(exportResult.ExportData.Length > 0);
      }
    }

    [TestMethod]
    public void CanGeneratePreSignedUrl()
    {
      var transfer = serviceProvider.GetRequiredService<ITransferProxy>();
      var result = transfer.GeneratePreSignedUrl(s3Key);
      Assert.IsFalse(string.IsNullOrEmpty(result));
      Assert.IsTrue(result.Contains(s3Key));
      //"https://vss-merino.s3.us-west-2.amazonaws.com/3dpm/Test_Job_1.zip?X-Amz-Expires=604799&X-Amz-Algorithm=AWS4-HMAC-SHA256&X-Amz-Credential=AKIAIBGOEETXHMANDX7A/20180417/us-west-2/s3/aws4_request&X-Amz-Date=20180417T042733Z&X-Amz-SignedHeaders=host&X-Amz-Signature=42a7e4052b5b62af324fb5082b88940cdd9e9e3ee29432cede4b808c989c3d94"
    }

    private static string s3Key;
  }
}
