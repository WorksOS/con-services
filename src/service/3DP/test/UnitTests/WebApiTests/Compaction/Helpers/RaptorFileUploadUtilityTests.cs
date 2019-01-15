using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Productivity3D.WebApi.Compaction.ActionServices;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class RaptorFileUploadUtilityTests
  {
    private static IServiceProvider serviceProvider;
    private static ILoggerFactory logger;
    private static RaptorFileUploadUtility uploadUtility;

    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);

      serviceProvider = serviceCollection.BuildServiceProvider();
      logger = serviceProvider.GetRequiredService<ILoggerFactory>();

      uploadUtility = new RaptorFileUploadUtility(logger);
    }

    [DataTestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("  ")]
    [DataRow("notpresentfile")]
    public void DeleteFile_should_not_throw_When_file_doesnt_exist(string filename)
    {
      Assert.IsTrue(Task.Run(() => { uploadUtility.DeleteFile(filename); })
                        .Wait(TimeSpan.FromSeconds(1)));
    }

    [TestMethod]
    public void UploadFile_should_return_When_filesize_exceeds_max_limit()
    { 
      var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
      var filename = uploadUtility.GenerateUniqueId();

      Assert.IsNotNull(filePath);

      File.WriteAllBytes(Path.Combine(filePath, filename), new byte[20 * 1024 * 1024 + 1]);

      var fileDescriptor = MasterData.Models.Models.FileDescriptor.CreateFileDescriptor("1", filePath, filename);

      using (var stream = File.OpenRead(Path.Combine(fileDescriptor.Path, fileDescriptor.FileName)))
      {
        var file = new FormFile(stream, 0, stream.Length, null, Path.GetFileName(stream.Name));

        (bool success, string message) = uploadUtility.UploadFile(fileDescriptor, file);

        Assert.IsFalse(success);
        Assert.IsNotNull(message);
      }
    }
  }
}
