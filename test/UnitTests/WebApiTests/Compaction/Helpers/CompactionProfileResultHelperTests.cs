using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class CompactionProfileResultHelperTests
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
        .AddTransient<IErrorCodesProvider, RaptorResult>();

      serviceProvider = serviceCollection.BuildServiceProvider();
    }

    #region FindCutFillElevations tests
    [TestMethod]
    [DataRow(CompactionDataPoint.CUT_FILL, VolumeCalcType.None)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void NoProdDataAndNoDesignProfile(string profileType, VolumeCalcType calcType)
    {
      CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>()
            }
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>();

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult, profileType, calcType);

      Assert.AreEqual(1, slicerProfileResult.results.Count, "Wrong number of profiles");
      var actualPoints = slicerProfileResult.results[0].data;
      Assert.AreEqual(0, actualPoints.Count, "Wrong number of profile points");
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.CUT_FILL, VolumeCalcType.None)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void NoDesignProfileShouldNotChangeProdData(string profileType, VolumeCalcType calcType)
    {
      CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint {x = 0, y = float.NaN, y2 = float.NaN},
                new CompactionDataPoint {x = 1, y = float.NaN, y2 = float.NaN},
                new CompactionDataPoint {x = 2, y = float.NaN, y2 = float.NaN},
              }
            }
          }
        };  
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>();

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult, profileType, calcType);

      Assert.AreEqual(1, slicerProfileResult.results.Count, "Wrong number of profiles");
      var actualPoints = slicerProfileResult.results[0].data;
      Assert.AreEqual(3, actualPoints.Count, "Wrong number of profile points");
      for (int i = 0; i < 3; i++)
      {
        Assert.AreEqual(float.NaN, actualPoints[i].y, $"{i}: Wrong y height");
        Assert.AreEqual(float.NaN, actualPoints[i].y2, $"{i}: Wrong y2 height");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.CUT_FILL, VolumeCalcType.None)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void CellStationsOutsideDesign(string profileType, VolumeCalcType calcType)
    {
      CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ x = 0, y = float.NaN, y2 = float.NaN},
                new CompactionDataPoint{ x = 1, y = float.NaN, y2 = float.NaN},
                new CompactionDataPoint{ x = 2, y = float.NaN, y2 = float.NaN},
                new CompactionDataPoint{ x = 3, y = float.NaN, y2 = float.NaN},
              }
            }
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          results = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex {station = 0.5, elevation = 10},
            new CompactionProfileVertex {station = 1.5, elevation = 20},
            new CompactionProfileVertex {station = 2.5, elevation = 40},
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult, profileType, calcType);

      Assert.AreEqual(1, slicerProfileResult.results.Count, "Wrong number of profiles");
      var actualPoints = slicerProfileResult.results[0].data;
      Assert.AreEqual(4, actualPoints.Count, "Wrong number of profile points");
      Assert.AreEqual(float.NaN, calcType == VolumeCalcType.DesignToGround ? actualPoints[0].y : actualPoints[0].y2, "0: Wrong cut-fill height");
      Assert.AreEqual(15, calcType == VolumeCalcType.DesignToGround ? actualPoints[1].y : actualPoints[1].y2, "1: Wrong cut-fill height");
      Assert.AreEqual(30, calcType == VolumeCalcType.DesignToGround ? actualPoints[2].y : actualPoints[2].y2, "2: Wrong cut-fill height");
      Assert.AreEqual(float.NaN, calcType == VolumeCalcType.DesignToGround ? actualPoints[3].y : actualPoints[3].y2, "3: Wrong cut-fill height");
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.CUT_FILL, VolumeCalcType.None)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void CellStationsMatchDesign(string profileType, VolumeCalcType calcType)
    {
      CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ x = 0, y2 = float.NaN},
                new CompactionDataPoint{ x = 1, y2 = float.NaN},
                new CompactionDataPoint{ x = 2, y2 = float.NaN},
              }
            }
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          results = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex {station = 0, elevation = 10},
            new CompactionProfileVertex {station = 0.5, elevation = 30},
            new CompactionProfileVertex {station = 1.0, elevation = 20},
            new CompactionProfileVertex {station = 1.5, elevation = 10},
            new CompactionProfileVertex {station = 2.0, elevation = 40},
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult, profileType, calcType);

      Assert.AreEqual(1, slicerProfileResult.results.Count, "Wrong number of profiles");
      var actualPoints = slicerProfileResult.results[0].data;
      Assert.AreEqual(3, actualPoints.Count, "Wrong number of profile points");
      Assert.AreEqual(10, calcType == VolumeCalcType.DesignToGround ? actualPoints[0].y : actualPoints[0].y2, "0: Wrong cut-fill height");
      Assert.AreEqual(20, calcType == VolumeCalcType.DesignToGround ? actualPoints[1].y : actualPoints[1].y2, "1: Wrong cut-fill height");
      Assert.AreEqual(40, calcType == VolumeCalcType.DesignToGround ? actualPoints[2].y : actualPoints[2].y2, "2: Wrong cut-fill height");
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.CUT_FILL, VolumeCalcType.None)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void CellStationsWithNoDesignElevation(string profileType, VolumeCalcType calcType)
    {
      CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ x = 0, y2 = float.NaN},
                new CompactionDataPoint{ x = 1, y2 = float.NaN},
                new CompactionDataPoint{ x = 2, y2 = float.NaN},
                new CompactionDataPoint{ x = 3, y2 = float.NaN},
              }
            }
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          results = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex {station = 0.0, elevation = 10},
            new CompactionProfileVertex {station = 0.75, elevation = float.NaN},
            new CompactionProfileVertex {station = 1.5, elevation = 40},
            new CompactionProfileVertex {station = 2.25, elevation = 10},
            new CompactionProfileVertex {station = 2.9, elevation = float.NaN},
            new CompactionProfileVertex {station = 3.5, elevation = 40}
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult, profileType, calcType);

      Assert.AreEqual(1, slicerProfileResult.results.Count, "Wrong number of profiles");
      var actualPoints = slicerProfileResult.results[0].data;
      Assert.AreEqual(4, actualPoints.Count, "Wrong number of profile points");
      Assert.AreEqual(10, calcType == VolumeCalcType.DesignToGround ? actualPoints[0].y : actualPoints[0].y2, "0: Wrong cut-fill height");
      Assert.AreEqual(float.NaN, calcType == VolumeCalcType.DesignToGround ? actualPoints[1].y : actualPoints[1].y2, "1: Wrong cut-fill height");
      Assert.AreEqual(20, calcType == VolumeCalcType.DesignToGround ? actualPoints[2].y : actualPoints[2].y2, "2: Wrong cut-fill height");
      Assert.AreEqual(float.NaN, calcType == VolumeCalcType.DesignToGround ? actualPoints[3].y : actualPoints[3].y2, "3: Wrong cut-fill height");
    }

    [TestMethod]
    public void WrongProfileTypeShouldNotChangeData()
    {
      CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.CMV_SUMMARY,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ x = 0, y2 = float.NaN},
                new CompactionDataPoint{ x = 1, y2 = float.NaN},
                new CompactionDataPoint{ x = 2, y2 = float.NaN},
              }
            }
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          results = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex {station = 0, elevation = 10},
            new CompactionProfileVertex {station = 0.5, elevation = 30},
            new CompactionProfileVertex {station = 1.0, elevation = 20},
            new CompactionProfileVertex {station = 1.5, elevation = 10},
            new CompactionProfileVertex {station = 2.0, elevation = 40},
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult, CompactionDataPoint.CMV_SUMMARY, VolumeCalcType.GroundToDesign);

      Assert.AreEqual(1, slicerProfileResult.results.Count, "Wrong number of profiles");
      var actualPoints = slicerProfileResult.results[0].data;
      Assert.AreEqual(3, actualPoints.Count, "Wrong number of profile points");
      for (int i = 0; i < 3; i++)
      {
        Assert.AreEqual(float.NaN, actualPoints[i].y2, $"{i}: Wrong cut-fill height");
      }
    }

    [TestMethod]
    public void GroundToGroundShouldNotChangeData()
    {
      CompactionProfileResult<CompactionProfileDataResult> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.SUMMARY_VOLUMES,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ x = 0, y = 10 , y2 = 11},
                new CompactionDataPoint{ x = 1, y = 15, y2 = 17},
                new CompactionDataPoint{ x = 2, y = 25, y2 = 30},
              }
            }
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          results = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex {station = 0, elevation = 10},
            new CompactionProfileVertex {station = 0.5, elevation = 30},
            new CompactionProfileVertex {station = 1.0, elevation = 20},
            new CompactionProfileVertex {station = 1.5, elevation = 10},
            new CompactionProfileVertex {station = 2.0, elevation = 40},
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult, CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround);

      Assert.AreEqual(1, slicerProfileResult.results.Count, "Wrong number of profiles");
      var actualPoints = slicerProfileResult.results[0].data;
      Assert.AreEqual(3, actualPoints.Count, "Wrong number of profile points");
      Assert.AreEqual(10, actualPoints[0].y, "0: Wrong y height");
      Assert.AreEqual(15, actualPoints[1].y, "1: Wrong y height");
      Assert.AreEqual(25, actualPoints[2].y, "2: Wrong y height");
      Assert.AreEqual(11, actualPoints[0].y2, "0: Wrong y2 height");
      Assert.AreEqual(17, actualPoints[1].y2, "1: Wrong y2 height");
      Assert.AreEqual(30, actualPoints[2].y2, "2: Wrong y2 height");
    }
    #endregion

    #region RearrangeProfileResult production data profile tests

    [TestMethod]
    public void RearrangeProductionDataProfileResultWithNull()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      Assert.ThrowsException<ServiceException>(
        () => helper.RearrangeProfileResult((CompactionProfileResult<CompactionProfileCell>) null));
    }

    [TestMethod]
    public void RearrangeProductionDataProfileResultWithNoProfile()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>();

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      Assert.ThrowsException<ServiceException>(() => helper.RearrangeProfileResult(slicerProfileResult));
    }

    [TestMethod]
    public void RearrangeProductionDataProfileResultSuccess()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileCell>
          {
            new CompactionProfileCell
            {
              cellType = ProfileCellType.MidPoint,
              station = 0,
              firstPassHeight = 0.5F,
              highestPassHeight = 0.8F,
              lastPassHeight = 0.7F,
              lowestPassHeight = 0.6F,
              lastCompositeHeight = 0.9F,
              designHeight = 0,
              cmv = 10.2F,
              cmvPercent = 89.6F,
              cmvHeight = 0.6F,
              mdpPercent = 78.3F,
              mdpHeight = 0.7F,
              temperature = 100.2F,
              temperatureHeight = 0.77F,
              topLayerPassCount = 5,
              cmvPercentChange = 4.5F,
              minSpeed = 5.9F,
              maxSpeed = 6.5F,
              cutFill = 0.06F,
              passCountIndex = ValueTargetType.NoData,
              temperatureIndex = ValueTargetType.AboveTarget,
              cmvIndex = ValueTargetType.NoData,
              mdpIndex = ValueTargetType.BelowTarget,
              speedIndex = ValueTargetType.OnTarget
            },
            new CompactionProfileCell
            {
              cellType = ProfileCellType.Edge,
              station = 1,
              firstPassHeight = 0.6F,
              highestPassHeight = 0.9F,
              lastPassHeight = 0.8F,
              lowestPassHeight = 0.7F,
              lastCompositeHeight = 0.4F,
              designHeight = 0.1F,
              cmv = 11.9F,
              cmvPercent = 90.1F,
              cmvHeight = 0.67F,
              mdpPercent = float.NaN,
              mdpHeight = float.NaN,
              temperature = 90.2F,
              temperatureHeight = 0.78F,
              topLayerPassCount = 6,
              cmvPercentChange = 3.9F,
              minSpeed = 8.1F,
              maxSpeed = 9.2F,
              cutFill = 0.15F,
              passCountIndex = ValueTargetType.AboveTarget,
              temperatureIndex = ValueTargetType.BelowTarget,
              cmvIndex = ValueTargetType.OnTarget,
              mdpIndex = ValueTargetType.NoData,
              speedIndex = ValueTargetType.NoData
            },
            new CompactionProfileCell
            {
              cellType = ProfileCellType.MidPoint,
              station = 1,
              firstPassHeight = 0.3F,
              highestPassHeight = 0.4F,
              lastPassHeight = 0.9F,
              lowestPassHeight = 0.7F,
              lastCompositeHeight = 0.99F,
              designHeight = float.NaN,
              cmv = 12.4F,
              cmvPercent = 85.1F,
              cmvHeight = float.NaN,
              mdpPercent = 45.6F,
              mdpHeight = 0.89F,
              temperature = 102.5F,
              temperatureHeight = 0.67F,
              topLayerPassCount = 8,
              cmvPercentChange = 2.1F,
              minSpeed = float.NaN,
              maxSpeed = float.NaN,
              cutFill = float.NaN,
              passCountIndex = ValueTargetType.NoData,
              temperatureIndex = ValueTargetType.BelowTarget,
              cmvIndex = ValueTargetType.AboveTarget,
              mdpIndex = ValueTargetType.OnTarget,
              speedIndex = ValueTargetType.NoData
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      var result = helper.RearrangeProfileResult(slicerProfileResult);
      Assert.IsNotNull(result);
      Assert.AreEqual(slicerProfileResult.gridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      const int expectedCount = 14;
      Assert.AreEqual(expectedCount, result.results.Count, "Wrong number of profiles");
      string[] expectedTypes =
      {
        CompactionDataPoint.FIRST_PASS, CompactionDataPoint.HIGHEST_PASS, CompactionDataPoint.LAST_PASS, CompactionDataPoint.LOWEST_PASS, CompactionDataPoint.LAST_COMPOSITE,
        CompactionDataPoint.CMV_SUMMARY, CompactionDataPoint.CMV_DETAIL, CompactionDataPoint.CMV_PERCENT_CHANGE, CompactionDataPoint.MDP_SUMMARY, CompactionDataPoint.TEMPERATURE_SUMMARY,
        CompactionDataPoint.SPEED_SUMMARY, CompactionDataPoint.PASS_COUNT_SUMMARY, CompactionDataPoint.PASS_COUNT_DETAIL, CompactionDataPoint.CUT_FILL
      };
      for (int i = 0; i < expectedCount; i++)
      {
        ValidateList(expectedTypes[i], i, slicerProfileResult.results, result.results);
      }
    }

    private void ValidateList(string expectedType, int i, List<CompactionProfileCell> expectedList,
      List<CompactionProfileDataResult> actualList)
    {
      Assert.AreEqual(expectedType, actualList[i].type, $"{i}: Wrong type");
      Assert.AreEqual(expectedList.Count, actualList[i].data.Count, $"{i}: Wrong number of points");

      ValueTargetType? expectedValueType = null;
      float? expectedY2 = null;
      float? expectedValue2 = null;

      for (int j = 0; j < expectedList.Count; j++)
      {
        float expectedHeight = float.NaN;
        float expectedValue = float.NaN;
        switch (i)
        {
          case 0: //firstPass
            expectedHeight = expectedList[j].firstPassHeight;
            expectedValue = expectedList[j].firstPassHeight;
            break;
          case 1: //highestPass
            expectedHeight = expectedList[j].highestPassHeight;
            expectedValue = expectedList[j].highestPassHeight;
            break;
          case 2: //lastPass
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].lastPassHeight;
            break;
          case 3: //lowestPass
            expectedHeight = expectedList[j].lowestPassHeight;
            expectedValue = expectedList[j].lowestPassHeight;
            break;
          case 4: //lastComposite
            expectedHeight = expectedList[j].lastCompositeHeight;
            expectedValue = expectedList[j].lastCompositeHeight;
            break;
          case 5: //cmvSummary
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmvPercent;
            expectedValueType = expectedList[j].cmvIndex;
            break;
          case 6: //cmvPercentChange
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmv;
            break;
          case 7: //cmvDetail
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmvPercentChange;
            break;
          case 8: //mdpSummary
            expectedHeight = expectedList[j].mdpHeight;
            expectedValue = expectedList[j].mdpPercent;
            expectedValueType = expectedList[j].mdpIndex;
            break;
          case 9: //temperatureSummary
            expectedHeight = expectedList[j].temperatureHeight;
            expectedValue = expectedList[j].temperature;
            expectedValueType = expectedList[j].temperatureIndex;
            break;
          case 10: //speedSummary
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].minSpeed;
            expectedValue2 = expectedList[j].maxSpeed;
            expectedValueType = expectedList[j].speedIndex;
            break;
          case 11: //passCountSummary
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].topLayerPassCount;
            expectedValueType = expectedList[j].passCountIndex;
            break;
          case 12: //passCountDetail
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].topLayerPassCount;
            break;
          case 13: //cutFill
            expectedHeight = expectedList[j].lastCompositeHeight;
            expectedValue = expectedList[j].cutFill;
            expectedY2 = float.NaN;
            break;
        }
        ValidatePoint(expectedType, j, expectedList[j], actualList[i].data[j], expectedHeight, expectedValue,
          expectedValueType, expectedY2, expectedValue2);
      }
    }

    private void ValidatePoint(string expectedType, int j, CompactionProfileCell expectedCell,
      CompactionDataPoint actualResult, float expectedY, float expectedValue, ValueTargetType? expectedValueType,
      float? expectedY2, float? expectedValue2)
    {
      Assert.AreEqual(expectedCell.cellType, actualResult.cellType, $"{j}: {expectedType} Wrong cellType");
      Assert.AreEqual(expectedCell.station, actualResult.x, $"{j}: {expectedType} Wrong x");
      Assert.AreEqual(expectedY, actualResult.y, $"{j}: {expectedType} Wrong y");
      Assert.AreEqual(expectedValue, actualResult.value, $"{j}: {expectedType} Wrong value");
      Assert.AreEqual(expectedValueType, actualResult.valueType, $"{j}: {expectedType} Wrong valueType");
      Assert.AreEqual(expectedY2, actualResult.y2, $"{j}: {expectedType} Wrong y2");
      Assert.AreEqual(expectedValue2, actualResult.value2, $"{j}: {expectedType} Wrong value2");
    }
    #endregion

    #region RearrangeProfileResult summary volumes profile tests
    [TestMethod]
    public void RearrangeSummaryVolumesProfileResultWithNull()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger); 
      var result = helper.RearrangeProfileResult((CompactionProfileResult<CompactionSummaryVolumesProfileCell>)null, VolumeCalcType.None);
      Assert.IsNull(result);
    }

    [TestMethod]
    public void RearrangeSummaryVolumesProfileResultWithJustEndPoints()
    {
      CompactionProfileResult<CompactionSummaryVolumesProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionSummaryVolumesProfileCell>
        {
          results = new List<CompactionSummaryVolumesProfileCell>
          {
            new CompactionSummaryVolumesProfileCell
            {
              cellType = ProfileCellType.Gap,
              station = 0,
              designHeight = float.NaN,
              lastPassHeight1 = float.NaN,
              lastPassHeight2 = float.NaN,
              cutFill = float.NaN
            },
            new CompactionSummaryVolumesProfileCell
            {
              cellType = ProfileCellType.Gap,
              station = 1234,
              designHeight = float.NaN,
              lastPassHeight1 = float.NaN,
              lastPassHeight2 = float.NaN,
              cutFill = float.NaN
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      var result = helper.RearrangeProfileResult(slicerProfileResult, VolumeCalcType.GroundToGround);

      ValidatePoint(CompactionDataPoint.SUMMARY_VOLUMES, 0, slicerProfileResult.results[0], result.data[0], float.NaN, float.NaN, float.NaN);
      ValidatePoint(CompactionDataPoint.SUMMARY_VOLUMES, 1, slicerProfileResult.results[1], result.data[1], float.NaN, float.NaN, float.NaN);
    }

    [TestMethod]
    [DataRow(VolumeCalcType.GroundToGround)]
    [DataRow(VolumeCalcType.GroundToDesign)]
    [DataRow(VolumeCalcType.DesignToGround)]
    public void RearrangeSummaryVolumesProfileResultSuccess(VolumeCalcType calcType)
    {
      CompactionProfileResult<CompactionSummaryVolumesProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionSummaryVolumesProfileCell>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionSummaryVolumesProfileCell>
          {
            new CompactionSummaryVolumesProfileCell
            {
              cellType = ProfileCellType.MidPoint,
              station = 0,
              lastPassHeight1 = 0.7F,
              lastPassHeight2 = 0.6F,
              designHeight = 0.8F,
              cutFill = 0.1F
            },
            new CompactionSummaryVolumesProfileCell
            {
              cellType = ProfileCellType.Edge,
              station = 1,
              lastPassHeight1 = 0.8F,
              lastPassHeight2 = 0.65F,
              designHeight = 1.2F,
              cutFill = 0.15F
            },
            new CompactionSummaryVolumesProfileCell
            {
              cellType = ProfileCellType.MidPoint,
              station = 2,
              lastPassHeight1 = 0.9F,
              lastPassHeight2 = 0.7F,
              designHeight = 0.9F,
              cutFill = 0.2F,
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      var result = helper.RearrangeProfileResult(slicerProfileResult, calcType);
      Assert.IsNotNull(result);
      ValidateList(slicerProfileResult.results, result, calcType);
    }

    private void ValidateList(List<CompactionSummaryVolumesProfileCell> expectedList,
      CompactionProfileDataResult actualResult, VolumeCalcType calcType)
    {
      Assert.AreEqual(expectedList.Count, actualResult.data.Count, "Wrong number of points");

      for (int j = 0; j < expectedList.Count; j++)
      {
        float expectedHeight = float.NaN;
        float expectedValue = expectedList[j].cutFill;
        float? expectedY2 = float.NaN;
        switch (calcType)
        {
          case VolumeCalcType.None:
            break;
          case VolumeCalcType.GroundToGround:
            expectedHeight = expectedList[j].lastPassHeight1;
            expectedY2 = expectedList[j].lastPassHeight2;
            break;
          case VolumeCalcType.GroundToDesign:
            expectedHeight = expectedList[j].lastPassHeight1;
            expectedY2 = float.NaN;
            break;
          case VolumeCalcType.DesignToGround:
            expectedHeight = float.NaN;
            expectedY2 = expectedList[j].lastPassHeight2;
            break;
        }
        ValidatePoint(CompactionDataPoint.SUMMARY_VOLUMES, j, expectedList[j], actualResult.data[j], expectedHeight, expectedValue, expectedY2);
      }
    }

    private void ValidatePoint(string expectedType, int j, CompactionSummaryVolumesProfileCell expectedCell,
      CompactionDataPoint actualResult, float expectedY, float expectedValue, float? expectedY2)
    {
      Assert.AreEqual(expectedCell.cellType, actualResult.cellType, $"{j}: {expectedType} Wrong cellType");
      Assert.AreEqual(expectedCell.station, actualResult.x, $"{j}: {expectedType} Wrong x");
      Assert.AreEqual(expectedY, actualResult.y, $"{j}: {expectedType} Wrong y");
      Assert.AreEqual(-expectedValue, actualResult.value, $"{j}: {expectedType} Wrong value");
      Assert.AreEqual(null, actualResult.valueType, $"{j}: {expectedType} Wrong valueType");
      Assert.AreEqual(expectedY2, actualResult.y2, $"{j}: {expectedType} Wrong y2");
      Assert.AreEqual(null, actualResult.value2, $"{j}: {expectedType} Wrong value2");
    }

    #endregion

    #region RemoveRepeatedNoData tests

    [TestMethod]
    [DataRow(CompactionDataPoint.FIRST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void AllGapsShouldReturnEmptyList(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = profileType,
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN,
                  y2 = useY2 ? float.NaN : (float?)null
                },
                new CompactionDataPoint
                {
                  type = profileType,
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = float.NaN,
                  y2 = useY2 ? float.NaN : (float?)null
                },
                new CompactionDataPoint
                {
                  type = profileType,
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN,
                  y2 = useY2 ? float.NaN : (float?)null
                }
              }             
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result, calcType);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(0, item.data.Count, $"{item.type}: Wrong number of data items");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.FIRST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void NoRepeatedGapsShouldNotChangeData(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      var cellTypes = new List<ProfileCellType>
      {
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Gap,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint
      };
      var stations = new List<double> { 0, 1, 2, 3, 4, 5 };
      var yValues = new List<float>
      {
        float.NaN,
        useY ? 1.2F : float.NaN,
        useY ? 1.5F : float.NaN,
        float.NaN,
        useY ? 1.3F : float.NaN,
        float.NaN
      };
      var y2Values = new List<float?>
      {
        useY2 ? float.NaN : (float?)null,
        useY2 ? 1.3F : (float?)null,
        useY2 ? 1.6F : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? 1.4F : (float?)null,
        useY2 ? float.NaN : (float?)null
      };
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>()
            }
          }
        };

      for (int i = 0; i < 6; i++)
      {
        result.results[0].data.Add(new CompactionDataPoint
        {
          type = profileType,
          cellType = cellTypes[i],
          x = stations[i],
          y = yValues[i],
          y2 = y2Values[i]
        });
      }

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result, calcType);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      var item = result.results[0];
      Assert.AreEqual(6, item.data.Count, $"{item.type}: Wrong number of data items");
      for (int i = 0; i < 6; i++)
      {
        //First and last points are gaps but also the slicer end points. 
        var expectedCellType = i == 0 || i == 5 ? ProfileCellType.Gap : cellTypes[i];
        ValidateItem(i, item.data[i], expectedCellType, stations[i], yValues[i], y2Values[i]);
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.FIRST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void NoGapsShouldNotChangeData(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      var cellTypes = new List<ProfileCellType>
      {
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint
      };
      var stations = new List<double> { 0, 1, 2, 4, 5 };
      var yValues = new List<float>
      {
        useY ? 1.1F : float.NaN,
        useY ? 1.2F : float.NaN,
        useY ? 1.5F : float.NaN,
        useY ? 1.3F : float.NaN,
        useY ? 1.0F : float.NaN,
      };
      var y2Values = new List<float?>
      {
        useY2 ? 1.0F : (float?)null,
        useY2 ? 1.3F : (float?)null,
        useY2 ? 1.6F : (float?)null,
        useY2 ? 1.2F : (float?)null,
        useY2 ? 1.4F : (float?)null,
      };
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>()
            }
          }
        };

      for (int i = 0; i < 5; i++)
      {
        result.results[0].data.Add(new CompactionDataPoint
        {
          type = profileType,
          cellType = cellTypes[i],
          x = stations[i],
          y = yValues[i],
          y2 = y2Values[i]
        });
      }


      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result, calcType);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(5, item.data.Count, $"{item.type}: Wrong number of data items");
        for (int i = 0; i < 5; i++)
        {
          ValidateItem(i, item.data[i], cellTypes[i], stations[i], yValues[i], y2Values[i]);
        }
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.FIRST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void RepeatedGapsShouldBeRemoved(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      var cellTypes = new List<ProfileCellType>
      {
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Gap,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Gap,
        ProfileCellType.MidPoint
      };
      var stations = new List<double> { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13 };
      var yValues = new List<float>
      {
        float.NaN,
        float.NaN,
        float.NaN,
        useY ? 1.3F : float.NaN,
        useY ? 1.2F : float.NaN,
        float.NaN,
        float.NaN,
        float.NaN,
        useY ? 1.8F : float.NaN,
        useY ? 1.6F : float.NaN,
        float.NaN,
        float.NaN,
        float.NaN,
        float.NaN
      };
      var y2Values = new List<float?>
      {
        useY2 ? float.NaN : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? 1.2F : (float?)null,
        useY2 ? 1.4F : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? 1.4F : (float?)null,
        useY2 ? 1.9F : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? float.NaN : (float?)null
      };
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>()
            }
          }
        };

      for (int i = 0; i < 14; i++)
      {
        result.results[0].data.Add(new CompactionDataPoint
        {
          type = profileType,
          cellType = cellTypes[i],
          x = stations[i],
          y = yValues[i],
          y2 = y2Values[i]
        });
      }

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result, calcType);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(8, item.data.Count, $"{item.type}: Wrong number of data items");
        var keptItemIndices = new int[] {0, 3, 4, 5, 8, 9, 10, 13};
        for (int i = 0; i < 8; i++)
        {
          //First and last points are gaps but also the slicer end points. Also keep start of last gap as a gap.
          var expectedCellType = i == 0 || i == 6 || i == 7 ? ProfileCellType.Gap : cellTypes[keptItemIndices[i]];
          ValidateItem(i, item.data[i], expectedCellType, stations[keptItemIndices[i]], yValues[keptItemIndices[i]], y2Values[keptItemIndices[i]]);
        }
      }
    }
 
    [TestMethod]
    public void RepeatedGapsForDifferentTypesShouldBeRemoved()
    {
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.FIRST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.FIRST_PASS,
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.FIRST_PASS,
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.FIRST_PASS,
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.FIRST_PASS,
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = 1.3F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.FIRST_PASS,
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.FIRST_PASS,
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.FIRST_PASS,
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = 1.7F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.FIRST_PASS,
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = 1.1F
                }
              }
            },
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.CMV_SUMMARY,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CMV_SUMMARY,
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CMV_SUMMARY,
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = 1.8F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CMV_SUMMARY,
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = 1.7F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CMV_SUMMARY,
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CMV_SUMMARY,
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CMV_SUMMARY,
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CMV_SUMMARY,
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = 1.1F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CMV_SUMMARY,
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = 1.0F
                }
              }
            },
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.PASS_COUNT_DETAIL,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.PASS_COUNT_DETAIL,
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = 1.4F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.PASS_COUNT_DETAIL,
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = 1.1F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.PASS_COUNT_DETAIL,
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = 1.4F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.PASS_COUNT_DETAIL,
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = 1.0F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.PASS_COUNT_DETAIL,
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = 1.5F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.PASS_COUNT_DETAIL,
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.PASS_COUNT_DETAIL,
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.PASS_COUNT_DETAIL,
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = float.NaN
                }
              }
            },
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.CUT_FILL,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CUT_FILL,
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CUT_FILL,
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CUT_FILL,
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CUT_FILL,
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = 1.3F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CUT_FILL,
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = 1.2F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CUT_FILL,
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CUT_FILL,
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = 1.7F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.CUT_FILL,
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = 1.6F
                }
              }
            },
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.SUMMARY_VOLUMES,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = ProfileCellType.MidPoint,
                  x = 0,
                  y = float.NaN,                  
                  y2 = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = ProfileCellType.Edge,
                  x = 1,
                  y = float.NaN,
                  y2 = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = ProfileCellType.MidPoint,
                  x = 2,
                  y = float.NaN,
                  y2 = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = ProfileCellType.Edge,
                  x = 3,
                  y = float.NaN,
                  y2 = 1.3F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = ProfileCellType.MidPoint,
                  x = 4,
                  y = float.NaN,
                  y2 = 1.2F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = ProfileCellType.Gap,
                  x = 5,
                  y = float.NaN,
                  y2 = float.NaN
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = ProfileCellType.Edge,
                  x = 6,
                  y = float.NaN,
                  y2 = 1.7F
                },
                new CompactionDataPoint
                {
                  type = CompactionDataPoint.SUMMARY_VOLUMES,
                  cellType = ProfileCellType.MidPoint,
                  x = 7,
                  y = float.NaN,
                  y2 = 1.6F
                }
              }
            },
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result, VolumeCalcType.DesignToGround);
      Assert.AreEqual(5, result.results.Count, "Wrong number of results");
      for (int i=0; i<5; i++)
      {
        var item = result.results[i];
        switch (i)
        {
          case 0:
            Assert.AreEqual(7, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual(CompactionDataPoint.FIRST_PASS, item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.MidPoint, 0, 1.2F, null);
            ValidateItem(1, item.data[1], ProfileCellType.Gap, 1, float.NaN, null);
            ValidateItem(2, item.data[2], ProfileCellType.Edge, 3, 1.3F, null);
            ValidateItem(3, item.data[3], ProfileCellType.MidPoint, 4, 1.2F, null);
            ValidateItem(4, item.data[4], ProfileCellType.Gap, 5, float.NaN, null);
            ValidateItem(5, item.data[5], ProfileCellType.Edge, 6, 1.7F, null);
            ValidateItem(6, item.data[6], ProfileCellType.MidPoint, 7, 1.1F, null);
            break;
          case 1:
            Assert.AreEqual(6, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual(CompactionDataPoint.CMV_SUMMARY, item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.Gap, 0, float.NaN, null);
            ValidateItem(1, item.data[1], ProfileCellType.Edge, 1, 1.8F, null);
            ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 2, 1.7F, null);
            ValidateItem(3, item.data[3], ProfileCellType.Gap, 3, float.NaN, null);
            ValidateItem(4, item.data[4], ProfileCellType.Edge, 6, 1.1F, null);
            ValidateItem(5, item.data[5], ProfileCellType.MidPoint, 7, 1.0F, null);
            break;
          case 2:
            Assert.AreEqual(7, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual(CompactionDataPoint.PASS_COUNT_DETAIL, item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.MidPoint, 0, 1.4F, null);
            ValidateItem(1, item.data[1], ProfileCellType.Edge, 1, 1.1F, null);
            ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 2, 1.4F, null);
            ValidateItem(3, item.data[3], ProfileCellType.Edge, 3, 1.0F, null);
            ValidateItem(4, item.data[4], ProfileCellType.MidPoint, 4, 1.5F, null);
            ValidateItem(5, item.data[5], ProfileCellType.Gap, 5, float.NaN, null);
            ValidateItem(6, item.data[6], ProfileCellType.Gap, 7, float.NaN, null);
            break;
          case 3:
            Assert.AreEqual(6, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual(CompactionDataPoint.CUT_FILL, item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.Gap, 0, float.NaN, null);
            ValidateItem(1, item.data[1], ProfileCellType.Edge, 3, 1.3F, null);
            ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 4, 1.2F, null);
            ValidateItem(3, item.data[3], ProfileCellType.Gap, 5, float.NaN, null);
            ValidateItem(4, item.data[4], ProfileCellType.Edge, 6, 1.7F, null);
            ValidateItem(5, item.data[5], ProfileCellType.MidPoint, 7, 1.6F, null);
            break;
          case 4:
            Assert.AreEqual(6, item.data.Count, $"{i}: Wrong number of data items");
            Assert.AreEqual(CompactionDataPoint.SUMMARY_VOLUMES, item.type, $"{i}: Wrong type");
            ValidateItem(0, item.data[0], ProfileCellType.Gap, 0, float.NaN, float.NaN);
            ValidateItem(1, item.data[1], ProfileCellType.Edge, 3, float.NaN, 1.3F);
            ValidateItem(2, item.data[2], ProfileCellType.MidPoint, 4, float.NaN, 1.2F);
            ValidateItem(3, item.data[3], ProfileCellType.Gap, 5, float.NaN, float.NaN);
            ValidateItem(4, item.data[4], ProfileCellType.Edge, 6, float.NaN, 1.7F);
            ValidateItem(5, item.data[5], ProfileCellType.MidPoint, 7, float.NaN, 1.6F);
            break;
        }
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.FIRST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void RepeatedGapsWithNoDataShouldBeRemoved(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      var cellTypes = new List<ProfileCellType>
      {
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint,
        ProfileCellType.Edge,
        ProfileCellType.MidPoint
      };
      var stations = new List<double> { 0, 1, 2, 3, 4, 5, 6 };
      var yValues = new List<float>
      {
        useY ? 2.0F : float.NaN,
        useY ? 1.8F : float.NaN,
        float.NaN,
        useY ? 1.3F : float.NaN,
        float.NaN,
        useY ? 1.7F : float.NaN,
        useY ? 1.2F : float.NaN
      };
      var y2Values = new List<float?>
      {
        useY2 ? 1.7F : (float?)null,
        useY2 ? 1.4F : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? 1.1F : (float?)null,
        useY2 ? float.NaN : (float?)null,
        useY2 ? 1.3F : (float?)null,
        useY2 ? 1.9F : (float?)null
      };
      var values = new List<float>
      {
        2.0F,
        float.NaN,
        float.NaN,
        float.NaN,
        float.NaN,
        1.7F,
        1.2F
      };
      CompactionProfileResult<CompactionProfileDataResult> result =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>()
            }
          }
        };

      for (int i = 0; i < 7; i++)
      {
        result.results[0].data.Add(new CompactionDataPoint
        {
          type = profileType,
          cellType = cellTypes[i],
          x = stations[i],
          y = yValues[i],
          y2 = y2Values[i],
          value = values[i]
        });
      }

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.RemoveRepeatedNoData(result, calcType);
      Assert.AreEqual(1, result.results.Count, "Wrong number of results");
      foreach (var item in result.results)
      {
        Assert.AreEqual(4, item.data.Count, $"{item.type}: Wrong number of data items");
        var keptItemIndices = new int[] { 0, 1, 5, 6 };
        for (int i = 0; i < 4; i++)
        {
          //Index 1 is a gap due to no data
          var expectedCellType = i == 1 ? ProfileCellType.Gap : cellTypes[keptItemIndices[i]];
          ValidateItem(i, item.data[i], expectedCellType, stations[keptItemIndices[i]], yValues[keptItemIndices[i]], y2Values[keptItemIndices[i]]);
        }
      }
    }

    private void ValidateItem(int i, CompactionDataPoint actual, ProfileCellType expectedCellType, double expectedStation, float expectedElevation, float? expectedElevation2)
    {
      Assert.AreEqual(expectedCellType, actual.cellType, $"{i}: Wrong cellType");
      Assert.AreEqual(expectedStation, actual.x, $"{i}: Wrong x");
      Assert.AreEqual(expectedElevation, actual.y, $"{i}: Wrong y");
      Assert.AreEqual(expectedElevation2, actual.y2, $"{i}: Wrong y2");
    }
    #endregion

    #region AddMidPoint tests

    [TestMethod]
    public void SlicerEmptyDataNoAddedMidPoints()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>()
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(0, actualPoints.Count, "Wrong number of profile points");
    }

    [TestMethod]
    public void SlicerInOneCellNoAddedMidPoints()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = 597.367F},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0.085, y = 597.367F}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(2, actualPoints.Count, "Wrong number of profile points");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[1].cellType, "Wrong cellType 2");
    }

    [TestMethod]
    public void SlicesOneEdgeNoAddedMidPoints()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = 596.3F},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 0.05, y = 596.7F},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0.12, y = 596.7F}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(3, actualPoints.Count, "Wrong number of profile points");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[2].cellType, "Wrong cellType 3");
    }

    [TestMethod]
    public void SlicesTwoEdgesOneAddedMidPoint()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = 100F},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 0.5, y = 250F},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = 190F},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.5, y = 190F}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(5, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(250F, actualPoints[1].y, "Wrong y 2");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(0.75, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(250F, actualPoints[2].y, "Wrong y 3");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(1.0, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(190F, actualPoints[3].y, "Wrong y 4");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(1.5, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(190F, actualPoints[4].y, "Wrong y 5");
    }

    [TestMethod]
    public void SlicerNoGapsMidPointsBetweenAllEdges()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = 100F},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 0.5, y = 250F},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = 190F},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.5, y = 235F},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 2.0, y = 235F}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(7, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(250F, actualPoints[1].y, "Wrong y 2");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(0.75, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(250F, actualPoints[2].y, "Wrong y 3");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(1.0, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(190F, actualPoints[3].y, "Wrong y 4");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(1.25, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(190F, actualPoints[4].y, "Wrong y 5");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[5].cellType, "Wrong cellType 6");
      Assert.AreEqual(1.5, actualPoints[5].x, "Wrong x 6");
      Assert.AreEqual(235F, actualPoints[5].y, "Wrong y 6");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[6].cellType, "Wrong cellType 7");
      Assert.AreEqual(2.0, actualPoints[6].x, "Wrong x 7");
      Assert.AreEqual(235F, actualPoints[6].y, "Wrong y 7");
    }

    [TestMethod]
    public void SlicerWithOneGapNoMidPointInGap()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = 100F},
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0.5, y = float.NaN},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = 190F},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.5, y = 235F},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 2.0, y = 235F}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(6, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(float.NaN, actualPoints[1].y, "Wrong y 2");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(1.0, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(190F, actualPoints[2].y, "Wrong y 3");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(1.25, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(190F, actualPoints[3].y, "Wrong y 4");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(1.5, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(235F, actualPoints[4].y, "Wrong y 5");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[5].cellType, "Wrong cellType 6");
      Assert.AreEqual(2.0, actualPoints[5].x, "Wrong x 6");
      Assert.AreEqual(235F, actualPoints[5].y, "Wrong y 6");
    }

    [TestMethod]
    public void SlicerWithOnlyAGapNoAddedMidPoints()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = 100F},
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0.5, y = float.NaN},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = 190F},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.5, y = 190F}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(4, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(float.NaN, actualPoints[1].y, "Wrong y 2");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(1.0, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(190F, actualPoints[2].y, "Wrong y 3");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(1.5, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(190F, actualPoints[3].y, "Wrong y 4");
    }

    [TestMethod]
    public void SlicerWithTwoGapsNoMidPointInGaps()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = 100F},
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0.5, y = float.NaN},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = 190F},
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 1.5, y = float.NaN},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 2.0, y = 235F},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 2.5, y = 235F}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(7, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(float.NaN, actualPoints[1].y, "Wrong y 2");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(1.0, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(190F, actualPoints[2].y, "Wrong y 3");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(1.25, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(190F, actualPoints[3].y, "Wrong y 4");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(1.5, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(float.NaN, actualPoints[4].y, "Wrong y 5");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[5].cellType, "Wrong cellType 6");
      Assert.AreEqual(2.0, actualPoints[5].x, "Wrong x 6");
      Assert.AreEqual(235F, actualPoints[5].y, "Wrong y 6");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[6].cellType, "Wrong cellType 7");
      Assert.AreEqual(2.5, actualPoints[6].x, "Wrong x 7");
      Assert.AreEqual(235F, actualPoints[6].y, "Wrong y 7");
    }

    [TestMethod]
    public void SlicerStartAndEndInGapAddMidPointsCorrectly()
    {
      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = CompactionDataPoint.LAST_PASS,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0, y = float.NaN},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 0.5, y = 100F},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = 200F},
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 1.5, y = float.NaN},
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 2.0, y = float.NaN}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.AddMidPoints(profileResult);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(7, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(float.NaN, actualPoints[0].y, "Wrong y 1");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(100F, actualPoints[1].y, "Wrong y 2");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(0.75, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(100F, actualPoints[2].y, "Wrong y 3");

      Assert.AreEqual(ProfileCellType.Edge, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(1.0, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(200F, actualPoints[3].y, "Wrong y 4");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(1.25, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(200F, actualPoints[4].y, "Wrong y 5");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[5].cellType, "Wrong cellType 6");
      Assert.AreEqual(1.5, actualPoints[5].x, "Wrong x 6");
      Assert.AreEqual(float.NaN, actualPoints[5].y, "Wrong y 6");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[6].cellType, "Wrong cellType 7");
      Assert.AreEqual(2.0, actualPoints[6].x, "Wrong x 7");
      Assert.AreEqual(float.NaN, actualPoints[6].y, "Wrong y 7");
    }

    #endregion

    #region InterpolateEdges tests
    [TestMethod]
    [DataRow(CompactionDataPoint.LAST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void SlicerInOneCellNoInterpolation(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = useY ? 597F : float.NaN, y2 = useY2 ? 603F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0.085, y = useY ? 597F : float.NaN, y2 = useY2 ? 603F : (float?)null }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.InterpolateEdges(profileResult, calcType);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(2, actualPoints.Count, "Wrong number of profile points");
      if (useY)
      {
        Assert.AreEqual(597F, actualPoints[0].y, "Wrong y 1");
        Assert.AreEqual(597F, actualPoints[1].y, "Wrong y 2");
      }
      if (useY2)
      {
        Assert.AreEqual(603F, actualPoints[0].y2, "Wrong y2 1");
        Assert.AreEqual(603F, actualPoints[1].y2, "Wrong y2 2");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.LAST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void SlicesOneEdgeInterpolated(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = useY ? 100F : float.NaN, y2 = useY2 ? 200F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 0.5, y = useY ? 200F : float.NaN, y2 = useY2 ? 300F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.0, y = useY ? 200F : float.NaN, y2 = useY2 ? 300F : (float?)null }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.InterpolateEdges(profileResult, calcType);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(3, actualPoints.Count, "Wrong number of profile points");
      if (useY)
      {
        Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");
        Assert.AreEqual(150F, actualPoints[1].y, "Wrong y 2");
        Assert.AreEqual(200F, actualPoints[2].y, "Wrong y 3");
      }
      if (useY2)
      {
        Assert.AreEqual(200F, actualPoints[0].y2, "Wrong y2 1");
        Assert.AreEqual(250F, actualPoints[1].y2, "Wrong y2 2");
        Assert.AreEqual(300F, actualPoints[2].y2, "Wrong y2 3");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.LAST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void SlicesTwoEdgesInterpolated(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = useY ? 100F : float.NaN, y2 = useY2 ? 200F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 0.5, y = useY ? 250F : float.NaN, y2 = useY2 ? 350F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0.75, y = useY ? 250F : float.NaN, y2 = useY2 ? 350F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = useY ? 325F : float.NaN, y2 = useY2 ? 425F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.5, y = useY ? 325F : float.NaN, y2 = useY2 ? 425F : (float?)null }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.InterpolateEdges(profileResult, calcType);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(5, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[4].cellType, "Wrong cellType 5");

      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(0.75, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(1.0, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(1.5, actualPoints[4].x, "Wrong x 5");

      if (useY)
      {
        Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");
        Assert.AreEqual(200F, actualPoints[1].y, "Wrong y 2");
        Assert.AreEqual(250F, actualPoints[2].y, "Wrong y 3");
        Assert.AreEqual(275F, actualPoints[3].y, "Wrong y 4");
        Assert.AreEqual(325F, actualPoints[4].y, "Wrong y 5");
      }
      if (useY2)
      {
        Assert.AreEqual(200F, actualPoints[0].y2, "Wrong y2 1");
        Assert.AreEqual(300F, actualPoints[1].y2, "Wrong y2 2");
        Assert.AreEqual(350F, actualPoints[2].y2, "Wrong y2 3");
        Assert.AreEqual(375F, actualPoints[3].y2, "Wrong y2 4");
        Assert.AreEqual(425F, actualPoints[4].y2, "Wrong y2 5");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.LAST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void SlicerWithOneGapInterpolated(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = useY ? 100F : float.NaN, y2 = useY2 ? 200F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0.5, y = float.NaN, y2 = useY2 ? float.NaN : (float?)null },//this is a gap
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = useY ? 190F : float.NaN, y2 = useY2 ? 290F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.25, y = useY ? 190F : float.NaN, y2 = useY2 ? 290F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.5, y = useY ? 235F : float.NaN, y2 = useY2 ? 335F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 2.0, y = useY ? 235F : float.NaN, y2 = useY2 ? 335F : (float?)null }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.InterpolateEdges(profileResult, calcType);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(6, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(ProfileCellType.Gap, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[5].cellType, "Wrong cellType 6");

      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(1.0, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(1.25, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(1.5, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(2.0, actualPoints[5].x, "Wrong x 6");

      if (useY)
      {
        Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");
        Assert.AreEqual(136F, actualPoints[1].y, "Wrong y 2");
        Assert.AreEqual(172F, actualPoints[2].y, "Wrong y 3");
        Assert.AreEqual(190F, actualPoints[3].y, "Wrong y 4");
        Assert.AreEqual(205F, actualPoints[4].y, "Wrong y 5");
        Assert.AreEqual(235F, actualPoints[5].y, "Wrong y 6");
      }

      if (useY2)
      {
        Assert.AreEqual(200F, actualPoints[0].y2, "Wrong y2 1");
        Assert.AreEqual(236F, actualPoints[1].y2, "Wrong y2 2");
        Assert.AreEqual(272F, actualPoints[2].y2, "Wrong y2 3");
        Assert.AreEqual(290F, actualPoints[3].y2, "Wrong y2 4");
        Assert.AreEqual(305F, actualPoints[4].y2, "Wrong y2 5");
        Assert.AreEqual(335F, actualPoints[5].y2, "Wrong y2 6");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.LAST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void SlicerWithOnlyAGapInterpolated(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = useY ? 100F : float.NaN, y2 = useY2 ? 200F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0.5, y = float.NaN, y2 = useY2 ? float.NaN : (float?)null },//this is a gap
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = useY ? 190F : float.NaN, y2 = useY2 ? 290F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.5, y = useY ? 190F : float.NaN, y2 = useY2 ? 290F : (float?)null}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.InterpolateEdges(profileResult, calcType);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(4, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(ProfileCellType.Gap, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[3].cellType, "Wrong cellType 4");

      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(1.0, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(1.5, actualPoints[3].x, "Wrong x 4");

      if (useY)
      {
        Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");
        Assert.AreEqual(130F, actualPoints[1].y, "Wrong y 2");
        Assert.AreEqual(160F, actualPoints[2].y, "Wrong y 3");
        Assert.AreEqual(190F, actualPoints[3].y, "Wrong y 4");
      }
      if (useY2)
      {
        Assert.AreEqual(200F, actualPoints[0].y2, "Wrong y2 1");
        Assert.AreEqual(230F, actualPoints[1].y2, "Wrong y2 2");
        Assert.AreEqual(260F, actualPoints[2].y2, "Wrong y2 3");
        Assert.AreEqual(290F, actualPoints[3].y2, "Wrong y2 4");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.LAST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void SlicerWithTwoGapsInterpolated(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0, y = useY ? 100F : float.NaN, y2 = useY2 ? 200F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0.5, y = float.NaN, y2 = useY2 ? float.NaN : (float?)null },//this is a gap
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = useY ? 190F : float.NaN, y2 = useY2 ? 290F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.25, y = useY ? 190F : float.NaN, y2 = useY2 ? 290F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 1.5, y = float.NaN, y2 = useY2 ? float.NaN : (float?)null },//this is a gap
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 2.0, y = useY ?  235F : float.NaN, y2 = useY2 ? 335F : (float?)null },
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 2.5, y = useY ? 235F : float.NaN, y2 = useY2 ? 335F : (float?)null }
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.InterpolateEdges(profileResult, calcType);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(7, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(ProfileCellType.Gap, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(ProfileCellType.Gap, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[5].cellType, "Wrong cellType 6");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[6].cellType, "Wrong cellType 7");

      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(1.0, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(1.25, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(1.5, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(2.0, actualPoints[5].x, "Wrong x 6");
      Assert.AreEqual(2.5, actualPoints[6].x, "Wrong x 7");

      if (useY)
      {
        Assert.AreEqual(100F, actualPoints[0].y, "Wrong y 1");
        Assert.AreEqual(136F, actualPoints[1].y, "Wrong y 2");
        Assert.AreEqual(172F, actualPoints[2].y, "Wrong y 3");
        Assert.AreEqual(190F, actualPoints[3].y, "Wrong y 4");
        Assert.AreEqual(199F, actualPoints[4].y, "Wrong y 5");
        Assert.AreEqual(217F, actualPoints[5].y, "Wrong y 6");
        Assert.AreEqual(235F, actualPoints[6].y, "Wrong y 7");
      }
      if (useY2)
      {
        Assert.AreEqual(200F, actualPoints[0].y2, "Wrong y2 1");
        Assert.AreEqual(236F, actualPoints[1].y2, "Wrong y2 2");
        Assert.AreEqual(272F, actualPoints[2].y2, "Wrong y2 3");
        Assert.AreEqual(290F, actualPoints[3].y2, "Wrong y2 4");
        Assert.AreEqual(299F, actualPoints[4].y2, "Wrong y2 5");
        Assert.AreEqual(317F, actualPoints[5].y2, "Wrong y2 6");
        Assert.AreEqual(335F, actualPoints[6].y2, "Wrong y2 7");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.LAST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void SlicerStartInGapExtrapolated(string profileType, VolumeCalcType? calcType)
    {
      //Tests the extrapolation special case
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0, y = float.NaN, y2 = useY2 ? float.NaN : (float?)null },//this is a gap
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 0.5, y = useY ? 100F : float.NaN, y2 = useY2 ? 200F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0.75, y = useY ? 100F: float.NaN, y2 = useY2 ? 200F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = useY? 200F : float.NaN, y2 = useY2? 300F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.25, y = useY ? 200F : float.NaN, y2 = useY2 ? 300F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.5, y = useY ? 350F : float.NaN, y2 = useY2 ? 450F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 2.0, y = useY ? 350F : float.NaN, y2 = useY2 ? 450F : (float?)null}
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.InterpolateEdges(profileResult, calcType);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(7, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[5].cellType, "Wrong cellType 6");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[6].cellType, "Wrong cellType 7");

      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(0.75, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(1.0, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(1.25, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(1.5, actualPoints[5].x, "Wrong x 6");
      Assert.AreEqual(2.0, actualPoints[6].x, "Wrong x 7");

      if (useY)
      {
        Assert.AreEqual(float.NaN, actualPoints[0].y, "Wrong y 1");
        Assert.AreEqual(50F, actualPoints[1].y, "Wrong y 2");
        Assert.AreEqual(100F, actualPoints[2].y, "Wrong y 3");
        Assert.AreEqual(150F, actualPoints[3].y, "Wrong y 4");
        Assert.AreEqual(200F, actualPoints[4].y, "Wrong y 5");
        Assert.AreEqual(250F, actualPoints[5].y, "Wrong y 6");
        Assert.AreEqual(350F, actualPoints[6].y, "Wrong y 7");
      }
      if (useY2)
      {
        Assert.AreEqual(float.NaN, actualPoints[0].y2, "Wrong y2 1");
        Assert.AreEqual(150F, actualPoints[1].y2, "Wrong y2 2");
        Assert.AreEqual(200F, actualPoints[2].y2, "Wrong y2 3");
        Assert.AreEqual(250F, actualPoints[3].y2, "Wrong y2 4");
        Assert.AreEqual(300F, actualPoints[4].y2, "Wrong y2 5");
        Assert.AreEqual(350F, actualPoints[5].y2, "Wrong y2 6");
        Assert.AreEqual(450F, actualPoints[6].y2, "Wrong y2 7");
      }
    }

    [TestMethod]
    [DataRow(CompactionDataPoint.LAST_PASS, null)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToGround)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.GroundToDesign)]
    [DataRow(CompactionDataPoint.SUMMARY_VOLUMES, VolumeCalcType.DesignToGround)]
    public void SlicerStartAndEndInGapInterpolatedCorrectly(string profileType, VolumeCalcType? calcType)
    {
      bool useY = profileType != CompactionDataPoint.SUMMARY_VOLUMES || calcType != VolumeCalcType.DesignToGround;
      bool useY2 = profileType == CompactionDataPoint.SUMMARY_VOLUMES && calcType != VolumeCalcType.GroundToDesign;

      CompactionProfileResult<CompactionProfileDataResult> profileResult =
        new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>
          {
            new CompactionProfileDataResult
            {
              type = profileType,
              data = new List<CompactionDataPoint>
              {
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 0, y = float.NaN, y2 = useY2 ? float.NaN : (float?)null },//this is a gap
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 0.5, y = useY ? 100F : float.NaN, y2 = useY2 ? 200F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 0.75, y = useY ? 100F : float.NaN, y2 = useY2 ? 200F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.Edge, x = 1.0, y = useY ? 200F : float.NaN, y2 = useY2 ? 300F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.MidPoint, x = 1.25, y = useY ? 200F : float.NaN, y2 = useY2 ? 300F : (float?)null},
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 1.5, y = float.NaN, y2 = useY2 ? float.NaN : (float?)null },//this is a gap
                new CompactionDataPoint{ cellType = ProfileCellType.Gap, x = 2.0, y = float.NaN, y2 = useY2 ? float.NaN : (float?)null },//this is a gap
              }
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);
      helper.InterpolateEdges(profileResult, calcType);

      Assert.AreEqual(1, profileResult.results.Count, "Wrong number of profiles");
      var actualPoints = profileResult.results[0].data;
      Assert.AreEqual(7, actualPoints.Count, "Wrong number of profile points");

      Assert.AreEqual(ProfileCellType.Gap, actualPoints[0].cellType, "Wrong cellType 1");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[1].cellType, "Wrong cellType 2");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[2].cellType, "Wrong cellType 3");
      Assert.AreEqual(ProfileCellType.Edge, actualPoints[3].cellType, "Wrong cellType 4");
      Assert.AreEqual(ProfileCellType.MidPoint, actualPoints[4].cellType, "Wrong cellType 5");
      Assert.AreEqual(ProfileCellType.Gap, actualPoints[5].cellType, "Wrong cellType 6");
      Assert.AreEqual(ProfileCellType.Gap, actualPoints[6].cellType, "Wrong cellType 7");

      Assert.AreEqual(0, actualPoints[0].x, "Wrong x 1");
      Assert.AreEqual(0.5, actualPoints[1].x, "Wrong x 2");
      Assert.AreEqual(0.75, actualPoints[2].x, "Wrong x 3");
      Assert.AreEqual(1.0, actualPoints[3].x, "Wrong x 4");
      Assert.AreEqual(1.25, actualPoints[4].x, "Wrong x 5");
      Assert.AreEqual(1.5, actualPoints[5].x, "Wrong x 6");
      Assert.AreEqual(2.0, actualPoints[6].x, "Wrong x 7");

      if (useY)
      {
        Assert.AreEqual(float.NaN, actualPoints[0].y, "Wrong y 1");
        Assert.AreEqual(50F, actualPoints[1].y, "Wrong y 2");
        Assert.AreEqual(100F, actualPoints[2].y, "Wrong y 3");
        Assert.AreEqual(150F, actualPoints[3].y, "Wrong y 4");
        Assert.AreEqual(200F, actualPoints[4].y, "Wrong y 5");
        Assert.AreEqual(250F, actualPoints[5].y, "Wrong y 6");
        Assert.AreEqual(float.NaN, actualPoints[6].y, "Wrong y 7");
      }
      if (useY2)
      {
        Assert.AreEqual(float.NaN, actualPoints[0].y2, "Wrong y2 1");
        Assert.AreEqual(150F, actualPoints[1].y2, "Wrong y2 2");
        Assert.AreEqual(200F, actualPoints[2].y2, "Wrong y2 3");
        Assert.AreEqual(250F, actualPoints[3].y2, "Wrong y2 4");
        Assert.AreEqual(300F, actualPoints[4].y2, "Wrong y2 5");
        Assert.AreEqual(350F, actualPoints[5].y2, "Wrong y2 6");
        Assert.AreEqual(float.NaN, actualPoints[6].y2, "Wrong y2 7");
      }
    }
    #endregion

    #region ConvertProfileResult design profile tests
    [TestMethod]
    public void ConvertDesignProfileResultWithNull()
    {
      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      Assert.ThrowsException<ServiceException>(
        () => helper.ConvertProfileResult((Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>)null));
    }

    [TestMethod]
    public void ConvertDesignProfileResultWithNoProfile()
    {
      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults =
        new Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>();

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      Assert.ThrowsException<ServiceException>(() => helper.ConvertProfileResult(slicerProfileResults));
    }

    [TestMethod]
    public void ConvertDesignProfileResultSuccess()
    {
      Guid designUid1 = Guid.NewGuid();
      Guid designUid2 = Guid.NewGuid();

      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults =
        new Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>();
      slicerProfileResults.Add(designUid1, new CompactionProfileResult<CompactionProfileVertex>
      {
        gridDistanceBetweenProfilePoints = 12.34,
        results = new List<CompactionProfileVertex>
        {
          new CompactionProfileVertex{station = 1.2, elevation = 0.9F},
          new CompactionProfileVertex{station = 1.7, elevation = 1.3F},
          new CompactionProfileVertex{station = 2.8, elevation = 2.1F},
          new CompactionProfileVertex{station = 2.9, elevation = float.NaN},
        }
      });
      slicerProfileResults.Add(designUid2, new CompactionProfileResult<CompactionProfileVertex>
      {
        gridDistanceBetweenProfilePoints = 12.34,
        results = new List<CompactionProfileVertex>
        {
          new CompactionProfileVertex{station = 0.8, elevation = 2.1F},
          new CompactionProfileVertex{station = 1.1, elevation = 1.3F},
          new CompactionProfileVertex{station = 2.7, elevation = float.NaN},
          new CompactionProfileVertex{station = 3.8, elevation = 3.4F},
          new CompactionProfileVertex{station = 4.2, elevation = 2.3F},
        }
      });

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      var result = helper.ConvertProfileResult(slicerProfileResults);
      Assert.IsNotNull(result);
      Assert.AreEqual(slicerProfileResults.Values.First().gridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(slicerProfileResults.Keys.Count, result.results.Count, "Wrong number of profiles");
      int i = 0;
      foreach (var item in slicerProfileResults)
      {
        ValidateDesignProfile(item.Key, i, item.Value.results, result.results[i]);
        i++;
      }
    }

    [TestMethod]
    public void ConvertDesignProfileResultWithEmptyProfile()
    {
      Guid designUid1 = Guid.NewGuid();
      Guid designUid2 = Guid.NewGuid();

      Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>> slicerProfileResults =
        new Dictionary<Guid, CompactionProfileResult<CompactionProfileVertex>>();
      slicerProfileResults.Add(designUid1, new CompactionProfileResult<CompactionProfileVertex>
      {
        gridDistanceBetweenProfilePoints = 0,
        results = new List<CompactionProfileVertex>()
      });
      slicerProfileResults.Add(designUid2, new CompactionProfileResult<CompactionProfileVertex>
      {
        gridDistanceBetweenProfilePoints = 12.34,
        results = new List<CompactionProfileVertex>
        {
          new CompactionProfileVertex{station = 0.8, elevation = 2.1F},
          new CompactionProfileVertex{station = 1.1, elevation = 1.3F},
          new CompactionProfileVertex{station = 2.7, elevation = float.NaN},
          new CompactionProfileVertex{station = 3.8, elevation = 3.4F},
          new CompactionProfileVertex{station = 4.2, elevation = 2.3F},
        }
      });

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      var result = helper.ConvertProfileResult(slicerProfileResults);
      Assert.IsNotNull(result);
      Assert.AreEqual(12.34, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(slicerProfileResults.Keys.Count, result.results.Count, "Wrong number of profiles");
      int i = 0;
      foreach (var item in slicerProfileResults)
      {
        ValidateDesignProfile(item.Key, i, item.Value.results, result.results[i]);
        i++;
      }
    }

    private void ValidateDesignProfile(Guid expectedDesignUid, int j, List<CompactionProfileVertex> expectedVertices, CompactionDesignProfileResult actualResult)
    {
      Assert.AreEqual(expectedDesignUid, actualResult.designFileUid, $"{j}: Wrong designUid");
      Assert.IsNotNull(actualResult.data, $"{j}: Should have some data returned");
      Assert.AreEqual(expectedVertices.Count, actualResult.data.Count, $"{j}: Wrong vertex count");
      for (int i = 0; i < expectedVertices.Count; i++)
      {
        Assert.AreEqual(expectedVertices[i].station, actualResult.data[i].station, $"{j}: Wrong station {i}");
        Assert.AreEqual(expectedVertices[i].elevation, actualResult.data[i].elevation, $"{j}: Wrong elevation {i}");
      }
    }
    #endregion

    #region AddSlicerEndPoints tests

    [TestMethod]
    public void DesignProfileWithSlicerEndPointsPresentShouldNotChange()
    {
      Guid designUid = Guid.NewGuid();
      var distance = 7.3;
      var v1 = new CompactionProfileVertex {station = 0.0, elevation = 2.1F};
      var v2 = new CompactionProfileVertex { station = 1.1, elevation = 1.3F };
      var v3 = new CompactionProfileVertex {station = 2.7, elevation = float.NaN};
      var v4 = new CompactionProfileVertex {station = 3.8, elevation = 3.4F};
      var v5 = new CompactionProfileVertex {station = distance, elevation = 2.3F};
      var expectedVertices = new List<CompactionProfileVertex>
      {
        v1,
        v2,
        v3,
        v4,
        v5
      };

      CompactionProfileResult<CompactionDesignProfileResult> result =
        new CompactionProfileResult<CompactionDesignProfileResult>
        {
          gridDistanceBetweenProfilePoints = distance,
          results = new List<CompactionDesignProfileResult>
          {
            new CompactionDesignProfileResult
            {
              designFileUid = designUid,
              data = new List<CompactionProfileVertex>(expectedVertices)
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      helper.AddSlicerEndPoints(result);
      Assert.AreEqual(distance, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(1, result.results.Count);
      Assert.AreEqual(designUid, result.results[0].designFileUid);
      Assert.AreEqual(expectedVertices.Count, result.results[0].data.Count);
      for (int i = 0; i < expectedVertices.Count; i++)
      {
        Assert.AreEqual(expectedVertices[i].station, result.results[0].data[i].station);
        Assert.AreEqual(expectedVertices[i].elevation, result.results[0].data[i].elevation);
      }
    }

    [TestMethod]
    public void DesignProfileWithoutSlicerEndPointsPresentShouldAddThem()
    {
      Guid designUid = Guid.NewGuid();
      var distance = 7.3;
      var v1 = new CompactionProfileVertex { station = 0.5, elevation = 2.1F };
      var v2 = new CompactionProfileVertex { station = 1.1, elevation = 1.3F };
      var v3 = new CompactionProfileVertex { station = 2.7, elevation = float.NaN };
      var v4 = new CompactionProfileVertex { station = 3.8, elevation = 3.4F };
      var v5 = new CompactionProfileVertex { station = 5.1, elevation = 2.3F };
      var expectedVertices = new List<CompactionProfileVertex>
      {
        v1,
        v2,
        v3,
        v4,
        v5
      };

      CompactionProfileResult<CompactionDesignProfileResult> result =
        new CompactionProfileResult<CompactionDesignProfileResult>
        {
          gridDistanceBetweenProfilePoints = distance,
          results = new List<CompactionDesignProfileResult>
          {
            new CompactionDesignProfileResult
            {
              designFileUid = designUid,
              data = new List<CompactionProfileVertex>(expectedVertices)
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      helper.AddSlicerEndPoints(result);
      Assert.AreEqual(distance, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(1, result.results.Count);
      Assert.AreEqual(designUid, result.results[0].designFileUid);
      Assert.AreEqual(expectedVertices.Count+2, result.results[0].data.Count);
      Assert.AreEqual(0, result.results[0].data[0].station);
      Assert.AreEqual(float.NaN, result.results[0].data[0].elevation);
      for (int i = 1; i < 6; i++)
      {
        Assert.AreEqual(expectedVertices[i-1].station, result.results[0].data[i].station);
        Assert.AreEqual(expectedVertices[i-1].elevation, result.results[0].data[i].elevation);
      }
      Assert.AreEqual(distance, result.results[0].data[6].station);
      Assert.AreEqual(float.NaN, result.results[0].data[6].elevation);
    }

    [TestMethod]
    public void DesignProfileWithNoPointsShouldNotChange()
    {
      Guid designUid = Guid.NewGuid();
      var distance = 0;
 
      CompactionProfileResult<CompactionDesignProfileResult> result =
        new CompactionProfileResult<CompactionDesignProfileResult>
        {
          gridDistanceBetweenProfilePoints = distance,
          results = new List<CompactionDesignProfileResult>
          {
            new CompactionDesignProfileResult
            {
              designFileUid = designUid,
              data = new List<CompactionProfileVertex>()
            }
          }
        };

      var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper(logger);

      helper.AddSlicerEndPoints(result);
      Assert.AreEqual(distance, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(1, result.results.Count);
      Assert.AreEqual(designUid, result.results[0].designFileUid);
      Assert.AreEqual(0, result.results[0].data.Count);
    }
    #endregion
  }
}
