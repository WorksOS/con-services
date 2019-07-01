#if NET_4_7 
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.Hydrology.WebApi.Common.Utilities;
using VSS.Hydrology.WebApi.DXF;
using VSS.Hydrology.WebApi.DXF.Header;
using VSS.Hydrology.WebApi.TTM;

namespace VSS.Hydrology.Tests.Ponding.FileConverter
{
  [TestClass]
  public class TTMToDXFConverterTests
  {
    private static IServiceProvider serviceProvider;
    private static ILoggerFactory loggerFactory;
    private const string DesignSurfaceFilePath = "..\\..\\..\\..\\TestData\\DesignSurfaceGoodContent.ttm";
    private const int DesignSurfaceGoodContentTriangleCount = 2;
    private const string AlphaDimensionsFilePath = "..\\..\\..\\..\\TestData\\AlphaDimensions2012_milling_surface5.ttm";
    private const int AlphaDimensions2012TriangleCount = 2297;


    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
      ILoggerFactory logFactory = new LoggerFactory();
      logFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(logFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>();
        //.AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>() // todoJeannie tests for exceptions
        //.AddTransient<IErrorCodesProvider, RaptorResult>();

      serviceProvider = serviceCollection.BuildServiceProvider();

      loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
    }

    [TestMethod]
    public void ReadSmallTTMfromFile()
    {
      var ttmLocalPathAndFileName = DesignSurfaceFilePath;
      
      if (!File.Exists(ttmLocalPathAndFileName))
        Assert.Fail("ttmLocalPathAndFileName doesn't exist");

      var tin = new TrimbleTINModel();
      tin.LoadFromFile(ttmLocalPathAndFileName);
      
      Assert.AreEqual(DesignSurfaceGoodContentTriangleCount, tin.Triangles.Count, "wrong triangle count");
    }

    [TestMethod]
    public void ReadLargeTTMfromStream()
    {
      var ttmLocalPathAndFileName = AlphaDimensionsFilePath;
      
      if (!File.Exists(ttmLocalPathAndFileName))
        Assert.Fail("ttmLocalPathAndFileName doesn't exist");

      var tin = new TrimbleTINModel();
      using (var ms = new MemoryStream(File.ReadAllBytes(ttmLocalPathAndFileName)))
      {
        tin.LoadFromStream(ms);
      }

      Assert.AreEqual(AlphaDimensions2012TriangleCount, tin.Triangles.Count, "wrong triangle count");
    }

    [TestMethod]
    public void ReadSmallTTMWriteDXF()
    {
      var ttmLocalPathAndFileName = DesignSurfaceFilePath;
      if (!File.Exists(ttmLocalPathAndFileName))
        Assert.Fail("ttmLocalPathAndFileName doesn't exist");

      var projectUid = Guid.NewGuid();
      var localProjectPath = FilePathHelper.GetTempFolderForProject(projectUid);
      var dxfLocalPathAndFileName = Path.Combine(new[] { localProjectPath, Path.GetFileNameWithoutExtension(ttmLocalPathAndFileName) + ".dxf" });

      if (!File.Exists(ttmLocalPathAndFileName))
        Assert.Fail("ttmLocalPathAndFileName doesn't exist");
      
      if (File.Exists(dxfLocalPathAndFileName))
        File.Delete(dxfLocalPathAndFileName);

      var converter = new TTMtoDXFConverter(loggerFactory);
      using (var ms = new MemoryStream(File.ReadAllBytes(ttmLocalPathAndFileName)))
      {
        converter.WriteDXFFromTTMStream(ms, dxfLocalPathAndFileName);
      }
      Assert.AreEqual(DesignSurfaceGoodContentTriangleCount, converter.TTMTriangleCount());
      Assert.AreEqual(DesignSurfaceGoodContentTriangleCount, converter.DXFTriangleCount());

      var dxfVersion = DxfDocument.CheckDxfFileVersion(dxfLocalPathAndFileName);
      Assert.AreEqual(DxfVersion.AutoCad2000, dxfVersion, $"incorrect DXF file version. expected {DxfVersion.AutoCad2000} but got {dxfVersion}");

      var dxf = DxfDocument.Load(dxfLocalPathAndFileName);
      Assert.AreEqual(DesignSurfaceGoodContentTriangleCount, dxf.Faces3d.Count());
      if (Directory.Exists(localProjectPath))
        Directory.Delete(localProjectPath, true);
    }

    [TestMethod]
    public void ReadLargeTTMWriteDXF()
    {
      var ttmLocalPathAndFileName = AlphaDimensionsFilePath;
      if (!File.Exists(ttmLocalPathAndFileName))
        Assert.Fail("ttmLocalPathAndFileName doesn't exist");

      var projectUid = Guid.NewGuid();
      var localProjectPath = FilePathHelper.GetTempFolderForProject(projectUid);
      var dxfLocalPathAndFileName = Path.Combine(new[] { localProjectPath, Path.GetFileNameWithoutExtension(ttmLocalPathAndFileName) + ".dxf" });

      if (!File.Exists(ttmLocalPathAndFileName))
        Assert.Fail("ttmLocalPathAndFileName doesn't exist");

      if (File.Exists(dxfLocalPathAndFileName))
        File.Delete(dxfLocalPathAndFileName);

      var converter = new TTMtoDXFConverter(loggerFactory);
      using (var ms = new MemoryStream(File.ReadAllBytes(ttmLocalPathAndFileName)))
      {
        converter.WriteDXFFromTTMStream(ms, dxfLocalPathAndFileName);
      }
      Assert.AreEqual(AlphaDimensions2012TriangleCount, converter.TTMTriangleCount());
      Assert.AreEqual(AlphaDimensions2012TriangleCount, converter.DXFTriangleCount());

      var dxfVersion = DxfDocument.CheckDxfFileVersion(dxfLocalPathAndFileName);
      Assert.AreEqual(DxfVersion.AutoCad2000, dxfVersion, $"incorrect DXF file version. expected {DxfVersion.AutoCad2000} but got {dxfVersion}");

      var dxf = DxfDocument.Load(dxfLocalPathAndFileName);
      Assert.AreEqual(AlphaDimensions2012TriangleCount, dxf.Faces3d.Count());
      if (Directory.Exists(localProjectPath))
        Directory.Delete(localProjectPath, true);
    }
  }
}

#endif
