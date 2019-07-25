using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Designs.Models;
using VSS.TRex.Designs.SVL;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees;

namespace VSS.TRex.Designs
{
  public class SVLAlignmentDesign : DesignBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SVLAlignmentDesign>();

    /// <summary>
    /// Reference to the NFF file representing the SVL description of a set of guidance alignments
    /// </summary>
    //private TNFFFile NFFFile;

    /// <summary>
    /// Represents the master guidance alignment selected from the NFFFile.
    /// </summary>
    private TNFFGuidableAlignmentEntity Data;

    /// <summary>
    /// Constructs a guidance alignment design with a cell size used for computing filter patches
    /// </summary>
    /// <param name="ACellSize"></param>
    public SVLAlignmentDesign(double ACellSize)
    {

    }

    public override bool ComputeFilterPatch(double StartStn, double EndStn, double LeftOffset, double RightOffset, SubGridTreeBitmapSubGridBits Mask, SubGridTreeBitmapSubGridBits Patch, double OriginX, double OriginY, double CellSize, double Offset)
    {
      throw new NotImplementedException();
    }

    public override List<XYZS> ComputeProfile(XYZ[] profilePath, double cellSize)
    {
      return new List<XYZS>();
    }

    public override List<Fence> GetBoundary()
    {
      throw new NotImplementedException();
    }

    public override void GetExtents(out double x1, out double y1, out double x2, out double y2)
    {
      throw new NotImplementedException();
    }

    public override void GetHeightRange(out double z1, out double z2)
    {
      z1 = Consts.NullDouble;
      z2 = Consts.NullDouble;
    }

    public override bool HasElevationDataForSubGridPatch(double X, double Y)
    {
      return false;
    }

    public override bool HasElevationDataForSubGridPatch(int SubGridX, int SubGridY)
    {
      return false;
    }

    public override bool HasFiltrationDataForSubGridPatch(double X, double Y)
    {
      return false;
    }

    public override bool HasFiltrationDataForSubGridPatch(int SubGridX, int SubGridY)
    {
      return false;
    }

    public override bool InterpolateHeight(ref int Hint, double X, double Y, double Offset, out double Z)
    {
      Z = Consts.NullDouble;
      return false;
    }

    public override bool InterpolateHeights(float[,] Patch, double OriginX, double OriginY, double CellSize, double Offset)
    {
      return false;
    }

    public override DesignLoadResult LoadFromFile(string fileName, bool saveIndexFiles = true)
    {
      var Result = DesignLoadResult.UnknownFailure;
      var NFFFile = TNFFFile.CreateFromFile(fileName);

      try
      {
        Result = DesignLoadResult.NoAlignmentsFound;

        for (int I = 0; I < NFFFile.GuidanceAlignments.Count; I++)
        {
          if (NFFFile.GuidanceAlignments[I].IsMasterAlignment())
          {
            Data = NFFFile.GuidanceAlignments[I] as TNFFGuidableAlignmentEntity;

            NFFFile.GuidanceAlignments.RemoveAt(I);

            if (Data != null)
              Result = DesignLoadResult.Success;

            break;
          }
        }
      }
      catch (Exception e)
      {
        Log.LogError(e, $"Exception in {nameof(LoadFromFile)}");
        Result = DesignLoadResult.UnknownFailure;
      }

      return Result;
    }

    public override Task<DesignLoadResult> LoadFromStorage(Guid siteModelUid, string fileName, string localPath, bool loadIndices = false)
    {
      throw new NotImplementedException();
    }
  }
}
