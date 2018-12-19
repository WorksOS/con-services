using System;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling.Models;
using VSS.TRex.Common.Utilities;

namespace VSS.TRex.Designs.TTM.Optimised.Profiling
{
  /// <summary>
  /// Contains the business logic to construct a vector of subgrids containing the triangles required to compute
  /// a profile along a vector of points
  /// </summary>
  public class OptimisedTTMCellProfileBuilder
  {
    /// <summary>
    /// The maximum number of subgrids permitted to be in the profile intercept list.
    /// At a cell size of 0.34 meters, this is roughly 20 kilometers in length
    /// </summary>
    private const int kMaxHzVtGridInterceptsToCalculate = 2000;

    private readonly double CellSize;

    private readonly InterceptList VtIntercepts = new InterceptList();
    private readonly InterceptList HzIntercepts = new InterceptList();

    /// <summary>
    /// The combined set of grid boundary intercepts representing the path taken by the profile line
    /// across the subgrids in the project.
    /// </summary>
    public InterceptList VtHzIntercepts { get; private set; } = new InterceptList();

    private readonly bool SlicerToolUsed;

    private double StartX, StartY, StartStation;
    private double EndX, EndY, EndStation;
    private double CurrentStationPos;
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
      CellSize = cellSize;

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

      while (IntersectionCount > 0 && HzIntercepts.Count < kMaxHzVtGridInterceptsToCalculate)
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
      while (IntersectionCount > 0 && VtIntercepts.Count < kMaxHzVtGridInterceptsToCalculate)
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
    /// <param name="originStation"></param>
    /// <returns></returns>
    public bool Build(XYZ[] nEECoords, double originStation)
    {
      CurrentStationPos = 0;

      for (int loopBound = nEECoords.Length - 1, I = 0; I < loopBound; I++)
      {
        StartX = nEECoords[I].X;
        StartY = nEECoords[I].Y;
        StartStation = nEECoords[I].Z;

        EndX = nEECoords[I + 1].X;
        EndY = nEECoords[I + 1].Y;
        EndStation = nEECoords[I + 1].Z;

        if (I == 0) 
        {
          CurrentStationPos = SlicerToolUsed ? originStation : nEECoords[I].Z; // alignment profiles pass in station for more accuracy

          // Add start point of profile line to intercept list
          VtHzIntercepts.AddPoint(StartX, StartY, CurrentStationPos);
        }

        Distance = SlicerToolUsed
          ? MathUtilities.Hypot(EndX - StartX, EndY - StartY) // station is not passed so compute
          : EndStation - StartStation; // use precise station passed

        if (Distance == 0) // if we have two points the same
          continue;

        // Get all intercepts between profile line and cell boundaries for this segment
        CalculateHorizontalIntercepts(CurrentStationPos); // pass the distance down alignment this segment starts
        CalculateVerticalIntercepts(CurrentStationPos);

        CurrentStationPos += Distance; // add distance to current station
      }

      // Merge vertical and horizontal cell boundary/profile line intercepts
      VtHzIntercepts.MergeInterceptLists(VtIntercepts, HzIntercepts);

      // Add end point of profile line to intercept list
       VtHzIntercepts.AddPoint(EndX, EndY, CurrentStationPos);

      // Update each intercept with it's midpoint and intercept length
      // i.e. the midpoint on the line between one intercept and the next one
      // and the length between those intercepts
      VtHzIntercepts.UpdateMergedListInterceptMidPoints();

      // Sort the intercept list to make life easier for the profiling code later on
      Array.Sort(VtHzIntercepts.Items, 0, VtHzIntercepts.Count); 

      return true;
    }
  }
}
