using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Common;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.DI;
using VSS.TRex.Events.Interfaces;
using VSS.TRex.Geometry;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.SubGridTrees.Server.Interfaces;
using VSS.TRex.TAGFiles.Classes.Processors;
using VSS.TRex.TAGFiles.Types;
using VSS.TRex.Types;

namespace VSS.TRex.TAGFiles.Classes.Swather
{
  /// <summary>
  /// Cutter Suction Dredge Swather
  /// </summary>
  public class CSDSwather : SwatherBase
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<CSDSwather>();

    private static readonly ICell_NonStatic_MutationHook Hook = DIContext.Obtain<ICell_NonStatic_MutationHook>();

    /// <summary>
    /// The maximum number of cell passes that may be generated when swathing a single interval between
    /// two measurement epochs
    /// </summary>
    private const int MaxNumberCellPassesPerSwathingEpoch = 25000;

    private readonly BoundingWorldExtent3D _swathBounds = new BoundingWorldExtent3D();

    private int _processedEpochNumber;
    public int ProcessedEpochNumber { get => _processedEpochNumber; set => _processedEpochNumber = value; }

    private XYZ FirstLeftPoint;
    private XYZ FirstRightPoint;
    private XYZ LastLeftPoint;
    private XYZ LastRightPoint;
    private XYZ FirstCenterPoint;
    private XYZ LastCenterPoint;
    private double CellElev;
    private double Radius;
    private double LinePtx;
    private double LinePty;
    private double OffsetToLine;
    private double StationToLine;
    private double TestCenterPointZ;

    public CSDSwather(TAGProcessorBase processor,
                          IProductionEventLists machineTargetValueChanges,
                          ISiteModel siteModel,
                          IServerSubGridTree grid,
                          Fence interpolationFence) : base(processor, machineTargetValueChanges, siteModel, grid, interpolationFence)
    {
    }

    /// <summary>
    /// Rotate blade 90 degrees due to sideways motion
    /// </summary>
    private void RotateBlade90(ref XYZ left, ref XYZ right)
    {
      var cx = (left.X + right.X) / 2.0;
      var cy = (left.Y + right.Y) / 2.0;
      var dx = cx - left.X;
      var dy = cy - left.Y;
      left.Y = cy + dx;
      left.X = cx + dy;
      dx = cx - right.X;
      dy = cy - right.Y;
      right.Y = cy + dx;
      right.X = cx + dy;
    }

    private double Distance(double x1, double y1, double x2, double y2)
    {
      return Math.Sqrt(Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0));
    }

    private void SwathSphere(PassType passType, MachineSide machineSide, XYZ centerPoint, double radius, double theTime)
    {


      var fMinX = centerPoint.X - radius;
      var fMinY = centerPoint.Y - radius;
      var fMaxX = centerPoint.X + radius;
      var fMaxY = centerPoint.Y + radius;

      // We assume that we have a pair of epochs to compute IC information between
      // Determine the rectangle of cells that overlap the interval between the two epochs
      Grid.CalculateRegionGridCoverage(new BoundingWorldExtent3D(fMinX, fMinY, fMaxX, fMaxY), out var cellExtent);
      var swatchCellCount = (long)cellExtent.SizeX * (long)cellExtent.SizeY;
      if (swatchCellCount > MaxNumberCellPassesPerSwathingEpoch)
      {
        Log.LogError($"Epoch {ProcessedEpochNumber} cell extents {cellExtent} (SizeX={cellExtent.SizeX}, SizeY={cellExtent.SizeX}) cover too many cell passes to swath sphere ({swatchCellCount}), limit is {MaxNumberCellPassesPerSwathingEpoch} per epoch");
        return;
      }

      // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
      for (var I = cellExtent.MinX; I <= cellExtent.MaxX; I++)
      {
        for (var J = cellExtent.MinY; J <= cellExtent.MaxY; J++)
        {
          Grid.GetCellCenterPosition(I, J, out var gridX, out var gridY);

          if (Distance(gridX, gridY, centerPoint.X, centerPoint.Y) <= radius)
          {

            var _TheTime = DateTime.SpecifyKind(DateTime.FromOADate(theTime), DateTimeKind.Utc);

            // Calculate the height of the cell from the cylindrical swathing context
            double deltaX = Distance(centerPoint.X, centerPoint.Y, gridX, gridY);

            var _TheHeight = centerPoint.Z - Math.Sqrt(Math.Pow(radius, 2.0) - Math.Pow(deltaX, 2.0));

            // Check to see if the blade-on-the-ground flag is set. if not, then we will not process this epoch.
            // The reason for this is that there is no useful information for us while the blade is not on the ground.
            // There is a counter-argument to this in that customers may use a supervisor system to do an initial
            // topo survey of the site, in which case this flag may not be set. Curently this is not supported.
            // If you want the data processed, set the blade-on-the-ground flag.
            if (!(passType == PassType.Track || passType == PassType.Wheel))
            {
              if (Processor.OnGrounds.GetValueAtDateTime(_TheTime, OnGroundState.No) == OnGroundState.No)
                continue;
            }

            // Fill in all the details for the processed cell pass, using the tag event lookups
            // to make sure the appropriate values at the cell pass time are used.
            var processedCellPass = Cells.CellPass.CLEARED_CELL_PASS;

            if (BaseProductionDataSupportedByMachine)
            {
              // Prepare a processed pass record to include in the cell
              processedCellPass.InternalSiteModelMachineIndex = MachineTargetValueChanges.InternalSiteModelMachineIndex;
              processedCellPass.Time = _TheTime;
              processedCellPass.Height = (float)_TheHeight;
              processedCellPass.RadioLatency = Processor.AgeOfCorrections.GetValueAtDateTime(_TheTime, CellPassConsts.NullRadioLatency);
              var machineSpd = Processor.ICMachineSpeedValues.GetValueAtDateTime(_TheTime, Consts.NullDouble);
              if (machineSpd == Consts.NullDouble)
                machineSpd = Processor.CalculatedMachineSpeed;

              // MachineSpeed is meters per second - we need to convert this to
              // centimeters per seconds for the cell pass
              if (machineSpd != Consts.NullDouble &&  machineSpd < 65535.0 / 100.0) // Machine too fast (its > 2358 km/hr)
                processedCellPass.MachineSpeed = (ushort)Math.Round(machineSpd * 100);

              processedCellPass.gpsMode = Processor.GPSModes.GetValueAtDateTime(_TheTime, CellPassConsts.NullGPSMode);

              processedCellPass.PassType = passType;

              CommitCellPassToModel(I, J, gridX, gridY, processedCellPass, true); // commit as lowestpassonly = true
            }
          }
        }
      }
    }


    public override bool PerformSwathing(SimpleTriangle heightInterpolator1,
                                         SimpleTriangle heightInterpolator2,
                                         SimpleTriangle timeInterpolator1,
                                         SimpleTriangle timeInterpolator2,
                                         bool halfPass,
                                         PassType passType,
                                         MachineSide machineSide)
    {
      _processedEpochNumber++;


      // Construct local geometric state and rotate measure epochs due to left and right side point measurement of cutter head
      // ie: Measurement context is co-linear with cutter head motion
      FirstLeftPoint = heightInterpolator1.V1;
      FirstRightPoint = heightInterpolator1.V2;

      LastLeftPoint = Processor.LastLeftPoint;   //heightInterpolator2.V1;
      LastRightPoint = Processor.LastRightPoint;  //heightInterpolator2.V2;

      // turn blade 90 degrees
      RotateBlade90(ref FirstLeftPoint, ref FirstRightPoint);
      RotateBlade90(ref LastLeftPoint, ref LastRightPoint);

      // centre line through cylinder
      FirstCenterPoint = (FirstLeftPoint + FirstRightPoint) * 0.5;
      LastCenterPoint = (LastLeftPoint + LastRightPoint) * 0.5;

      // radius and elevation. In theory they are the same for left and right
      Radius = Distance(FirstLeftPoint.X, FirstLeftPoint.Y, FirstRightPoint.X, FirstRightPoint.Y) / 2.0;

      // Modify time interpolate state to take into account the rotated epochs
      timeInterpolator1.V1.X = FirstLeftPoint.X;
      timeInterpolator1.V1.Y = FirstLeftPoint.Y;
      timeInterpolator1.V2.X = FirstRightPoint.X;
      timeInterpolator1.V2.Y = FirstRightPoint.Y;
      timeInterpolator1.V3.X = LastLeftPoint.X;
      timeInterpolator1.V3.Y = LastLeftPoint.Y;

      timeInterpolator2.V1.X = FirstRightPoint.X;
      timeInterpolator2.V1.Y = FirstRightPoint.Y;
      timeInterpolator2.V2.X = LastLeftPoint.X;
      timeInterpolator2.V2.Y = LastLeftPoint.Y;
      timeInterpolator2.V3.X = LastRightPoint.X;
      timeInterpolator2.V3.Y = LastRightPoint.Y;

      // make fence based on 90 degree turned blade
      var interpolationCSDFence = new Fence();
      interpolationCSDFence.Points.Add(new FencePoint(FirstLeftPoint.X, FirstLeftPoint.Y, 0));
      interpolationCSDFence.Points.Add(new FencePoint(FirstRightPoint.X, FirstRightPoint.Y, 0));
      interpolationCSDFence.Points.Add(new FencePoint(LastRightPoint.X, LastRightPoint.Y, 0));
      interpolationCSDFence.Points.Add(new FencePoint(LastLeftPoint.X, LastLeftPoint.Y, 0));

      // MinX/Y, MaxX/Y describe the world coordinate rectangle the encompasses
      // the pair of epochs denoting a processing interval.
      // Calculate the grid coverage of the bounding rectangle for the
      // quadrilateral held in the fence
      interpolationCSDFence.UpdateExtents();
      interpolationCSDFence.GetExtents(out _swathBounds.MinX, out _swathBounds.MinY, out _swathBounds.MaxX, out _swathBounds.MaxY);

      Hook?.EmitNote($"Interpolation extents: {_swathBounds.MinX:F3},{_swathBounds.MinY:F3} --> {_swathBounds.MaxX:F3},{_swathBounds.MaxY:F3}");

      // We assume that we have a pair of epochs to compute IC information between
      // Determine the rectangle of cells that overlap the interval between the two epochs
      Grid.CalculateRegionGridCoverage(_swathBounds, out var cellExtent);

      // Check that the swathing of this epoch will not create an inordinate number of cell passes
      // If so, prevent swathing of this epoch interval
      var swatchCellCount = (long)cellExtent.SizeX * (long)cellExtent.SizeY;
      if (swatchCellCount > MaxNumberCellPassesPerSwathingEpoch)
      {
        Log.LogError($"Epoch {ProcessedEpochNumber} cell extents {cellExtent} (SizeX={cellExtent.SizeX}, SizeY={cellExtent.SizeX}) cover too many cell passes to swath ({swatchCellCount}), limit is {MaxNumberCellPassesPerSwathingEpoch} per epoch");
        return true;
      }

      if (Hook != null)
      {
        Hook.EmitNote($"Swathing: {cellExtent.MinX},{cellExtent.MinY} --> {cellExtent.MaxX},{cellExtent.MaxY}");

        // Emit count of cells matching quad boundary
        var cellCount = 0;

        // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
        for (var I = cellExtent.MinX; I <= cellExtent.MaxX; I++)
        {
          for (var J = cellExtent.MinY; J <= cellExtent.MaxY; J++)
          {
            Grid.GetCellCenterPosition(I, J, out var gridX, out var gridY);

            if (interpolationCSDFence.IncludesPoint(gridX, gridY))
              cellCount++;
          }
        }

        Hook.EmitNote($"Potential CellCount={cellCount}");
      }

      // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
      for (var I = cellExtent.MinX; I <= cellExtent.MaxX; I++)
      {
        for (var J = cellExtent.MinY; J <= cellExtent.MaxY; J++)
        {
          Grid.GetCellCenterPosition(I, J, out var gridX, out var gridY);

          if (interpolationCSDFence.IncludesPoint(gridX, gridY))
          {
            var timeVal = Consts.NullDouble;

            if (timeInterpolator1.IncludesPoint(gridX, gridY))
            {
              timeVal = timeInterpolator1.InterpolateHeight(gridX, gridY);
            }
            else if (timeInterpolator2.IncludesPoint(gridX, gridY))
            {
              timeVal = timeInterpolator2.InterpolateHeight(gridX, gridY);
            }

            if (timeVal == Consts.NullDouble)
              continue;  // We do not want to record this pass in this cell


            GeometryUtils.LineClosestPoint(gridX, gridY,  // Cell center point
                                     FirstCenterPoint.X, FirstCenterPoint.Y,
                                     LastCenterPoint.X, LastCenterPoint.Y,
                                     out LinePtx, out LinePty, out StationToLine, out OffsetToLine);


            TestCenterPointZ = FirstCenterPoint.Z + (LastCenterPoint.Z - FirstCenterPoint.Z) * (StationToLine / XYZ.Get2DLength(FirstCenterPoint, LastCenterPoint));

            var heightVal = TestCenterPointZ - Math.Sqrt(Math.Pow(Radius, 2.0) - Math.Pow(OffsetToLine, 2.0));

            var theTime = DateTime.SpecifyKind(DateTime.FromOADate(timeVal), DateTimeKind.Utc);
            var theHeight = (float)heightVal;

            // Check to see if the blade-on-the-ground flag is set. if not, then we will not process this epoch.
            // The reason for this is that there is no useful information for us while the blade is not on the ground.
            // There is a counter-argument to this in that customers may use a supervisor system to do an initial
            // topo survey of the site, in which case this flag may not be set. Currently this is not supported.
            // If you want the data processed, set the blade-on-the-ground flag.

            if (!(passType == PassType.Track || passType == PassType.Wheel))
            {
              if (Processor.OnGrounds.GetValueAtDateTime(theTime, OnGroundState.No) == OnGroundState.No)
                continue;
            }

            // Fill in all the details for the processed cell pass, using the tag event lookups
            // to make sure the appropriate values at the cell pass time are used.
            var processedCellPass = Cells.CellPass.CLEARED_CELL_PASS;

            if (BaseProductionDataSupportedByMachine)
            {
              // Prepare a processed pass record to include in the cell
              processedCellPass.InternalSiteModelMachineIndex = MachineTargetValueChanges.InternalSiteModelMachineIndex;
              processedCellPass.Time = theTime;
              processedCellPass.Height = theHeight;
              processedCellPass.RadioLatency = Processor.AgeOfCorrections.GetValueAtDateTime(theTime, CellPassConsts.NullRadioLatency);
              var machineSpd = Processor.ICMachineSpeedValues.GetValueAtDateTime(theTime, Consts.NullDouble);
              if (machineSpd == Consts.NullDouble)
                machineSpd = Processor.CalculatedMachineSpeed;

              // MachineSpeed is meters per second - we need to convert this to
              // centimeters per seconds for the cell pass
              if (machineSpd != Consts.NullDouble &&
                  machineSpd < 65535.0 / 100.0) // Machine too fast (its > 2358 km/hr)
              {
                processedCellPass.MachineSpeed = (ushort)Math.Round(machineSpd * 100);
              }

              processedCellPass.gpsMode = Processor.GPSModes.GetValueAtDateTime(theTime, CellPassConsts.NullGPSMode);

              processedCellPass.HalfPass = halfPass;
              processedCellPass.PassType = passType;

              CommitCellPassToModel(I, J, gridX, gridY, processedCellPass, true);// commit as lowestpassonly = true
            }
          }
        }
      }

      // Take care of swathing over sphere endpoints
      SwathSphere(passType, machineSide, FirstCenterPoint, Radius, timeInterpolator1.V1.Z);
      SwathSphere(passType, machineSide, LastCenterPoint, Radius, timeInterpolator1.V3.Z);

      return true;
    }
  }
}
