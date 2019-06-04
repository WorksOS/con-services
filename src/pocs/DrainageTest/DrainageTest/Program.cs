using Core.Contracts.SurfaceImportExport;
using Core.Services.SurfaceImportExport;
using Microsoft.Practices.ServiceLocation;
using Morph.Contracts.Interfaces;
using Morph.Core.Utility;
using Morph.Module.Services.QAInputOutput;
using Morph.Module.Services.Utility.Fmx;
using Morph.Module.Services.Utility.MultiPlane;
using Morph.Services.Core.DataModel;
using Morph.Services.Core.Interfaces;
using Morph.Services.Core.Tools;
using SkuTester.DataModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using Microsoft.Practices.Prism.Logging;

namespace DrainageTest
{
  public class Program
  {
    static void Main(string[] args)
    {
      ILoggerFacade logger = (ILoggerFacade)null;
      try
      {
        Thread thread = new Thread(new ThreadStart(Program.CreateApp));
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Thread.Sleep(100);
        new BootStrapper().Run();

        logger = ServiceLocator.Current.GetInstance<ILoggerFacade>();
        logger.Log("here", Category.Info, Priority.Low);

        //  try arg     ..\..\TestData\Sample\TestCase.xml
        if (args.Length != 1)
          throw new ArgumentException("Should be 1 argument i.e. the xml path and filename");

        var stringBuilder = new StringBuilder();
        foreach (string str in args)
          stringBuilder.Append($"{str} ");
        Console.WriteLine($"{nameof(Main)}: args: {stringBuilder}");

        Program.Execute(args, logger);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"{nameof(Main)}: exception: {ex}");
        logger.Log($"{nameof(Main)} Exception: {ex}", Category.Exception, Priority.High);
      }
      finally
      {
        //Application.Current.Dispatcher.Invoke((Action)(() => MediaTypeNames.Application.Current.Shutdown()));
      }
    }

    private static void Execute(string[] args, ILoggerFacade logger)
    {
      string commandLineArgument = Program.ParseCommandLineArguments(args)["input"];
      Console.WriteLine($"{nameof(Execute)}: args: {commandLineArgument}");
      if (!System.IO.File.Exists(commandLineArgument))
        throw new ArgumentException($"Unable to locate the xml file: {commandLineArgument}");

      TestCase useCase = TestCase.Load(Path.GetFullPath(commandLineArgument));
      Console.WriteLine($"{nameof(Execute)}: surface file: {useCase.Surface}");

      // just trying this util for grins
      var rowsDirection =
        Morph.Services.Core.DataModel.Utils.NormalizeAngleRad(
          -useCase.FurrowHeading * (Math.PI / 180.0) + Math.PI / 2.0, -1.0 * Math.PI);
      Console.WriteLine($"{nameof(Execute)}: rowsDirection: {rowsDirection}");

      bool flag = false;
      using (ILandLeveling instance = ServiceLocator.Current.GetInstance<ILandLeveling>())
      {
        ISurfaceInfo surfaceInfo = null;
        if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(useCase.Surface), ".dxf") == 0)
          /* this fails trying to load
             Could not load file or assembly 'TD_SwigDbMgd, Version=4.0.0.0, Culture=neutral, PublicKeyToken=5ccc28765cdf0a88' or one of its dependencies. 
             An attempt was made to load a program with an incorrect format.
             ((System.BadImageFormatException)ex).FusionLog
          */
          surfaceInfo = instance.ImportSurface(useCase.Surface, (Action<float>) null);

        //else if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(useCase.Surface), ".xml") == 0)
        //{
        //  FieldLevelData fieldLevelData = FieldLevelData.Load(useCase.Surface);
        //  Point3D transform = fieldLevelData.Origin.Transform;
        //  if (Math.Abs(transform.X) < 1E-06 && Math.Abs(transform.Y) < 1E-06 && Math.Abs(transform.Z) < 1E-06)
        //  {
        //    transform.Z = fieldLevelData.Origin.Altitude;
        //    fieldLevelData.Origin.Transform = transform;
        //  }
        //  surfaceInfo = instance.CreateSurface(Path.GetFileNameWithoutExtension(useCase.Surface), fieldLevelData.BoundaryPoints.Union<Point3D>(fieldLevelData.SurveyPoints), (IList<int>)null, fieldLevelData.BoundaryPoints.Select<Point3D, Point>((Func<Point3D, Point>)(p => p.ToPoint())), (IEnumerable<IEnumerable<Point>>)null, double.NaN);
        //}
        //else if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(useCase.Surface), ".gbg") == 0)
        //{
        //  flag = true;
        //  SurfaceInfoDump surfaceInfoDump = SurfaceInfoDump.LoadSurface(useCase.Surface);
        //  logger.LogVerbose("SkuTester.Execute", "Morph TestCase file: {0} Boundary: {1} Point: {2} Triangle: {3} ", (object)useCase.Surface, (object)surfaceInfoDump.Boundary.Count, (object)surfaceInfoDump.Points.Count, (object)surfaceInfoDump.Triangles.Count);
        //  surfaceInfo = instance.CreateSurface(Path.GetFileNameWithoutExtension(useCase.Surface), (IEnumerable<Point3D>)surfaceInfoDump.Points, surfaceInfoDump.Triangles, (IEnumerable<Point>)surfaceInfoDump.Boundary, (IEnumerable<IEnumerable<Point>>)null, double.NaN);
        //}
        //else
        //{
        //  MultiPlaneParser multiPlaneParser = new MultiPlaneParser(logger, new Morph.Core.Utility.UnitsManager.UnitsManager(ServiceLocator.Current.GetInstance<UnitConverter>(), (IPreferences)null));
        //  MultiPlaneSettings multiPlaneSettings = new MultiPlaneSettings()
        //  {
        //    CoordinateSystem = useCase.IsXYZ ? MulitplaneCoordinateSystems.Xyz : MulitplaneCoordinateSystems.Yxz,
        //    DistanceType = useCase.IsMetric ? MultiplaneDistanceTypes.Meters : MultiplaneDistanceTypes.Feet,
        //    HasId = useCase.HasPointIds
        //  };
        //  string surface = useCase.Surface;
        //  MultiPlaneSettings settings = multiPlaneSettings;
        //  List<Point3D> source;
        //  ref List<Point3D> local1 = ref source;
        //  Origin origin;
        //  ref Origin local2 = ref origin;
        //  IDesignEditImports designEditImports;
        //  ref IDesignEditImports local3 = ref designEditImports;
        //  List<Point3D> pointsFromTextFile = multiPlaneParser.GetPointsFromTextFile(surface, settings, out local1, out local2, out local3);
        //  surfaceInfo = instance.CreateSurface(Path.GetFileNameWithoutExtension(useCase.Surface), (IEnumerable<Point3D>)pointsFromTextFile, (IList<int>)null, source.Select<Point3D, Point>((Func<Point3D, Point>)(p => p.ToPoint())), (IEnumerable<IEnumerable<Point>>)null, double.NaN);
        //}

        if (!useCase.IsMetric)
          useCase = useCase.AsMetric();
        Design design = (Design) null;
        switch (useCase.Compute)
        {
          //    case ComputeEnum.SinglePlaneBestFit:
          //      if (useCase.Sections.Count == 0)
          //        useCase.Sections.Add(new PlanesConstraints()
          //        {
          //          Boundary = useCase.Boundary.Points.Count > 2 ? useCase.Boundary : new Linestring((IEnumerable<Point>)surfaceInfo.Boundary),
          //          MinimumSlope = useCase.MinSlope,
          //          MaximumSlope = useCase.MaxSlope,
          //          Shrinkage = useCase.Shrinkage,
          //          Bulkage = useCase.Bulkage
          //        });
          //      else if (!flag)
          //      {
          //        foreach (PlanesConstraints section in useCase.Sections)
          //        {
          //          section.Shrinkage = useCase.Shrinkage;
          //          section.Bulkage = useCase.Bulkage;
          //        }
          //      }
          //      design = instance.ComputePlanes((IList<PlanesConstraints>)useCase.Sections, (IEnumerable<Linestring>)useCase.ExclusionZones, useCase.ExportVolume, (Predicate<float>)null);
          //      break;

          case ComputeEnum.SurfaceBestFit:
            SurfaceConstraints constraints1 = new SurfaceConstraints();
            constraints1.Resolution = useCase.Resolution;
            constraints1.Boundary = useCase.Boundary;
            constraints1.TargetDitches = useCase.TargetDitches;
            constraints1.ExclusionZones.AddRange((IEnumerable<Linestring>) useCase.ExclusionZones);
            constraints1.Areas.AddRange((IEnumerable<AreaConstraints>) useCase.Areas);
            if (!constraints1.Areas.Any<AreaConstraints>())
              constraints1.Areas.Add(new AreaConstraints()
              {
                Tag = "Field",
                Boundary = useCase.Boundary.Points.Count > 2
                  ? useCase.Boundary
                  : new Linestring((IEnumerable<Point>) surfaceInfo.Boundary),
                MinimumSlope = useCase.MinSlope,
                MaximumSlope = useCase.MaxSlope,
                MaximumCutDepth = useCase.MaxCutDepth,
                MaximumFillHeight = useCase.MaxFillHeight,
                Shrinkage = useCase.Shrinkage,
                Bulkage = useCase.Bulkage,
                ExportVolume = useCase.ExportVolume
              });
            foreach (AreaConstraints area in useCase.Areas)
            {
              area.Shrinkage = useCase.Shrinkage;
              area.Bulkage = useCase.Bulkage;
            }

            for (int index = 0; index < constraints1.Areas.Count; ++index)
            {
              if (string.IsNullOrEmpty(constraints1.Areas[index].Tag))
                constraints1.Areas[index].Tag = string.Format("Area{0:00}", (object) (index + 1));
            }

            design = instance.ComputeSurface(constraints1, (Predicate<float>) null);
            break;
          //    case ComputeEnum.Furrows:
          //      RowsConstraints constraints2 = new RowsConstraints();
          //      constraints2.Boundary = useCase.Boundary;
          //      constraints2.MinimumSlope = useCase.MinSlope;
          //      constraints2.MaximumSlope = useCase.MaxSlope;
          //      constraints2.MaximumSlopeChange = useCase.MaxSlopeChange;
          //      constraints2.MinimumCrossSlope = useCase.MinCrossSlope;
          //      constraints2.MaximumCrossSlope = useCase.MaxCrossSlope;
          //      constraints2.MaximumCrossSlopeChange = useCase.MaxCrossSlopeChange;
          //      constraints2.MaximumCutDepth = useCase.MaxCutDepth;
          //      constraints2.Pipeline = useCase.Pipeline;
          //      constraints2.RowsDirection = Morph.Services.Core.DataModel.Utils.NormalizeAngleRad(-useCase.FurrowHeading * (Math.PI / 180.0) + Math.PI / 2.0, -1.0 * Math.PI);
          //      constraints2.Resolution = useCase.Resolution;
          //      constraints2.Shrinkage = useCase.Shrinkage;
          //      constraints2.Bulkage = useCase.Bulkage;
          //      constraints2.ExportVolume = useCase.ExportVolume;
          //      constraints2.ExclusionZones.AddRange((IEnumerable<Linestring>)useCase.ExclusionZones);
          //      design = instance.ComputeRows(constraints2, (Predicate<float>)null);
          //      break;
          //    case ComputeEnum.Subzones:
          //      ZonesConstraints constraints3 = new ZonesConstraints();
          //      constraints3.Boundary = useCase.Boundary;
          //      constraints3.MainDirection = Morph.Services.Core.DataModel.Utils.NormalizeAngleRad(-useCase.MainHeading * (Math.PI / 180.0) + Math.PI / 2.0, -1.0 * Math.PI);
          //      constraints3.Resolution = useCase.Resolution;
          //      constraints3.ExclusionZones.AddRange((IEnumerable<Linestring>)useCase.ExclusionZones);
          //      constraints3.Subzones.AddRange((IEnumerable<SubzoneConstraints>)useCase.Zones);
          //      foreach (SubzoneConstraints zone in useCase.Zones)
          //      {
          //        zone.Shrinkage = useCase.Shrinkage;
          //        zone.Bulkage = useCase.Bulkage;
          //      }
          //      for (int index = 0; index < constraints3.Subzones.Count; ++index)
          //      {
          //        if (string.IsNullOrEmpty(constraints3.Subzones[index].Tag))
          //          constraints3.Subzones[index].Tag = string.Format("Subzone{0:00}", (object)(index + 1));
          //      }
          //      design = instance.ComputeZones(constraints3, (Predicate<float>)null);
          //      break;
          //    case ComputeEnum.Basins:
          //      BasinConstraints constraints4 = new BasinConstraints();
          //      constraints4.BasinBoundary = useCase.Boundary;
          //      constraints4.ExitPoint = useCase.ExitPoint;
          //      constraints4.Resolution = useCase.Resolution;
          //      constraints4.MinimumSlope = useCase.MinSlope;
          //      constraints4.MaximumSlope = useCase.MaxSlope;
          //      constraints4.ExportVolume = useCase.ExportVolume;
          //      constraints4.ExclusionZones.AddRange((IEnumerable<Linestring>)useCase.ExclusionZones);
          //      constraints4.Shrinkage = useCase.Shrinkage;
          //      constraints4.Bulkage = useCase.Bulkage;
          //      design = instance.ComputeBasin(constraints4, (Predicate<float>)null);
          //      break;
        }
      }

      //  string str = Path.ChangeExtension(Path.Combine(Path.GetDirectoryName(Path.GetFullPath(commandLineArgument)), Path.GetFileNameWithoutExtension(commandLineArgument)), "skp");
      //  Program.CreateSketchupFile(str, surfaceInfo, design, useCase);
      //  Morph.Services.Core.DataModel.Utils.LaunchSketchup(str);
    }

    public static IDictionary<string, string> ParseCommandLineArguments(string[] args)
    {
      SortedList<string, string> sortedList = new SortedList<string, string>(args.Length);
      for (int index1 = 0; index1 < args.Length; ++index1)
      {
        if (args[index1].StartsWith("/") || args[index1].StartsWith("-"))
        {
          string str1 = args[index1].Substring(1);
          if (str1.Length > 1)
          {
            string index2 = str1;
            string str2 = string.Empty;
            int length = str1.IndexOf(':');
            if (length > 0)
            {
              index2 = str1.Substring(0, length);
              str2 = str1.Substring(length + 1);
            }
            sortedList[index2] = str2;
          }
        }
        else
          sortedList["input"] = args[index1];
      }
      return (IDictionary<string, string>)sortedList;
    }

    private static void CreateApp()
    {
      //new Application
      //{
      //  System.Windows.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown
      //}.Run();
    }
  }
}
