using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    
  }
}
