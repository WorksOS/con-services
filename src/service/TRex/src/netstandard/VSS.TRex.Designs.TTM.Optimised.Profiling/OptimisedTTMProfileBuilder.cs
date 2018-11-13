using System;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Utilities;

namespace VSS.TRex.Designs.TTM.Optimised.Profiling
{
  /// <summary>
  /// Contains the business logic to construct a vector of subgrids containing the triangles required to compute
  /// a profile along a vector of points
  /// </summary>
  public class OptimisedTTMCellProfileBuilder
  {
    public const int kMaxHzVtGridInterceptsToCalculate = 8000;

    private double CellSize;
    public bool Aborted { get; set; }

    private readonly InterceptList VtIntercepts = new InterceptList();
    private readonly InterceptList HzIntercepts = new InterceptList();
    public InterceptList VtHzIntercepts = new InterceptList();

    private readonly bool SlicerToolUsed;

    private double StartX, StartY, StartStation;
    private double EndX, EndY, EndStation;
    private double CurrStationPos;
    private double Distance;

    /// <summary>
    /// Creates a CellProfile builder given a list of coordinates defining the path to profile and a container to place the resulting cells into
    /// </summary>
    /// <param name="cellSize"></param>
    /// <param name="slicerToolUsed"></param>
    public OptimisedTTMCellProfileBuilder(double cellSize, bool slicerToolUsed)
    {
      // Modify the cell size to represent the node subgrid above the leaf subgrid as these are the size of the
      // cells in the spatial TIN index
      CellSize = cellSize * SubGridTreeConsts.SubGridTreeDimension;

      SlicerToolUsed = slicerToolUsed;
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
      // horizontal grid line starts 'before' the StartX parameter and 'ends'
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
      // vertical grid line starts 'before' the StartX parameter and 'ends'
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
    /// Constructs a vector of cells in profileCells along the path of the profile geometry containing in nEECoords
    /// </summary>
    /// <param name="nEECoords"></param>
    /// <returns></returns>
    public bool Build(XYZ[] nEECoords)
    {
      CurrStationPos = 0;

      for (int loopBound = nEECoords.Length - 1, I = 0; I < loopBound; I++)
      {
        StartX = nEECoords[I].X;
        StartY = nEECoords[I].Y;
        StartStation = nEECoords[I].Z;

        EndX = nEECoords[I + 1].X;
        EndY = nEECoords[I + 1].Y;
        EndStation = nEECoords[I + 1].Z;

        if (I == 0) // Add start point of profile line to intercept list
        {
          CurrStationPos = SlicerToolUsed ? 0 : nEECoords[I].Z; // alignment profiles pass in chainage for more accuracy
          VtHzIntercepts.AddPoint(StartX, StartY, CurrStationPos);
        }

        Distance = SlicerToolUsed
          ? MathUtilities.Hypot(EndX - StartX, EndY - StartY) // chainage is not passed so compute
          : EndStation - StartStation; // use precise chainage passed

        if (Distance == 0) // if we have two points the same
          continue;

        // Get all intercepts between profile line and cell boundaries for this segment
        CalculateHorizontalIntercepts(CurrStationPos); // pass the distance down alignment this segment starts
        CalculateVerticalIntercepts(CurrStationPos);

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

      return true;
    }
  }
}

