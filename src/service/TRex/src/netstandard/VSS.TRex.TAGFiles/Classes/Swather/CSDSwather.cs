using System;
using Microsoft.Extensions.Logging;
using VSS.TRex.Cells;
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
using VSS.TRex.Designs.SVL.Utilities;

namespace VSS.TRex.TAGFiles.Classes.Swather
{
  public class CSDSwather : SwatherBase
  {

    private static readonly ILogger Log = Logging.Logger.CreateLogger<TerrainSwather>();

    private static readonly ICell_NonStatic_MutationHook hook = DIContext.Obtain<ICell_NonStatic_MutationHook>();

    /// <summary>
    /// The maximum number of cell passes that may be generated when swathing a single interval between
    /// two measurement epochs
    /// </summary>
    private const int kMaxNumberCellPassesPerSwathingEpoch = 25000;

    private readonly BoundingWorldExtent3D swathBounds = new BoundingWorldExtent3D();

    private int _processedEpochNumber;
    public int ProcessedEpochNumber { get => _processedEpochNumber; set => _processedEpochNumber = value; }

    public CSDSwather(TAGProcessorBase processor,
                          IProductionEventLists machineTargetValueChanges,
                          ISiteModel siteModel,
                          IServerSubGridTree grid,
                          Fence interpolationFence) : base(processor, machineTargetValueChanges, siteModel, grid, interpolationFence)
    {
    }

    private XYZ Multiply(XYZ v, double factor)
    {
      return new XYZ
      {
        X = v.X * factor,
        Y = v.Y * factor,
        Z = v.Z * factor
      };
    }

    private XYZ Add(XYZ v1, XYZ v2)
    {
      return new XYZ
      {
        X = v1.X + v2.X,
        Y = v1.Y + v2.Y,
        Z = v1.Z + v2.Z
      };
    }

    private double Get2DLength(XYZ p1, XYZ p2)
    {
      return Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
    }

    private void RotateBlade90(ref XYZ left, ref XYZ right)
    {
      double cx = (left.X + right.X) / 2;
      double cy = (left.Y + right.Y) / 2;
      double dx = cx - left.X;
      double dy = cy - left.Y;
      left.Y = cy + dx;
      left.X = cx + dy;
      dx = cx - right.X;
      dy = cy - right.Y;
      right.Y = cy + dx;
      right.X = cx + dy;
    }

    private double Distance(double x1, double y1, double x2, double y2)
    {
      return Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));
    }


    public override bool PerformSwathing(SimpleTriangle HeightInterpolator1,
                                         SimpleTriangle HeightInterpolator2,
                                         SimpleTriangle TimeInterpolator1,
                                         SimpleTriangle TimeInterpolator2,
                                         bool HalfPass,
                                         PassType passType,
                                         MachineSide machineSide)
    {
      _processedEpochNumber++;

      // Construct local geometric state and rotate measure epochs due to left and right side point measurement of cutter head
      // ie: Measurement context is co-linear with cutter head motion
      var FirstLeftPoint = HeightInterpolator1.V1;
      var FirstRightPoint = HeightInterpolator1.V2;

      var LastLeftPoint = HeightInterpolator2.V3;
      var LastRightPoint = HeightInterpolator2.V2;

      // turn blade 90 degrees
      RotateBlade90(ref FirstLeftPoint, ref FirstRightPoint);
      RotateBlade90(ref LastLeftPoint, ref LastRightPoint);

      // centre line through cylinder
      var FirstCenterPoint = Multiply(Add(FirstLeftPoint, FirstRightPoint), 0.5);
      var LastCenterPoint = Multiply(Add(LastLeftPoint, LastRightPoint), 0.5);

      // radius and elevation. In theory they are the same for left and right
      var radius = Distance(FirstLeftPoint.X, FirstLeftPoint.Y, FirstRightPoint.X, FirstRightPoint.Y) / 2;

      // Modify time interpolate state to take into account the rotated epochs
      TimeInterpolator1.V1.X = FirstLeftPoint.X;
      TimeInterpolator1.V1.Y = FirstLeftPoint.Y;
      TimeInterpolator1.V2.X = FirstRightPoint.X;
      TimeInterpolator1.V2.Y = FirstRightPoint.Y;
      TimeInterpolator1.V3.X = LastLeftPoint.X;
      TimeInterpolator1.V3.Y = LastLeftPoint.Y;

      TimeInterpolator2.V1.X = FirstRightPoint.X;
      TimeInterpolator2.V1.Y = FirstRightPoint.Y;
      TimeInterpolator2.V2.X = LastLeftPoint.X;
      TimeInterpolator2.V2.Y = LastLeftPoint.Y;
      TimeInterpolator2.V3.X = LastRightPoint.X;
      TimeInterpolator2.V3.Y = LastRightPoint.Y;

      // make fence based on 90 degree turned blade
      var CSDInterpolationFence = new Fence();
      CSDInterpolationFence.Points.Add(new FencePoint(FirstLeftPoint.X, FirstLeftPoint.Y, 0));
      CSDInterpolationFence.Points.Add(new FencePoint(FirstRightPoint.X, FirstRightPoint.Y, 0));
      CSDInterpolationFence.Points.Add(new FencePoint(LastRightPoint.X, LastRightPoint.Y, 0));
      CSDInterpolationFence.Points.Add(new FencePoint(LastLeftPoint.X, LastLeftPoint.Y, 0));

      // MinX/Y, MaxX/Y describe the world coordinate rectangle the encompasses
      // the pair of epochs denoting a processing interval.
      // Calculate the grid coverage of the bounding rectangle for the
      // quadrilateral held in the fence
      CSDInterpolationFence.UpdateExtents();

      // Calculate the grid coverage of the bounding rectangle for the
      // quadritateral held in the fence
      CSDInterpolationFence.GetExtents(out swathBounds.MinX, out swathBounds.MinY, out swathBounds.MaxX, out swathBounds.MaxY);
      //  InterpolationFence.GetExtents(out swathBounds.MinX, out swathBounds.MinY, out swathBounds.MaxX, out swathBounds.MaxY);

      hook?.EmitNote($"Interpolation extents: {swathBounds.MinX:F3},{swathBounds.MinY:F3} --> {swathBounds.MaxX:F3},{swathBounds.MaxY:F3}");

      // SIGLogMessage.PublishNoODS(Self,
      //                            Format('Swathing over rectangle: (%.3f, %.3f) -> (%.3f, %.3f) [%.3f wide by %.3f tall]', {SKIP}
      //                                   [fMinX, fMinY, fMaxX, fMaxY, fMaxX - fMinX, fMaxY - fMinY]),
      //                            slmcDebug);

      // We assume that we have a pair of epochs to compute IC information between
      // Determine the rectangle of cells that overlap the interval between the two epochs
      Grid.CalculateRegionGridCoverage(swathBounds, out BoundingIntegerExtent2D CellExtent);

      // Check that the swathing of this epoch will not create an inordinate number of cell passes
      // If so, prevent swathing of this epoch interval
      long CellCount = (long)CellExtent.SizeX * (long)CellExtent.SizeY;
      if (CellCount > kMaxNumberCellPassesPerSwathingEpoch)
      {
        Log.LogError($"Epoch {ProcessedEpochNumber} cell extents {CellExtent} (SizeX={CellExtent.SizeX}, SizeY={CellExtent.SizeX}) cover too many cell passes to swath ({CellCount}), limit is {kMaxNumberCellPassesPerSwathingEpoch} per epoch");
        return true;
      }

      if (hook != null)
      {
        hook.EmitNote($"Swathing: {CellExtent.MinX},{CellExtent.MinY} --> {CellExtent.MaxX},{CellExtent.MaxY}");

        // Emit count of cells matching quad boundary
        int cellCount = 0;

        // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
        for (int I = CellExtent.MinX; I <= CellExtent.MaxX; I++)
        {
          for (int J = CellExtent.MinY; J <= CellExtent.MaxY; J++)
          {
            Grid.GetCellCenterPosition(I, J, out double GridX, out double GridY);

            if (CSDInterpolationFence.IncludesPoint(GridX, GridY))
              cellCount++;
          }
        }

        hook.EmitNote($"Potential CellCount={cellCount}");
      }

      // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
      for (int I = CellExtent.MinX; I <= CellExtent.MaxX; I++)
      {
        for (int J = CellExtent.MinY; J <= CellExtent.MaxY; J++)
        {
          Grid.GetCellCenterPosition(I, J, out double GridX, out double GridY);

          if (CSDInterpolationFence.IncludesPoint(GridX, GridY))
          {
            double timeVal = Consts.NullDouble;
            double heightVal = Consts.NullDouble;

            if (TimeInterpolator1.IncludesPoint(GridX, GridY))
            {
              timeVal = TimeInterpolator1.InterpolateHeight(GridX, GridY);
              //     heightVal = HeightInterpolator1.InterpolateHeight(GridX, GridY);
            }
            else if (TimeInterpolator2.IncludesPoint(GridX, GridY))
            {
              timeVal = TimeInterpolator2.InterpolateHeight(GridX, GridY);
              //   heightVal = HeightInterpolator2.InterpolateHeight(GridX, GridY);
            }

            // Compute the distance along the line between the center point of the two half cylinders
            // representing the first and last epoc

            GeometryUtils.LineClosestPoint(GridX, GridY,  // Cell center point
                                     FirstCenterPoint.X, FirstCenterPoint.Y,
                                     LastCenterPoint.X, LastCenterPoint.Y,
                                     out double linePtx, out double linePty, out double stationToLine, out double offsetToLine);

            var testCenterPointZ = FirstCenterPoint.Z + (LastCenterPoint.Z - FirstCenterPoint.Z) * (stationToLine / Get2DLength(FirstCenterPoint, LastCenterPoint));

            heightVal = testCenterPointZ - Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(offsetToLine, 2));

            bool haveInterpolation = timeVal != Consts.NullDouble && heightVal != Consts.NullDouble;
            if (!haveInterpolation)
            {
              continue;  // We do not want to record this pass in this cell
            }

            DateTime _TheTime = DateTime.SpecifyKind(DateTime.FromOADate(timeVal), DateTimeKind.Utc);
            float _TheHeight = (float)heightVal;

            //if (_TheTime.ToString("yyyy-MM-dd HH-mm-ss.fff") == "2012-11-07 00-12-38.330")
            //{
            //  double d = TimeInterpolator2.InterpolateHeight(GridX, GridY);                                   
            //}

            // Check to see if the blade-on-the-ground flag is set. if not, then we will not process this epoch.
            // The reason for this is that there is no useful information for us while the blade is not on the ground.
            // There is a counter-argument to this in that customers may use a supervisor system to do an initial
            // topo survey of the site, in which case this flag may not be set. Currently this is not supported.
            // If you want the data processed, set the blade-on-the-ground flag.

            if (!(passType == PassType.Track || passType == PassType.Wheel))
            {
              if (Processor.OnGrounds.GetValueAtDateTime(_TheTime, OnGroundState.No) == OnGroundState.No)
                continue;
            }

            // Fill in all the details for the processed cell pass, using the tag event lookups
            // to make sure the appropriate values at the cell pass time are used.

            CellPass ProcessedCellPass = Cells.CellPass.CLEARED_CELL_PASS;

            if (BaseProductionDataSupportedByMachine)
            {
              // Prepare a processed pass record to include in the cell
              //ProcessedCellPass.InternalSiteModelMachineIndex = InternalSiteModelMachineIndex;
              ProcessedCellPass.InternalSiteModelMachineIndex = CellPassConsts.NullInternalSiteModelMachineIndex;
              ProcessedCellPass.Time = _TheTime;
              ProcessedCellPass.Height = _TheHeight;
              ProcessedCellPass.RadioLatency = Processor.AgeOfCorrections.GetValueAtDateTime(_TheTime, CellPassConsts.NullRadioLatency);

              double MachineSpd = Processor.ICMachineSpeedValues.GetValueAtDateTime(_TheTime, Consts.NullDouble);
              if (MachineSpd == Consts.NullDouble)
              {
                MachineSpd = Processor.CalculatedMachineSpeed;
              }

              // MachineSpeed is meters per second - we need to convert this to
              // centimeters per seconds for the cell pass
              if (MachineSpd != Consts.NullDouble &&
                  MachineSpd < 65535.0 / 100.0) // Machine too fast (its > 2358 km/hr)
              {
                ProcessedCellPass.MachineSpeed = (ushort)Math.Round(MachineSpd * 100);
              }

              ProcessedCellPass.gpsMode = Processor.GPSModes.GetValueAtDateTime(_TheTime, CellPassConsts.NullGPSMode);
              ProcessedCellPass.HalfPass = HalfPass;
              ProcessedCellPass.PassType = passType;

              CommitCellPassToModel(I, J, GridX, GridY, ProcessedCellPass);
            }
          }
        }
      }

      // Take care of swathing over sphere endpoints
      SwathSphere(passType, machineSide, FirstCenterPoint, radius, TimeInterpolator1.V1.Z);
      SwathSphere(passType, machineSide, LastCenterPoint, radius, TimeInterpolator1.V3.Z);

      return true;
    }

    /// <summary>
    /// Spherical swath over blade centers 
    /// </summary>
    /// <param name="passType"></param>
    /// <param name="machineSide"></param>
    /// <param name="centerPoint"></param>
    /// <param name="radius"></param>
    /// <param name="TheTime"></param>
    private void SwathSphere(PassType passType, MachineSide machineSide, XYZ centerPoint, double radius, double theTime)
    {

      swathBounds.MinX = centerPoint.X - radius;
      swathBounds.MinY = centerPoint.Y - radius;
      swathBounds.MaxX = centerPoint.X + radius;
      swathBounds.MaxY = centerPoint.Y + radius;

      hook?.EmitNote($"Interpolation extents: {swathBounds.MinX:F3},{swathBounds.MinY:F3} --> {swathBounds.MaxX:F3},{swathBounds.MaxY:F3}");

      // We assume that we have a pair of epochs to compute IC information between
      // Determine the rectangle of cells that overlap the interval between the two epochs
      Grid.CalculateRegionGridCoverage(swathBounds, out BoundingIntegerExtent2D CellExtent);

      // Check that the swathing of this epoch will not create an inordinate number of cell passes
      // If so, prevent swathing of this epoch interval
      long CellCount = (long)CellExtent.SizeX * (long)CellExtent.SizeY;
      if (CellCount > kMaxNumberCellPassesPerSwathingEpoch)
      {
        Log.LogError($"Epoch {ProcessedEpochNumber} cell extents {CellExtent} (SizeX={CellExtent.SizeX}, SizeY={CellExtent.SizeX}) cover too many cell passes to swath ({CellCount}), limit is {kMaxNumberCellPassesPerSwathingEpoch} per epoch");
        return;
      }

      // Scan the rectangle of grid cells, checking which of those fall within the quadrilateral
      for (int I = CellExtent.MinX; I <= CellExtent.MaxX; I++)
      {
        for (int J = CellExtent.MinY; J <= CellExtent.MaxY; J++)
        {
          Grid.GetCellCenterPosition(I, J, out double GridX, out double GridY);
          var deltaX = Distance(centerPoint.X, centerPoint.Y, GridX, GridY);

          if (deltaX <= radius)
          {

            double timeVal = theTime;
            // Calculate the height of the cell from the cylindrical swathing context
            double heightVal = centerPoint.Z - Math.Sqrt(Math.Pow(radius, 2) - Math.Pow(deltaX, 2));

            bool haveInterpolation = timeVal != Consts.NullDouble && heightVal != Consts.NullDouble;
            if (!haveInterpolation)
              continue;  // We do not want to record this pass in this cell

            DateTime _TheTime = DateTime.SpecifyKind(DateTime.FromOADate(timeVal), DateTimeKind.Utc);
            float _TheHeight = (float)heightVal;

            // Check to see if the blade-on-the-ground flag is set. if not, then we will not process this epoch.
            // The reason for this is that there is no useful information for us while the blade is not on the ground.
            // There is a counter-argument to this in that customers may use a supervisor system to do an initial
            // topo survey of the site, in which case this flag may not be set. Currently this is not supported.
            // If you want the data processed, set the blade-on-the-ground flag.

            if (!(passType == PassType.Track || passType == PassType.Wheel))
            {
              if (Processor.OnGrounds.GetValueAtDateTime(_TheTime, OnGroundState.No) == OnGroundState.No)
                continue;
            }

            // Fill in all the details for the processed cell pass, using the tag event lookups
            // to make sure the appropriate values at the cell pass time are used.

            CellPass ProcessedCellPass = Cells.CellPass.CLEARED_CELL_PASS;

            if (BaseProductionDataSupportedByMachine)
            {
              // Prepare a processed pass record to include in the cell
              //ProcessedCellPass.InternalSiteModelMachineIndex = InternalSiteModelMachineIndex;
              ProcessedCellPass.InternalSiteModelMachineIndex = CellPassConsts.NullInternalSiteModelMachineIndex;
              ProcessedCellPass.Time = _TheTime;
              ProcessedCellPass.Height = _TheHeight;
              ProcessedCellPass.RadioLatency = Processor.AgeOfCorrections.GetValueAtDateTime(_TheTime, CellPassConsts.NullRadioLatency);

              double MachineSpd = Processor.ICMachineSpeedValues.GetValueAtDateTime(_TheTime, Consts.NullDouble);
              if (MachineSpd == Consts.NullDouble)
              {
                MachineSpd = Processor.CalculatedMachineSpeed;
              }

              // MachineSpeed is meters per second - we need to convert this to
              // centimeters per seconds for the cell pass
              if (MachineSpd != Consts.NullDouble &&
                  MachineSpd < 65535.0 / 100.0) // Machine too fast (its > 2358 km/hr)
              {
                ProcessedCellPass.MachineSpeed = (ushort)Math.Round(MachineSpd * 100);
              }

              ProcessedCellPass.gpsMode = Processor.GPSModes.GetValueAtDateTime(_TheTime, CellPassConsts.NullGPSMode);
              ProcessedCellPass.HalfPass = false;
              ProcessedCellPass.PassType = passType;

              CommitCellPassToModel(I, J, GridX, GridY, ProcessedCellPass);
            }
          }
        }

      }

    }

  }
}
