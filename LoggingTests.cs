using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.UnifiedProductivity.Service.Interfaces;
using VSS.UnifiedProductivity.Service.Utils;
using VSS.VisionLink.Interfaces.Events.Telematics.Machine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using log4netExtensions;
using System.Linq;
using VSS.UnifiedProductivity.Service.DataFeed;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MasterDataConsumer.Tests
{

  [TestClass]
  public class LoggingTests
  {

    [TestMethod]
    public void CanUseLog4net()
    {
      string loggerRepoName = "UnitTestLogTest";
      var logPath = System.IO.Directory.GetCurrentDirectory();

      var logFileFullPath = string.Format(string.Format("{0}/{1}.log", logPath, loggerRepoName));
      if (File.Exists(logFileFullPath))
      {
        File.WriteAllText(logFileFullPath, string.Empty);
      }

      Log4NetAspExtensions.ConfigureLog4Net(logPath, "log4nettest.xml", loggerRepoName);
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();
      loggerFactory.AddLog4Net(loggerRepoName);

      ILogger loggerPre = loggerFactory.CreateLogger<KafkaQueueTests>();
      loggerPre.LogDebug("This test is outside of Container. Should reference KafkaQueueTests.");
      Assert.IsTrue(File.Exists(logFileFullPath));

      IServiceProvider serviceProvider = new ServiceCollection()
        .AddLogging()
        .AddSingleton<ILoggerFactory>(loggerFactory)
        .BuildServiceProvider();

      var retrievedloggerFactory = serviceProvider.GetService<ILoggerFactory>();
      Assert.IsNotNull(retrievedloggerFactory);

      ILogger loggerPost = retrievedloggerFactory.CreateLogger<KafkaQueuePuller>();
      Assert.IsNotNull(retrievedloggerFactory);
      loggerPost.LogDebug("This test is retrieved from Container. Should reference KafkaQueuePuller.");

      System.IO.FileStream fs = new System.IO.FileStream(logFileFullPath, System.IO.FileMode.Open,
          System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite);
      System.IO.StreamReader sr = new System.IO.StreamReader(fs);
      List<string> allLines = new List<string>();
      while (!sr.EndOfStream)
        allLines.Add(sr.ReadLine());

      Assert.AreEqual(2, allLines.Count());
      Assert.AreEqual(2, Regex.Matches(allLines[0], "KafkaQueueTests").Count);
      Assert.AreEqual(2, Regex.Matches(allLines[1], "KafkaQueuePuller").Count);
      DependencyInjectionProvider.CleanDependencyInjection();
      fs.Dispose();
      sr.Dispose();
    }

  }
}
