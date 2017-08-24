using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VSS.Common.Exceptions;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;

namespace VSS.Productivity3D.WebApiTests.Compaction.Helpers
{
  [TestClass]
  public class CompactionProfileResultHelperTests
  {

    [TestMethod]
    public void NoProdDataAndNoDesignProfile()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult = new CompactionProfileResult<CompactionProfileCell>();
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult = new CompactionProfileResult<CompactionProfileVertex>();

      CompactionProfileResultHelper helper = new CompactionProfileResultHelper();
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.IsNull(slicerProfileResult.points, "Slicer profile should be null");
    }

    [TestMethod]
    public void NoDesignProfileShouldNotChangeProdData()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          points = new List<CompactionProfileCell>
          {
            new CompactionProfileCell{station = 0, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 1, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 2, cutFillHeight = float.NaN},
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult = new CompactionProfileResult<CompactionProfileVertex>();

      CompactionProfileResultHelper helper = new CompactionProfileResultHelper();
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.AreEqual(3, slicerProfileResult.points.Count, "Wrong number of profile points");
      for (int i = 0; i < 3; i++)
      {
        Assert.IsTrue(float.IsNaN(slicerProfileResult.points[i].cutFillHeight), $"{i}: Wrong cut-fill height");
      }

    }

    [TestMethod]
    public void CellStationsOutsideDesign()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          points = new List<CompactionProfileCell>
          {
            new CompactionProfileCell{station = 0, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 1, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 2, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 3, cutFillHeight = float.NaN},
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          points = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex{station = 0.5, elevation = 10},
            new CompactionProfileVertex{station = 1.5, elevation = 20},
            new CompactionProfileVertex{station = 2.5, elevation = 40},
          }
        };

      CompactionProfileResultHelper helper = new CompactionProfileResultHelper();
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.AreEqual(4, slicerProfileResult.points.Count, "Wrong number of profile points");
      Assert.IsTrue(float.IsNaN(slicerProfileResult.points[0].cutFillHeight), "0: Wrong cut-fill height");
      Assert.AreEqual(15, slicerProfileResult.points[1].cutFillHeight, "1: Wrong cut-fill height");
      Assert.AreEqual(30, slicerProfileResult.points[2].cutFillHeight, "2: Wrong cut-fill height");
      Assert.IsTrue(float.IsNaN(slicerProfileResult.points[3].cutFillHeight), "3: Wrong cut-fill height");
    }

    
    [TestMethod]
    public void CellStationsMatchDesign()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          points = new List<CompactionProfileCell>
          {
            new CompactionProfileCell{station = 0, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 1, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 2, cutFillHeight = float.NaN},
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          points = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex{station = 0, elevation = 10},
            new CompactionProfileVertex{station = 0.5, elevation = 30},
            new CompactionProfileVertex{station = 1.0, elevation = 20},
            new CompactionProfileVertex{station = 1.5, elevation = 10},
            new CompactionProfileVertex{station = 2.0, elevation = 40},
          }
        };

      CompactionProfileResultHelper helper = new CompactionProfileResultHelper();
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.AreEqual(3, slicerProfileResult.points.Count, "Wrong number of profile points");
      Assert.AreEqual(10, slicerProfileResult.points[0].cutFillHeight, "0: Wrong cut-fill height");
      Assert.AreEqual(20, slicerProfileResult.points[1].cutFillHeight, "1: Wrong cut-fill height");
      Assert.AreEqual(40, slicerProfileResult.points[2].cutFillHeight, "2: Wrong cut-fill height");
    }

    [TestMethod]
    public void CellStationsWithNoDesignElevation()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          points = new List<CompactionProfileCell>
          {
            new CompactionProfileCell{station = 0, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 1, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 2, cutFillHeight = float.NaN},
            new CompactionProfileCell{station = 3, cutFillHeight = float.NaN},
          }
        };
      CompactionProfileResult<CompactionProfileVertex> slicerDesignResult =
        new CompactionProfileResult<CompactionProfileVertex>
        {
          points = new List<CompactionProfileVertex>
          {
            new CompactionProfileVertex{station = 0.0, elevation = 10},
            new CompactionProfileVertex{station = 0.75, elevation = float.NaN},
            new CompactionProfileVertex{station = 1.5, elevation = 40},
            new CompactionProfileVertex{station = 2.25, elevation = 10},
            new CompactionProfileVertex{station = 2.9, elevation = float.NaN},
            new CompactionProfileVertex{station = 3.5, elevation = 40}
          }
        };

      CompactionProfileResultHelper helper = new CompactionProfileResultHelper();
      helper.FindCutFillElevations(slicerProfileResult, slicerDesignResult);

      Assert.AreEqual(4, slicerProfileResult.points.Count, "Wrong number of profile points");
      Assert.AreEqual(10, slicerProfileResult.points[0].cutFillHeight, "0: Wrong cut-fill height");
      Assert.IsTrue(float.IsNaN(slicerProfileResult.points[1].cutFillHeight), "1: Wrong cut-fill height");
      Assert.AreEqual(20, slicerProfileResult.points[2].cutFillHeight, "2: Wrong cut-fill height");
      Assert.IsTrue(float.IsNaN(slicerProfileResult.points[3].cutFillHeight), "3: Wrong cut-fill height");
    }

    [TestMethod]
    public void ConvertProfileResultWithNull()
    {
      CompactionProfileResultHelper helper = new CompactionProfileResultHelper();

      Assert.ThrowsException<ServiceException>(() => helper.ConvertProfileResult(null));
    }

    [TestMethod]
    public void ConvertProfileResultWithNoProfile()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult = new CompactionProfileResult<CompactionProfileCell>();

      CompactionProfileResultHelper helper = new CompactionProfileResultHelper();

      Assert.ThrowsException<ServiceException>(() => helper.ConvertProfileResult(slicerProfileResult));
    }

    [TestMethod]
    public void ConvertProfileResultSuccess()
    {
      CompactionProfileResult<CompactionProfileCell> slicerProfileResult =
        new CompactionProfileResult<CompactionProfileCell>
        {
          gridDistanceBetweenProfilePoints = 1.234,
          designFileUid = Guid.Empty,
          points = new List<CompactionProfileCell>
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
              speed = 5.9F,
              speedHeight = 0.56F,
              cutFill = 0.06F,
              cutFillHeight = 0.8F,
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
              speed = 8.1F,
              speedHeight = 0.7F,
              cutFill = 0.15F,
              cutFillHeight = 0.9F,
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
              speed = float.NaN,
              speedHeight = 0.65F,
              cutFill = float.NaN,
              cutFillHeight = float.NaN,
              passCountIndex = ValueTargetType.NoData,
              temperatureIndex = ValueTargetType.BelowTarget,
              cmvIndex = ValueTargetType.AboveTarget,
              mdpIndex = ValueTargetType.OnTarget,
              speedIndex = ValueTargetType.NoData
            }
          }
        };

      CompactionProfileResultHelper helper = new CompactionProfileResultHelper();

      var result = helper.ConvertProfileResult(slicerProfileResult);
      Assert.IsNotNull(result);
      Assert.AreEqual(slicerProfileResult.gridDistanceBetweenProfilePoints, result.gridDistanceBetweenProfilePoints,
        "Wrong gridDistanceBetweenProfilePoints");
      Assert.AreEqual(slicerProfileResult.designFileUid, result.designFileUid, "Wrong designFileUid");
      const int expectedCount = 14;
      Assert.AreEqual(expectedCount, result.points.Count, "Wrong number of profiles");
      string[] expectedTypes = { "firstPass", "highestPass", "lastPass", "lowestPass", "lastComposite",
        "cmvSummary", "cmvDetail", "cmvPercentChange", "mdpSummary", "temperatureSummary",
        "speedSummary", "passCountSummary", "passCountDetail", "cutFill" };
      for (int i = 0; i < expectedCount; i++)
      {       
        ValidateList(expectedTypes[i], i, slicerProfileResult.points, result.points);
      }
    }

    private void ValidateList(string expectedType, int i, List<CompactionProfileCell> expectedList, List<CompactionProfileData> actualList)
    {
      Assert.AreEqual(expectedType, actualList[i].type, $"{i}: Wrong type");
      Assert.AreEqual(expectedList.Count, actualList[i].data.Count, $"{i}: Wrong number of points");

      ValueTargetType expectedValueType = ValueTargetType.NoData;
      float expectedY2 = float.NaN;

      for (int j = 0; j < expectedList.Count; j++)
      {
        float expectedHeight = float.NaN;
        float expectedValue = float.NaN;
        switch (i)
        {
          case 0://firstPass
            expectedHeight = expectedList[j].firstPassHeight;
            expectedValue = expectedList[j].firstPassHeight;
            break;
          case 1://highestPass
            expectedHeight = expectedList[j].highestPassHeight;
            expectedValue = expectedList[j].highestPassHeight;
            break;
          case 2://lastPass
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].lastPassHeight;
            break;
          case 3://lowestPass
            expectedHeight = expectedList[j].lowestPassHeight;
            expectedValue = expectedList[j].lowestPassHeight;
            break;
          case 4://lastComposite
            expectedHeight = expectedList[j].lastCompositeHeight;
            expectedValue = expectedList[j].lastCompositeHeight;
            break;
          case 5://cmvSummary
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmvPercent;
            expectedValueType = expectedList[j].cmvIndex;
            break;
          case 6://cmvPercentChange
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmv;
            break;
          case 7://cmvDetail
            expectedHeight = expectedList[j].cmvHeight;
            expectedValue = expectedList[j].cmvPercentChange;
            break;
          case 8://mdpSummary
            expectedHeight = expectedList[j].mdpHeight;
            expectedValue = expectedList[j].mdpPercent;
            expectedValueType = expectedList[j].mdpIndex;
            break;
          case 9://temperatureSummary
            expectedHeight = expectedList[j].temperatureHeight;
            expectedValue = expectedList[j].temperature;
            expectedValueType = expectedList[j].temperatureIndex;
            break;
          case 10://speedSummary
            expectedHeight = expectedList[j].speedHeight;
            expectedValue = expectedList[j].speed;
            expectedValueType = expectedList[j].speedIndex;
            break;
          case 11://passCountSummary
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].topLayerPassCount;
            expectedValueType = expectedList[j].passCountIndex;
            break;
          case 12://passCountDetail
            expectedHeight = expectedList[j].lastPassHeight;
            expectedValue = expectedList[j].topLayerPassCount;
            break;
          case 13://cutFill
            expectedHeight = expectedList[j].lastCompositeHeight;
            expectedValue = expectedList[j].cutFill;
            expectedY2 = expectedList[j].cutFillHeight;
            break;
        }
        ValidatePoint(expectedType, j, expectedList[j], actualList[i].data[j], expectedHeight, expectedValue, expectedValueType, expectedY2);
      }
    }

    private void ValidatePoint(string expectedType, int j, CompactionProfileCell expectedCell, CompactionDataPoint actualResult, float expectedY, float expectedValue, ValueTargetType expectedValueType, float expectedY2)
    {
      Assert.AreEqual(expectedCell.cellType, actualResult.cellType, $"{j}: {expectedType} Wrong cellType");
      Assert.AreEqual(expectedCell.station, actualResult.x, $"{j}: {expectedType} Wrong x");
      Assert.AreEqual(expectedY, actualResult.y, $"{j}: {expectedType} Wrong y");
      Assert.AreEqual(expectedValue, actualResult.value, $"{j}: {expectedType} Wrong value");
      Assert.AreEqual(expectedValueType, actualResult.valueType, $"{j}: {expectedType} Wrong valueType");
      Assert.AreEqual(expectedY2, actualResult.y2, $"{j}: {expectedType} Wrong y2");
    }

  }
}
