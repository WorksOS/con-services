using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SVOICProfileCell;
using SVOICSummaryVolumesProfileCell;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Utilities;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApiModels.Compaction.Executors;

namespace VSS.Productivity3D.WebApiTests.Compaction.Executors
{
  [TestClass]
  public class CompactionProfileExecutorTests : ExecutorTestsBase
  {
    private const string WrongCellType1 = "wrong cellType 1";
    private const string WrongCellType2 = "wrong cellType 2";
    private const string WrongCellType3 = "wrong cellType 3";
    private const string WrongCellType4 = "wrong cellType 4";
    private const string WrongCellType5 = "wrong cellType 5";
    private const string WrongCellType6 = "wrong cellType 6";
    private const string WrongCellType7 = "wrong cellType 7";
    private const string WrongStation1 = "wrong station 1";
    private const string WrongStation2 = "wrong station 2";
    private const string WrongStation3 = "wrong station 3";
    private const string WrongStation4 = "wrong station 4";
    private const string WrongStation5 = "wrong station 5";
    private const string WrongStation6 = "wrong station 6";
    private const string WrongStation7 = "wrong station 7";
    private const string WrongLastPassHeight1 = "wrong lastPassHeight 1";
    private const string WrongLastPassHeight2 = "wrong lastPassHeight 2";
    private const string WrongLastPassHeight3 = "wrong lastPassHeight 3";
    private const string WrongLastPassHeight4 = "wrong lastPassHeight 4";
    private const string WrongLastPassHeight5 = "wrong lastPassHeight 5";
    private const string WrongLastPassHeight6 = "wrong lastPassHeight 6";
    private const string WrongLastPassHeight7 = "wrong lastPassHeight 7";
    private const string WrongY_1 = "wrong y 1";
    private const string WrongY_2 = "wrong y 2";
    private const string WrongY_3 = "wrong y 3";
    private const string WrongY_4 = "wrong y 4";
    private const string WrongY_5 = "wrong y 5";
    private const string WrongY_6 = "wrong y 6";
    private const string WrongY_7 = "wrong y 7";
    private const string WrongY2_1 = "wrong y2 1";
    private const string WrongY2_2 = "wrong y2 2";
    private const string WrongY2_3 = "wrong y2 3";
    private const string WrongY2_4 = "wrong y2 4";
    private const string WrongY2_5 = "wrong y2 5";
    private const string WrongY2_6 = "wrong y2 6";
    private const string WrongY2_7 = "wrong y2 7";
    private const string WrongValue1 = "wrong value 1";
    private const string WrongValue2 = "wrong value 2";
    private const string WrongValue3 = "wrong value 3";
    private const string WrongValue4 = "wrong value 4";
    private const string WrongValue5 = "wrong value 5";
    private const string WrongValue6 = "wrong value 6";
    private const string WrongValue7 = "wrong value 7";


    public IServiceProvider serviceProvider;

    [TestInitialize]
    public void InitTest()
    {
      ILoggerFactory loggerFactory = new LoggerFactory();
      loggerFactory.AddDebug();

      var serviceCollection = new ServiceCollection();
      serviceCollection.AddLogging();
      serviceCollection.AddSingleton(loggerFactory);
      serviceCollection
        .AddSingleton<IConfigurationStore, GenericConfiguration>()
        .AddTransient<IServiceExceptionHandler, ServiceExceptionHandler>()
        .AddTransient<IErrorCodesProvider, RaptorResult>()
        .AddTransient<ICompactionProfileResultHelper, CompactionProfileResultHelper>();
 
      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    [TestMethod]
    public void ProfileExecutorSlicerNoResult()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var profileResultHelper = serviceProvider.GetRequiredService<ICompactionProfileResultHelper>();

      var raptorClient = new Mock<IASNodeClient>();
    
      raptorClient
        .Setup(x => x.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()))
        .Returns((MemoryStream)null);

      var settingsManager = new CompactionSettingsManager();
      var liftBuildSettings = settingsManager.CompactionLiftBuildSettings(CompactionProjectSettings.DefaultSettings);

      var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty, ProductionDataType.Height, null, -1,
        null, null, null, ValidationConstants3D.MIN_STATION, ValidationConstants3D.MIN_STATION, liftBuildSettings, false, null, null, null, null, null);

      var executor = RequestExecutorContainerFactory
        .Build<CompactionProfileExecutor>(logger, raptorClient.Object, null, null, null, null, null, profileResultHelper);

      var result = executor.Process(request) as CompactionProfileResult<CompactionProfileDataResult>;
      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(0, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);
      Assert.AreEqual(0, result.results.Count, ResultsShouldBeEmpty);
    }

    #region Production Data Profile
    [TestMethod]
    public void ProfileExecutorSlicerInOneCell()
    {
      // O-----O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.085,
            CellLowestElev=597.292F, CellHighestElev=597.629F, CellFirstElev=597.294F, CellLastElev=597.367F, CellLastCompositeElev=597.367F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(2, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.MidPoint, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[1].y, WrongLastPassHeight2);
      
    }


    [TestMethod]
    public void ProfileExecutorSlicesOneEdge()
    {
      // O-----X-------O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.05,
            CellLowestElev=597.2F, CellHighestElev=598.6F, CellFirstElev=597.2F, CellLastElev=596.0F, CellLastCompositeElev=597.3F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.05, InterceptLength= 0.05,
            CellLowestElev=597.4F, CellHighestElev=598.0F, CellFirstElev=597.1F, CellLastElev=597.0F, CellLastCompositeElev=597.8F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);
      
      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(3, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(596.5F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);
    }

    [TestMethod]
    public void ProfileExecutorSlicesTwoEdges()
    {
      // O-----X-------X-----O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=250F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(5, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(200F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                             (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(230F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);
    }


    [TestMethod]
    public void ProfileExecutorSlicerNoGaps()
    {
      // O-----X------X------X------O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=250F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(200F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(220F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result[5].x, WrongStation6);
      Assert.AreEqual(205F, result[5].y, WrongLastPassHeight6);

      Assert.AreEqual(ProfileCellType.MidPoint, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(packager.CellList[3].CellLastElev, result[6].y, WrongLastPassHeight7);
    }


    [TestMethod]
    public void ProfileExecutorWithOneGap()
    {
      // O-----X      X------X------O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
           //Gap here
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(6, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(136F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result[2].x, WrongStation3);
      Assert.AreEqual(172F, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      var expectedStation = packager.CellList[1].Station +
                        (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[3].x, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Edge, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[2].Station, result[4].x, WrongStation5);
      Assert.AreEqual(205F, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.MidPoint, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result[5].x, WrongStation6);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[5].y, WrongLastPassHeight6);
    }

    [TestMethod]
    public void ProfileExecutorWithOnlyAGap()
    {
      // O----X    X-------O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          //Gap here
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(4, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(130F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result[2].x, WrongStation3);
      Assert.AreEqual(160F, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result[3].x, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[3].y, WrongLastPassHeight4);
    }

    [TestMethod]
    public void ProfileExecutorWithTwoGaps()
    {
      // O-------X        X---------X      X-----O

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          //Gap here
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          //Gap here
          new TICProfileCell
          {
            Station= 2.0, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(136F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result[2].x, WrongStation3);
      Assert.AreEqual(172F, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      Assert.AreEqual(1.25, result[3].x, WrongStation4);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Gap, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[1].Station + packager.CellList[1].InterceptLength, result[4].x, WrongStation5);
      Assert.AreEqual(199F, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[2].Station, result[5].x, WrongStation6);
      Assert.AreEqual(217F, result[5].y, WrongLastPassHeight6); 

      Assert.AreEqual(ProfileCellType.MidPoint, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[6].y, WrongLastPassHeight7);
    }

    [TestMethod]
    public void ProfileExecutorWithGapFromNoData()
    {
      // O-----X------X------X------O becomes O-----X      X------X------O


      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          //No data = gap here
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=VelociraptorConstants.NULL_SINGLE, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=200F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=350F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(6, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(packager.CellList[0].CellLastElev, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(140F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[2].Station, result[2].x, WrongStation3);
      Assert.AreEqual(180F, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      var expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[3].x, WrongStation4);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Edge, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[3].Station, result[4].x, WrongStation5);
      Assert.AreEqual(250F, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.MidPoint, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[5].x, WrongStation6);
      Assert.AreEqual(packager.CellList[3].CellLastElev, result[5].y, WrongLastPassHeight6);
    }

    [TestMethod]
    public void ProfileExecutorStartInGapFromNoData()
    {
      // O-----X------X------X------O becomes O     X------X------X------O

      //Tests the extrapolation special case

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=VelociraptorConstants.NULL_SINGLE, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=100F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=200F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=350F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.Gap, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(float.NaN, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(50F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(150F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Edge, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result[5].x, WrongStation6);
      Assert.AreEqual(250F, result[5].y, WrongLastPassHeight6);

      Assert.AreEqual(ProfileCellType.MidPoint, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(packager.CellList[3].CellLastElev, result[6].y, WrongLastPassHeight7);
    }

    [TestMethod]
    public void ProfileExecutorEndInGapFromNoData()
    {
      // O-----X------X------X------O becomes O-----X------X------X      O

      //Tests the extrapolation special case

      TICProfileCellListPackager packager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=70F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 0.5, InterceptLength= 0.5,
            CellLowestElev=150, CellHighestElev=300F, CellFirstElev=240F, CellLastElev=100F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=200F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=VelociraptorConstants.NULL_SINGLE, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetProductionDataProfile(packager);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(70F, result[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      Assert.AreEqual(90F, result[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      Assert.AreEqual(packager.CellList[1].CellLastElev, result[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      Assert.AreEqual(150F, result[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[4].x, WrongStation5);
      Assert.AreEqual(packager.CellList[2].CellLastElev, result[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.Gap, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result[5].x, WrongStation6);
      Assert.AreEqual(250F, result[5].y, WrongLastPassHeight6);

      Assert.AreEqual(ProfileCellType.Gap, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[6].x, WrongStation7);
      Assert.AreEqual(float.NaN, result[6].y, WrongLastPassHeight7);
    }

    private List<CompactionDataPoint> MockGetProductionDataProfile(TICProfileCellListPackager packager)
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var profileResultHelper = serviceProvider.GetRequiredService<ICompactionProfileResultHelper>();

      var raptorClient = new Mock<IASNodeClient>();
      using (var ms = new MemoryStream())
      {
        packager.WriteToStream(ms);
        ms.Position = 0;
        raptorClient
          .Setup(x => x.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()))
          .Returns(ms);

        var settingsManager = new CompactionSettingsManager();
        var liftBuildSettings = settingsManager.CompactionLiftBuildSettings(CompactionProjectSettings.DefaultSettings);

        var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty,
          ProductionDataType.Height, null, -1, null, null, null, ValidationConstants3D.MIN_STATION, ValidationConstants3D.MIN_STATION, 
          liftBuildSettings, false, null, null, null, null, null);

        var executor = RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(logger, raptorClient.Object, null, null, null, null, null, profileResultHelper);
        var result = executor.Process(request) as CompactionProfileResult<CompactionProfileDataResult>;
        Assert.IsNotNull(result, ExecutorFailed);
        Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);

        //NOTE: Tests all use "last pass" to test logic as it's the same for each type of data (last pass, composite, CMV, temperature etc.)
        var lastPassResult = (from r in result.results where r.type == CompactionDataPoint.LAST_PASS select r).SingleOrDefault();
        Assert.IsNotNull(lastPassResult, ExecutorFailed);
        return lastPassResult.data;
      }
    }
    #endregion

    #region Summary Volumes Profile

    [TestMethod]
    [DataRow(VolumeCalcType.GroundToGround)]
    [DataRow(VolumeCalcType.GroundToDesign)]
    [DataRow(VolumeCalcType.DesignToGround)]
    public void ProfileExecutorSummaryVolumesSlicerInOneCell(VolumeCalcType calcType)
    {
      // O-----O

      TICSummaryVolumesProfileCellListPackager packager = new TICSummaryVolumesProfileCellListPackager
      {
        CellList = new TICSummaryVolumesProfileCellList
        {
          //public TICSummaryVolumesProfileCell(float FilteredHeight1, float FilteredHeight2, float DesignHeight, int OTGX, int OTGY, double AStation, double AInterceptLength)
          new TICSummaryVolumesProfileCell(45F, 48F, 46.5F, 0, 0, 0.0, 0.085)
        },
        GridDistanceBetweenProfilePoints = 1.234
      };

      var result = MockGetSummaryVolumesProfile(packager, calcType);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(2, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      float yExpected, y2Expected, valueExpected;
      GetRawExpectedValues(calcType, packager.CellList[0], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      Assert.AreEqual(yExpected, result[0].y, WrongY_1);
      Assert.AreEqual(y2Expected, result[0].y2, WrongY2_1);
      Assert.AreEqual(-valueExpected, result[0].value, WrongValue1);

      Assert.AreEqual(ProfileCellType.MidPoint, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      GetRawExpectedValues(calcType, packager.CellList[0], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      Assert.AreEqual(yExpected, result[1].y, WrongY_2);
      Assert.AreEqual(y2Expected, result[1].y2, WrongY2_2);
      Assert.AreEqual(-valueExpected, result[1].value, WrongValue2);
    }

    [TestMethod]
    [DataRow(VolumeCalcType.GroundToGround)]
    [DataRow(VolumeCalcType.GroundToDesign)]
    [DataRow(VolumeCalcType.DesignToGround)]
    public void ProfileExecutorSummaryVolumesSlicerNoGaps(VolumeCalcType calcType)
    {
      // O-----X------X------X------O

      TICSummaryVolumesProfileCellListPackager packager = new TICSummaryVolumesProfileCellListPackager
      {
        CellList = new TICSummaryVolumesProfileCellList
        {
          //public TICSummaryVolumesProfileCell(float FilteredHeight1, float FilteredHeight2, float DesignHeight, int OTGX, int OTGY, double AStation, double AInterceptLength)
          new TICSummaryVolumesProfileCell(200F, 300F, 43F, 0, 0, 0.0, 0.5),
          new TICSummaryVolumesProfileCell(350F, 450F, 25F, 0, 0, 0.5, 0.5),
          new TICSummaryVolumesProfileCell(290F, 390F, 192F, 0, 0, 1.0, 0.5),
          new TICSummaryVolumesProfileCell(335F, 435F, 79F, 0, 0, 1.5, 0.5)
        },
        GridDistanceBetweenProfilePoints = 1.234
      };
 
      var result = MockGetSummaryVolumesProfile(packager, calcType);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(7, result.Count, IncorrectNumberOfPoints);
      
      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      float yExpected, y2Expected, valueExpected;
      GetRawExpectedValues(calcType, packager.CellList[0], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[0].y, WrongY_1);
      Assert.AreEqual(y2Expected, result[0].y2, WrongY2_1);
      Assert.AreEqual(-valueExpected, result[0].value, WrongValue1);

      Assert.AreEqual(ProfileCellType.Edge, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[1].Station, result[1].x, WrongStation2);
      GetInterpolatedExpectedValues(calcType, packager.CellList[1], out yExpected, out y2Expected, out valueExpected, 300F, 400F);
      Assert.AreEqual(yExpected, result[1].y, WrongY_2);
      Assert.AreEqual(y2Expected, result[1].y2, WrongY2_2);
      Assert.AreEqual(-valueExpected, result[1].value, WrongValue2);

      Assert.AreEqual(ProfileCellType.MidPoint, result[2].cellType, WrongCellType3);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[2].x, WrongStation3);
      GetRawExpectedValues(calcType, packager.CellList[1], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[2].y, WrongY_3);
      Assert.AreEqual(y2Expected, result[2].y2, WrongY2_3);
      Assert.AreEqual(-valueExpected, result[2].value, WrongValue3);

      Assert.AreEqual(ProfileCellType.Edge, result[3].cellType, WrongCellType4);
      Assert.AreEqual(packager.CellList[2].Station, result[3].x, WrongStation4);
      GetInterpolatedExpectedValues(calcType, packager.CellList[2], out yExpected, out y2Expected, out valueExpected, 320F, 420F);
      Assert.AreEqual(yExpected, result[3].y, WrongY_4);
      Assert.AreEqual(y2Expected, result[3].y2, WrongY2_4);
      Assert.AreEqual(-valueExpected, result[3].value, WrongValue4);

      Assert.AreEqual(ProfileCellType.MidPoint, result[4].cellType, WrongCellType5);
      expectedStation = packager.CellList[2].Station +
                        (packager.CellList[3].Station - packager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, result[4].x, WrongStation5);
      GetRawExpectedValues(calcType, packager.CellList[2], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[4].y, WrongY_5);
      Assert.AreEqual(y2Expected, result[4].y2, WrongY2_5);
      Assert.AreEqual(-valueExpected, result[4].value, WrongValue5);

      Assert.AreEqual(ProfileCellType.Edge, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[3].Station, result[5].x, WrongStation6);
      GetInterpolatedExpectedValues(calcType, packager.CellList[3], out yExpected, out y2Expected, out valueExpected, 305F, 405F);
      Assert.AreEqual(yExpected, result[5].y, WrongY_6);
      Assert.AreEqual(y2Expected, result[5].y2, WrongY2_6);
      Assert.AreEqual(-valueExpected, result[5].value, WrongValue6);

      Assert.AreEqual(ProfileCellType.MidPoint, result[6].cellType, WrongCellType7);
      Assert.AreEqual(packager.CellList[3].Station + packager.CellList[3].InterceptLength, result[6].x, WrongStation7);
      GetRawExpectedValues(calcType, packager.CellList[3], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[6].y, WrongY_7);
      Assert.AreEqual(y2Expected, result[6].y2, WrongY2_7);
      Assert.AreEqual(-valueExpected, result[6].value, WrongValue7);
    }

    [TestMethod]
    [DataRow(VolumeCalcType.GroundToGround)]
    [DataRow(VolumeCalcType.GroundToDesign)]
    [DataRow(VolumeCalcType.DesignToGround)]
    public void ProfileExecutorSummaryVolumesWithOneGap(VolumeCalcType calcType)
    {
      // O-----X      X------X------O

      TICSummaryVolumesProfileCellListPackager packager = new TICSummaryVolumesProfileCellListPackager
      {
        CellList = new TICSummaryVolumesProfileCellList
        {
          //public TICSummaryVolumesProfileCell(float FilteredHeight1, float FilteredHeight2, float DesignHeight, int OTGX, int OTGY, double AStation, double AInterceptLength)
          new TICSummaryVolumesProfileCell(200F, 300F, 215F, 0, 0, 0.0, 0.5),
          //Gap here
          new TICSummaryVolumesProfileCell(290F, 390F, 267F, 0, 0, 1.0, 0.5),
          new TICSummaryVolumesProfileCell(335F, 435F, 382F, 0, 0, 1.5, 0.5),
        },
        GridDistanceBetweenProfilePoints = 1.234
      };

      var result = MockGetSummaryVolumesProfile(packager, calcType);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(6, result.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, result[0].cellType, WrongCellType1);
      Assert.AreEqual(packager.CellList[0].Station, result[0].x, WrongStation1);
      float yExpected, y2Expected, valueExpected;
      GetRawExpectedValues(calcType, packager.CellList[0], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[0].y, WrongY_1);
      Assert.AreEqual(y2Expected, result[0].y2, WrongY2_1);
      Assert.AreEqual(-valueExpected, result[0].value, WrongValue1);

      Assert.AreEqual(ProfileCellType.Gap, result[1].cellType, WrongCellType2);
      Assert.AreEqual(packager.CellList[0].Station + packager.CellList[0].InterceptLength, result[1].x, WrongStation2);
      GetInterpolatedExpectedValues(calcType, packager.CellList[0], out yExpected, out y2Expected, out valueExpected, 236F, 336F);
      Assert.AreEqual(yExpected, result[1].y, WrongY_2);
      Assert.AreEqual(y2Expected, result[1].y2, WrongY2_2);
      Assert.AreEqual(float.NaN, result[1].value, WrongValue2);//Gap has NaN value

      Assert.AreEqual(ProfileCellType.Edge, result[2].cellType, WrongCellType3);
      Assert.AreEqual(packager.CellList[1].Station, result[2].x, WrongStation3);
      GetInterpolatedExpectedValues(calcType, packager.CellList[1], out yExpected, out y2Expected, out valueExpected, 272F, 372F);
      Assert.AreEqual(yExpected, result[2].y, WrongY_3);
      Assert.AreEqual(y2Expected, result[2].y2, WrongY2_3);
      Assert.AreEqual(-valueExpected, result[2].value, WrongValue3);

      Assert.AreEqual(ProfileCellType.MidPoint, result[3].cellType, WrongCellType4);
      var expectedStation = packager.CellList[1].Station +
                            (packager.CellList[2].Station - packager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, result[3].x, WrongStation4);
      GetRawExpectedValues(calcType, packager.CellList[1], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[3].y, WrongY_4);
      Assert.AreEqual(y2Expected, result[3].y2, WrongY2_4);
      Assert.AreEqual(-valueExpected, result[3].value, WrongValue4);

      Assert.AreEqual(ProfileCellType.Edge, result[4].cellType, WrongCellType5);
      Assert.AreEqual(packager.CellList[2].Station, result[4].x, WrongStation5);
      GetInterpolatedExpectedValues(calcType, packager.CellList[2], out yExpected, out y2Expected, out valueExpected, 305F, 405F);
      Assert.AreEqual(yExpected, result[4].y, WrongY_5);
      Assert.AreEqual(y2Expected, result[4].y2, WrongY2_5);
      Assert.AreEqual(-valueExpected, result[4].value, WrongValue5);

      Assert.AreEqual(ProfileCellType.MidPoint, result[5].cellType, WrongCellType6);
      Assert.AreEqual(packager.CellList[2].Station + packager.CellList[2].InterceptLength, result[5].x, WrongStation6);
      GetRawExpectedValues(calcType, packager.CellList[2], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, result[5].y, WrongY_6);
      Assert.AreEqual(y2Expected, result[5].y2, WrongY2_6);
      Assert.AreEqual(-valueExpected, result[5].value, WrongValue6);
    }

    private void GetRawExpectedValues(VolumeCalcType calcType, TICSummaryVolumesProfileCell cell, out float yExpected, out float y2Expected, out float valueExpected)
    {
      yExpected = float.NaN;
      y2Expected = float.NaN;
      valueExpected = float.NaN;
      switch (calcType)
      {
        case VolumeCalcType.GroundToGround:
          yExpected = cell.LastCellPassElevation1;
          y2Expected = cell.LastCellPassElevation2;
          valueExpected = y2Expected - yExpected;
          break;
        case VolumeCalcType.GroundToDesign:
          yExpected = cell.LastCellPassElevation1;
          //y2 is NaN as it will be set layer using the design
          y2Expected = float.NaN;
          valueExpected = cell.DesignElevation - yExpected;
          break;
        case VolumeCalcType.DesignToGround:
          //y is NaN as it will be set layer using the design
          yExpected = float.NaN;
          y2Expected = cell.LastCellPassElevation2;
          valueExpected = y2Expected - cell.DesignElevation;
          break;
      }
    }

    private void GetInterpolatedExpectedValues(VolumeCalcType calcType, TICSummaryVolumesProfileCell cell, out float yExpected, out float y2Expected, out float valueExpected, float yInterpolated, float y2Interpolated)
    {
      GetRawExpectedValues(calcType, cell, out yExpected, out y2Expected, out valueExpected);
      bool useY = calcType != VolumeCalcType.DesignToGround;
      bool useY2 = calcType != VolumeCalcType.GroundToDesign;
      yExpected = useY ? yInterpolated : yExpected;
      y2Expected = useY2 ? y2Interpolated : y2Expected;
    }

    private List<CompactionDataPoint> MockGetSummaryVolumesProfile(TICSummaryVolumesProfileCellListPackager packager, VolumeCalcType calcType)
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var profileResultHelper = serviceProvider.GetRequiredService<ICompactionProfileResultHelper>();

      var raptorClient = new Mock<IASNodeClient>();
      using (var ms = new MemoryStream())
      {
        packager.WriteToStream(ms);
        ms.Position = 0;
        raptorClient
          .Setup(x => x.GetSummaryVolumesProfile(It.IsAny<ASNode.RequestSummaryVolumesProfile.RPC.TASNodeServiceRPCVerb_RequestSummaryVolumesProfile_Args>()))
          .Returns(ms);

        var settingsManager = new CompactionSettingsManager();
        var liftBuildSettings = settingsManager.CompactionLiftBuildSettings(CompactionProjectSettings.DefaultSettings);

        var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty,
          ProductionDataType.Height, null, -1, null, null, null, ValidationConstants3D.MIN_STATION, ValidationConstants3D.MIN_STATION,
          liftBuildSettings, false, null, null, null, calcType, null);

        var executor = RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(logger, raptorClient.Object, null, null, null, null, null, profileResultHelper);
        var result = executor.Process(request) as CompactionProfileResult<CompactionProfileDataResult>;
        Assert.IsNotNull(result, ExecutorFailed);
        Assert.AreEqual(packager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);

        var sumVolResult = (from r in result.results where r.type == CompactionDataPoint.SUMMARY_VOLUMES select r).SingleOrDefault();
        Assert.IsNotNull(sumVolResult, ExecutorFailed);
        return sumVolResult.data;
      }
    }
    #endregion

    #region Combined ProductionData and Summary Volumes Profile
    [TestMethod]
    [DataRow(VolumeCalcType.GroundToGround)]
    [DataRow(VolumeCalcType.GroundToDesign)]
    [DataRow(VolumeCalcType.DesignToGround)]
    public void ProfileExecutorCombinedSlicer(VolumeCalcType calcType)
    {
      //Summary Volumes - No Gaps
      // O-----X------X------X------O
      //Production Data - One Gap
      // O-----X      X------X------O

      TICSummaryVolumesProfileCellListPackager svPackager = new TICSummaryVolumesProfileCellListPackager
      {
        CellList = new TICSummaryVolumesProfileCellList
        {
          //public TICSummaryVolumesProfileCell(float FilteredHeight1, float FilteredHeight2, float DesignHeight, int OTGX, int OTGY, double AStation, double AInterceptLength)
          new TICSummaryVolumesProfileCell(200F, 300F, 43F, 0, 0, 0.0, 0.5),
          new TICSummaryVolumesProfileCell(350F, 450F, 25F, 0, 0, 0.5, 0.5),
          new TICSummaryVolumesProfileCell(290F, 390F, 192F, 0, 0, 1.0, 0.5),
          new TICSummaryVolumesProfileCell(335F, 435F, 79F, 0, 0, 1.5, 0.5)
        },
        GridDistanceBetweenProfilePoints = 1.234
      };

      TICProfileCellListPackager pdPackager = new TICProfileCellListPackager
      {
        CellList = new TICProfileCellList
        {
          new TICProfileCell
          {
            Station= 0.000, InterceptLength= 0.5,
            CellLowestElev=100F, CellHighestElev=200F, CellFirstElev=120F, CellLastElev=100F, CellLastCompositeElev=200F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          //Gap here
          new TICProfileCell
          {
            Station= 1.0, InterceptLength= 0.5,
            CellLowestElev=200F, CellHighestElev=350F, CellFirstElev=360F, CellLastElev=190F, CellLastCompositeElev=400F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          },
          new TICProfileCell
          {
            Station= 1.5, InterceptLength= 0.5,
            CellLowestElev=210F, CellHighestElev=320F, CellFirstElev=320F, CellLastElev=235F, CellLastCompositeElev=300F,
            DesignElev=0F,
            CellCCV=0, CellTargetCCV=0, CellCCVElev=0, CellPreviousMeasuredCCV=0,
            CellMDP=0, CellTargetMDP=0, CellMDPElev=0,
            CellMaterialTemperature=0, CellMaterialTemperatureWarnMin=0, CellMaterialTemperatureWarnMax=0, CellMaterialTemperatureElev=0,
            TopLayerPassCount=0, TopLayerPassCountTargetRangeMin=0, TopLayerPassCountTargetRangeMax=0,
            CellMaxSpeed = 0, CellMinSpeed = 0
          }
        },
        GridDistanceBetweenProfilePoints = 1.234,
        WriteCellPassesAndLayers = false,
        LatLongList = new TWGS84StationPoint[0]
      };

      var result = MockGetCombinedProfile(pdPackager, svPackager, calcType);

      Assert.IsNotNull(result, ExecutorFailed);
      Assert.AreEqual(CompactionDataPoint.TOTAL_NUMBER_OF_PROFILES, result.results.Count, "Wrong number of profiles");

      //Production data
      var lastPassResult = (from r in result.results where r.type == CompactionDataPoint.LAST_PASS select r).SingleOrDefault();
      Assert.IsNotNull(lastPassResult, ExecutorFailed);
      var lastPassData = lastPassResult.data;
      Assert.AreEqual(6, lastPassData.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, lastPassData[0].cellType, WrongCellType1);
      Assert.AreEqual(pdPackager.CellList[0].Station, lastPassData[0].x, WrongStation1);
      Assert.AreEqual(pdPackager.CellList[0].CellLastElev, lastPassData[0].y, WrongLastPassHeight1);

      Assert.AreEqual(ProfileCellType.Gap, lastPassData[1].cellType, WrongCellType2);
      Assert.AreEqual(pdPackager.CellList[0].Station + pdPackager.CellList[0].InterceptLength, lastPassData[1].x, WrongStation2);
      Assert.AreEqual(136F, lastPassData[1].y, WrongLastPassHeight2);

      Assert.AreEqual(ProfileCellType.Edge, lastPassData[2].cellType, WrongCellType3);
      Assert.AreEqual(pdPackager.CellList[1].Station, lastPassData[2].x, WrongStation3);
      Assert.AreEqual(172F, lastPassData[2].y, WrongLastPassHeight3);

      Assert.AreEqual(ProfileCellType.MidPoint, lastPassData[3].cellType, WrongCellType4);
      var expectedStation = pdPackager.CellList[1].Station +
                            (pdPackager.CellList[2].Station - pdPackager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, lastPassData[3].x, WrongStation4);
      Assert.AreEqual(pdPackager.CellList[1].CellLastElev, lastPassData[3].y, WrongLastPassHeight4);

      Assert.AreEqual(ProfileCellType.Edge, lastPassData[4].cellType, WrongCellType5);
      Assert.AreEqual(pdPackager.CellList[2].Station, lastPassData[4].x, WrongStation5);
      Assert.AreEqual(205F, lastPassData[4].y, WrongLastPassHeight5);

      Assert.AreEqual(ProfileCellType.MidPoint, lastPassData[5].cellType, WrongCellType6);
      Assert.AreEqual(pdPackager.CellList[2].Station + pdPackager.CellList[2].InterceptLength, lastPassData[5].x, WrongStation6);
      Assert.AreEqual(pdPackager.CellList[2].CellLastElev, lastPassData[5].y, WrongLastPassHeight6);

      //Summary Volumes
      var sumVolResult = (from r in result.results where r.type == CompactionDataPoint.SUMMARY_VOLUMES select r).SingleOrDefault();
      Assert.IsNotNull(sumVolResult, ExecutorFailed);
      var sumVolData = sumVolResult.data;
      Assert.AreEqual(7, sumVolData.Count, IncorrectNumberOfPoints);

      Assert.AreEqual(ProfileCellType.MidPoint, sumVolData[0].cellType, WrongCellType1);
      Assert.AreEqual(svPackager.CellList[0].Station, sumVolData[0].x, WrongStation1);
      float yExpected, y2Expected, valueExpected;
      GetRawExpectedValues(calcType, svPackager.CellList[0], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, sumVolData[0].y, WrongY_1);
      Assert.AreEqual(y2Expected, sumVolData[0].y2, WrongY2_1);
      Assert.AreEqual(-valueExpected, sumVolData[0].value, WrongValue1);

      Assert.AreEqual(ProfileCellType.Edge, sumVolData[1].cellType, WrongCellType2);
      Assert.AreEqual(svPackager.CellList[1].Station, sumVolData[1].x, WrongStation2);
      GetInterpolatedExpectedValues(calcType, svPackager.CellList[1], out yExpected, out y2Expected, out valueExpected, 300F, 400F);
      Assert.AreEqual(yExpected, sumVolData[1].y, WrongY_2);
      Assert.AreEqual(y2Expected, sumVolData[1].y2, WrongY2_2);
      Assert.AreEqual(-valueExpected, sumVolData[1].value, WrongValue2);

      Assert.AreEqual(ProfileCellType.MidPoint, sumVolData[2].cellType, WrongCellType3);
      expectedStation = svPackager.CellList[1].Station +
                            (svPackager.CellList[2].Station - svPackager.CellList[1].Station) / 2;
      Assert.AreEqual(expectedStation, sumVolData[2].x, WrongStation3);
      GetRawExpectedValues(calcType,svPackager.CellList[1], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, sumVolData[2].y, WrongY_3);
      Assert.AreEqual(y2Expected, sumVolData[2].y2, WrongY2_3);
      Assert.AreEqual(-valueExpected, sumVolData[2].value, WrongValue3);

      Assert.AreEqual(ProfileCellType.Edge, sumVolData[3].cellType, WrongCellType4);
      Assert.AreEqual(svPackager.CellList[2].Station, sumVolData[3].x, WrongStation4);
      GetInterpolatedExpectedValues(calcType, svPackager.CellList[2], out yExpected, out y2Expected, out valueExpected, 320F, 420F);
      Assert.AreEqual(yExpected, sumVolData[3].y, WrongY_4);
      Assert.AreEqual(y2Expected, sumVolData[3].y2, WrongY2_4);
      Assert.AreEqual(-valueExpected, sumVolData[3].value, WrongValue4);

      Assert.AreEqual(ProfileCellType.MidPoint, sumVolData[4].cellType, WrongCellType5);
      expectedStation = svPackager.CellList[2].Station +
                        (svPackager.CellList[3].Station - svPackager.CellList[2].Station) / 2;
      Assert.AreEqual(expectedStation, sumVolData[4].x, WrongStation5);
      GetRawExpectedValues(calcType, svPackager.CellList[2], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, sumVolData[4].y, WrongY_5);
      Assert.AreEqual(y2Expected, sumVolData[4].y2, WrongY2_5);
      Assert.AreEqual(-valueExpected, sumVolData[4].value, WrongValue5);

      Assert.AreEqual(ProfileCellType.Edge, sumVolData[5].cellType, WrongCellType6);
      Assert.AreEqual(svPackager.CellList[3].Station, sumVolData[5].x, WrongStation6);
      GetInterpolatedExpectedValues(calcType, svPackager.CellList[3], out yExpected, out y2Expected, out valueExpected, 305F, 405F);
      Assert.AreEqual(yExpected, sumVolData[5].y, WrongY_6);
      Assert.AreEqual(y2Expected, sumVolData[5].y2, WrongY2_6);
      Assert.AreEqual(-valueExpected, sumVolData[5].value, WrongValue6);

      Assert.AreEqual(ProfileCellType.MidPoint, sumVolData[6].cellType, WrongCellType7);
      Assert.AreEqual(svPackager.CellList[3].Station + svPackager.CellList[3].InterceptLength, sumVolData[6].x, WrongStation7);
      GetRawExpectedValues(calcType, svPackager.CellList[3], out yExpected, out y2Expected, out valueExpected);
      Assert.AreEqual(yExpected, sumVolData[6].y, WrongY_7);
      Assert.AreEqual(y2Expected, sumVolData[6].y2, WrongY2_7);
      Assert.AreEqual(-valueExpected, sumVolData[6].value, WrongValue7);
    }

    private CompactionProfileResult<CompactionProfileDataResult> MockGetCombinedProfile(TICProfileCellListPackager pdPackager, TICSummaryVolumesProfileCellListPackager svPackager, VolumeCalcType calcType)
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      var profileResultHelper = serviceProvider.GetRequiredService<ICompactionProfileResultHelper>();

      var raptorClient = new Mock<IASNodeClient>();
      using (var msProdData = new MemoryStream())
      using (var msSumVol = new MemoryStream())
      {
        pdPackager.WriteToStream(msProdData);
        msProdData.Position = 0;
        raptorClient
          .Setup(x => x.GetProfile(It.IsAny<ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args>()))
          .Returns(msProdData);

        svPackager.WriteToStream(msSumVol);
        msSumVol.Position = 0;
        raptorClient
          .Setup(x => x.GetSummaryVolumesProfile(It.IsAny<ASNode.RequestSummaryVolumesProfile.RPC.TASNodeServiceRPCVerb_RequestSummaryVolumesProfile_Args>()))
          .Returns(msSumVol);

        var settingsManager = new CompactionSettingsManager();
        var liftBuildSettings = settingsManager.CompactionLiftBuildSettings(CompactionProjectSettings.DefaultSettings);

        var request = CompactionProfileProductionDataRequest.CreateCompactionProfileProductionDataRequest(1234, Guid.Empty,
          ProductionDataType.Height, null, -1, null, null, null, ValidationConstants3D.MIN_STATION, ValidationConstants3D.MIN_STATION,
          liftBuildSettings, false, null, null, null, calcType, null);

        var executor = RequestExecutorContainerFactory
          .Build<CompactionProfileExecutor>(logger, raptorClient.Object, null, null, null, null, null, profileResultHelper);
        var result = executor.Process(request) as CompactionProfileResult<CompactionProfileDataResult>;
        Assert.IsNotNull(result, ExecutorFailed);
        Assert.AreEqual(pdPackager.GridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints, WrongGridDistanceBetweenProfilePoints);

        return result;
      }
    }
    #endregion
  }
}
