using System;
using System.Collections.Generic;
using VSS.TRex.Common;
using VSS.TRex.DesignProfiling;
using VSS.TRex.Designs.Storage;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Client;
using VSS.TRex.Utilities;

namespace VSS.TRex.Profiling
{
  /// <summary>
  /// Contains the business logic to construct a vector of cells containing the infomation calculated for a profile along a vecotr of points
  /// </summary>
  public class CellProfileBuilder
  {
    public const int kMaxHzVtGridInterceptsToCalculate = 8000;

    private ISiteModel SiteModel;
    private double CellSize;
    public bool Aborted;

    private InterceptList VtIntercepts = new InterceptList();
    private InterceptList HzIntercepts = new InterceptList();
    private InterceptList VtHzIntercepts = new InterceptList();

    private bool SlicerToolUsed;
    private bool ReturnDesignElevation;
    private uint OTGCellX, OTGCellY;

    private SubGridCellAddress CurrentSubgridOrigin;
    private SubGridCellAddress ThisSubgridOrigin;

    private SubGridTreeBitmapSubGridBits FilterMask =
      new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

    private double StartX, StartY, StartStation;
    private double EndX, EndY, EndStation;
    private double CurrStationPos;
    private double Distance;

    private ClientHeightLeafSubGrid DesignElevations;
    DesignProfilerRequestResult DesignResult;

    public List<ProfileCell> ProfileCells { get; set; }
    public XYZ[] NEECoords { get; set; }
    public CellSpatialFilter CellFilter { get; set; }
    public Design Design { get; set; }

    public double GridDistanceBetweenProfilePoints;

    /// <summary>
    /// Default no-args constructor
    /// </summary>
    public CellProfileBuilder()
    {
    }

    /// <summary>
    /// Creates a CellProfile builder given a list of coordinates defining the path to profile and a container to place the resulting cells into
    /// </summary>
    /// <param name="nEECoords"></param>
    /// <param name="profileCells"></param>
    /// <param name="cellFilter"></param>
    /// <param name="design"></param>
    public CellProfileBuilder(ISiteModel siteModel,
      XYZ[] nEECoords,
      List<ProfileCell> profileCells,
      CellSpatialFilter cellFilter,
      Design design)
    {
      SiteModel = siteModel;
      NEECoords = nEECoords;
      ProfileCells = profileCells;
      CellFilter = cellFilter;
      Design = design;
    }

    /// <summary>
    /// Calculates all intercepts between the profile line and horizontal lines separating rows of cells
    /// </summary>
    /// <param name="startStation"></param>
    private void CalculateHorizontalIntercepts(double startStation)
    {
      // Find intersections of all horizontal grid rows
      int VGridLineStartIndex = (int) Math.Truncate(StartY / CellSize);
      int VGridLineEndIndex = (int) Math.Truncate(EndY / CellSize);
      int Increment = Math.Sign(VGridLineEndIndex - VGridLineStartIndex);

      // To find a match, tell the intersection matching method that each
      // horizontal gridline starts 'before' the StartX parameter and 'ends'
      // after the EndX parameter - use the CellSize as an arbitrary value for this.
      // This gets around an issue where with a perfectly vertical profile line,
      // no intersections were being determined.
      double HGridStartX = StartX;
      double HGridEndX = EndX;
      if (HGridStartX > HGridEndX)
        MinMax.Swap(ref HGridEndX, ref HGridStartX);

      HGridStartX = HGridStartX - CellSize;
      HGridEndX = HGridEndX + CellSize;

      int IntersectionCount = Math.Abs(VGridLineEndIndex - VGridLineStartIndex) + 1;

      while (IntersectionCount > 0 && HzIntercepts.Count < kMaxHzVtGridInterceptsToCalculate && !Aborted)
      {
        if (LineIntersection.LinesIntersect(StartX, StartY, EndX, EndY,
          HGridStartX, VGridLineStartIndex * CellSize,
          HGridEndX, VGridLineStartIndex * CellSize,
          out double IntersectX, out double IntersectY, true, out bool _))
        {
          HzIntercepts.AddPoint(IntersectX, IntersectY,
            startStation + MathUtilities.Hypot(StartX - IntersectX, StartY - IntersectY));
        }

        VGridLineStartIndex += Increment;
        IntersectionCount--;
      }
    }

    /// <summary>
    /// Calculates all intercepts between the profile line and vertical lines separating rows of cells
    /// </summary>
    /// <param name="startStation"></param>
    private void CalculateVerticalIntercepts(double startStation)
    {
      // Find intersections of all vertical grid columns
      int HGridLineStartIndex = (int) Math.Truncate(StartX / CellSize);
      int HGridLineEndIndex = (int) Math.Truncate(EndX / CellSize);
      int Increment = Math.Sign(HGridLineEndIndex - HGridLineStartIndex);

      // To find a match, tell the intersection matching method that each
      // vertical gridline starts 'before' the StartX parameter and 'ends'
      // after the EndX parameter - use the CellSize as an arbitrary value for this.
      // This gets around an issue where with a perfectly horizontal profile line,
      // no intersections were being determined.
      double VGridStartX = StartY;
      double VGridEndX = EndY;

      if (VGridStartX > VGridEndX)
        MinMax.Swap(ref VGridEndX, ref VGridStartX);

      VGridStartX -= CellSize;
      VGridEndX += CellSize;

      int IntersectionCount = Math.Abs(HGridLineEndIndex - HGridLineStartIndex) + 1;
      while (IntersectionCount > 0 && VtIntercepts.Count < kMaxHzVtGridInterceptsToCalculate && !Aborted)
      {
        if (LineIntersection.LinesIntersect(StartX, StartY, EndX, EndY,
          HGridLineStartIndex * CellSize, VGridStartX,
          HGridLineStartIndex * CellSize, VGridEndX,
          out double IntersectX, out double IntersectY, true, out bool _))
        {
          VtIntercepts.AddPoint(IntersectX, IntersectY,
            startStation + MathUtilities.Hypot(StartX - IntersectX, StartY - IntersectY));
        }

        HGridLineStartIndex += Increment;
        IntersectionCount--;
      }
    }

    /// <summary>
    /// Adds the data for a single cell into the list ocntaining the result fof the profile cells
    /// </summary>
    /// <param name="OTGX"></param>
    /// <param name="OTGY"></param>
    /// <param name="Station"></param>
    /// <param name="InterceptLength"></param>
    private void AddCellPassesDataToList(uint OTGX, uint OTGY, double Station, double InterceptLength)
    {
      ProfileCell ProfileCell = new ProfileCell()
      {
        OTGCellX = OTGX,
        OTGCellY = OTGY,
        Station = Station,
        InterceptLength = InterceptLength
      };

      if (DesignElevations != null)
        ProfileCell.DesignElev = DesignElevations.Cells[OTGX & TRex.SubGridTree.SubGridLocalKeyMask,
          OTGY & TRex.SubGridTree.SubGridLocalKeyMask];
      else
        ProfileCell.DesignElev = Consts.NullHeight;

      ProfileCells.Add(ProfileCell);
    }

    public bool Build()
    {
      CellSize = SiteModel.Grid.CellSize;

      CurrStationPos = 0;
      SlicerToolUsed = false;

      CurrentSubgridOrigin = new SubGridCellAddress(int.MaxValue, int.MaxValue);
      GridDistanceBetweenProfilePoints = 0;
      ReturnDesignElevation = Design != null;
      DesignElevations = null;

      for (int I = 0; I < NEECoords.Length - 1; I++) //for I := 0 to Arraycount - 2 do
      {
        StartX = NEECoords[I].X;
        StartY = NEECoords[I].Y;
        StartStation = NEECoords[I].Z;

        EndX = NEECoords[I + 1].X;
        EndY = NEECoords[I + 1].Y;
        EndStation = NEECoords[I + 1].Z;

        if (I == 0) // Add start point of profile line to intercept list
        {
          // this could be improved by passing a is slicertoolused variable but this method should be reliable
          SlicerToolUsed = NEECoords.Length == 2 && NEECoords[0].Z == 0 && NEECoords[1].Z == 0;
          if (SlicerToolUsed)
            CurrStationPos = 0;
          else
            CurrStationPos = NEECoords[I].Z; // alignment profiles pass in chainage for more accuracy
          VtHzIntercepts.AddPoint(StartX, StartY, CurrStationPos);
        }

        if (SlicerToolUsed)
          Distance = MathUtilities.Hypot(EndX - StartX, EndY - StartY); // chainage is not passed so compute
        else
          Distance = EndStation - StartStation; // use precise chainage passed

        if (Distance == 0) // if we have two points the same
          continue;

        // Get all intercepts between profile line and cell boundaries for this segment
        CalculateHorizontalIntercepts(CurrStationPos); // pass the distance down alignment this segment starts
        CalculateVerticalIntercepts(CurrStationPos);

        GridDistanceBetweenProfilePoints += Distance; // add actual distance along line
        CurrStationPos += Distance; // add distance to current station
      }

      // Merge vertical and horizontal cell boundary/profile line intercepts
      VtHzIntercepts.MergeInterceptLists(VtIntercepts, HzIntercepts);

      // Add end point of profile line to intercept list
      VtHzIntercepts.AddPoint(EndX, EndY, CurrStationPos);

      // Update each intercept with it's midpoint and intercept length
      // i.e. the midpoint on the line between one intercept and the next one
      // and the length between those intercepts
      VtHzIntercepts.UpdateMergedListInterceptMidPoints();

      if (VtHzIntercepts.Count > ProfileCells.Capacity)
        ProfileCells.Capacity = VtHzIntercepts.Count;

      // Iterate over all intercepts calculating the results for each cell that lies in
      // a subgrid handled by this node
      for (int i = 0; i < VtHzIntercepts.Count; i++)
      {
        if (Aborted)
          return false;

        // Determine the on-the-ground cell underneath the midpoint of each intercept line
        SiteModel.Grid.CalculateIndexOfCellContainingPosition(VtHzIntercepts.Items[i].MidPointX,
          VtHzIntercepts.Items[i].MidPointY, out OTGCellX, out OTGCellY);

        ThisSubgridOrigin = new SubGridCellAddress(OTGCellX >> TRex.SubGridTree.SubGridIndexBitsPerLevel,
          OTGCellY >> TRex.SubGridTree.SubGridIndexBitsPerLevel);

        if (!CurrentSubgridOrigin.Equals(ThisSubgridOrigin))
        {
          /* TODO: Affinity key calculation to detemrine if this node is responsible for this subgrid in the sequence along the profile
          //with VLPDDynamicSvcLocations do
          if (PSNodeServicingSubgrid(ThisSubgridOrigin.ToSkipInterleavedDescriptor).Descriptor != PSNodeDescriptors.ThisNodeDescriptor)
            continue;
          */

          CurrentSubgridOrigin = ThisSubgridOrigin;

          if (!ProfileFilterMask.ConstructSubgridCellFilterMask(CurrentSubgridOrigin, VtHzIntercepts, i, ref FilterMask, CellFilter, SiteModel.Grid))
            continue;

          if (ReturnDesignElevation) // cutfill profile request then get elevation at same spot along design
          {
            DesignElevations = null;
            DesignResult = DesignProfilerRequestResult.UnknownError;

            Design?.GetDesignHeights(SiteModel.ID, new SubGridCellAddress(OTGCellX, OTGCellY), CellSize,
              out DesignElevations, out DesignResult);

            if (DesignResult != DesignProfilerRequestResult.OK &&
                DesignResult != DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              continue;

            if (DesignResult == DesignProfilerRequestResult.NoElevationsInRequestedPatch)
              DesignElevations = null; // force a nullsingle to be written
          }
        }

        if (FilterMask.BitSet(OTGCellX & TRex.SubGridTree.SubGridLocalKeyMask,
          OTGCellY & TRex.SubGridTree.SubGridLocalKeyMask))
          AddCellPassesDataToList(OTGCellX, OTGCellY, VtHzIntercepts.Items[i].ProfileItemIndex,
            VtHzIntercepts.Items[i].InterceptLength);
      }

      return true;
    }
  }
}
