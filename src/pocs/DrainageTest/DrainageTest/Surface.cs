using Morph.Services.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Morph.Services.Core.DataModel;
using SkuTester.DataModel;

namespace DrainageTest
{
  internal class Surface
  {
    private readonly ILogger _logger = ServiceLocator.Current.GetInstance<ILogger>();
    private TestCase _useCase;
    private ISurfaceInfo _surfaceInfo;
    private Design _design;
    private bool _isGbg = false;

    public Surface(TestCase useCase)
    {
      _useCase = useCase;
    }

    protected internal ISurfaceInfo GenerateSurfaceInfo(ILandLeveling landLevelingInstance)
    {
      if (_useCase == null)
        return null;

      if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(_useCase.Surface), ".dxf") == 0)
        _surfaceInfo = landLevelingInstance.ImportSurface(_useCase.Surface, (Action<float>) null);

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
      //
      //else if (StringComparer.InvariantCultureIgnoreCase.Compare(Path.GetExtension(useCase.Surface), ".gbg") == 0)
      //{
      //  isGBG = true;
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

      _logger.LogInfo(nameof(GenerateSurfaceInfo), $"_surfaceInfo boundary count: {_surfaceInfo.Boundary.Count}");
      return _surfaceInfo;
    }

    protected internal Design ComputeSurfaces(ILandLeveling landLevelingInstance)
    {
      if (!_useCase.IsMetric)
        _useCase = _useCase.AsMetric();

      switch (_useCase.Compute)
      {
        case ComputeEnum.SurfaceBestFit:
          _logger.LogInfo(nameof(ComputeSurfaces),
            $"Compute SurfaceBestFit. surface Name: {_surfaceInfo.Name} pointCount: {_surfaceInfo.Points.Count}");
          var constraints1 = new SurfaceConstraints();
          constraints1.Resolution = _useCase.Resolution;
          constraints1.Boundary = _useCase.Boundary;
          constraints1.TargetDitches = _useCase.TargetDitches;
          constraints1.ExclusionZones.AddRange(_useCase.ExclusionZones);
          constraints1.Areas.AddRange(_useCase.Areas);
          if (!constraints1.Areas.Any())
            constraints1.Areas.Add(new AreaConstraints()
            {
              Tag = "Field",
              Boundary = _useCase.Boundary.Points.Count > 2
                ? _useCase.Boundary
                : new Linestring(_surfaceInfo.Boundary),
              MinimumSlope = _useCase.MinSlope,
              MaximumSlope = _useCase.MaxSlope,
              MaximumCutDepth = _useCase.MaxCutDepth,
              MaximumFillHeight = _useCase.MaxFillHeight,
              Shrinkage = _useCase.Shrinkage,
              Bulkage = _useCase.Bulkage,
              ExportVolume = _useCase.ExportVolume
            });
          foreach (var area in _useCase.Areas)
          {
            area.Shrinkage = _useCase.Shrinkage;
            area.Bulkage = _useCase.Bulkage;
          }

          for (int index = 0; index < constraints1.Areas.Count; ++index)
          {
            if (string.IsNullOrEmpty(constraints1.Areas[index].Tag))
              constraints1.Areas[index].Tag = $"Area{(object) (index + 1):00}";
          }

          _design = landLevelingInstance.ComputeSurface(constraints1, (Predicate<float>) null);

          break;

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
        //      else if (!isGBG)
        //      {
        //        foreach (PlanesConstraints section in useCase.Sections)
        //        {
        //          section.Shrinkage = useCase.Shrinkage;
        //          section.Bulkage = useCase.Bulkage;
        //        }
        //      }
        //      design = instance.ComputePlanes((IList<PlanesConstraints>)useCase.Sections, (IEnumerable<Linestring>)useCase.ExclusionZones, useCase.ExportVolume, (Predicate<float>)null);
        //      break;
        //
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
        // 
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
        //
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

      return _design;
    }

    /*
    private static IList<Point3D> ReadTxtPoints(string filename)
    {
      List<Point3D> point3DList = new List<Point3D>();
      using (StreamReader streamReader = new StreamReader(filename))
      {
        string str1;
        while ((str1 = streamReader.ReadLine()) != null)
        {
          if (!str1.StartsWith("#"))
          {
            string[] strArray = str1.Split(",; \t".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            string str2 = ((IEnumerable<string>) strArray).Count<string>() > 4 ? strArray[4] : string.Empty;
            if (strArray.Length > 3 && (str2 == string.Empty || str2 == "B"))
              point3DList.Add(new Point3D(double.Parse(strArray[1]), double.Parse(strArray[2]), double.Parse(strArray[3])));
          }
        }
      }
      return (IList<Point3D>) point3DList;
    }

     private static void SaveResult(
      string dataFolder,
      ISurfaceInfo surfaceInfo,
      Design design,
      Linestring target_ditch = null,
      Linestring pipeline = null)
    {
      Program.OutputTriangles(surfaceInfo.Points, surfaceInfo.Triangles, Path.Combine(dataFolder, "surface.txt"));
      if (design.Planes.Count > 0)
        Program.OutputPlanes(design, Path.Combine(dataFolder, "planes.txt"));
      else
        File.Delete(Path.Combine(dataFolder, "planes.txt"));
      if (design.Surface != null)
        Program.OutputTriangles(design.Surface.Points, design.Surface.Triangles, Path.Combine(dataFolder, "design.txt"));
      else
        File.Delete(Path.Combine(dataFolder, "design.txt"));
      if (design.CutFillSurface != null)
        Program.OutputTriangles(design.CutFillSurface.Points, design.CutFillSurface.Triangles, Path.Combine(dataFolder, "cutfill.txt"));
      else
        File.Delete(Path.Combine(dataFolder, "cutfill.txt"));
      if (design.Rows.Count > 0 || design.Columns.Count > 0)
        Program.OutputLines(design, Path.Combine(dataFolder, "rows.txt"));
      else
        File.Delete(Path.Combine(dataFolder, "rows.txt"));
      if (target_ditch != null)
        Program.OutputLinestring(target_ditch, Path.Combine(dataFolder, "ditch.txt"));
      else
        File.Delete(Path.Combine(dataFolder, "ditch.txt"));
      if (pipeline != null)
        Program.OutputLinestring(pipeline, Path.Combine(dataFolder, "pipeline.txt"));
      else
        File.Delete(Path.Combine(dataFolder, "pipeline.txt"));
    }

    private static void OutputLinestring(Linestring ls, string filePath)
    {
      using (StreamWriter streamWriter = new StreamWriter(filePath))
      {
        if (ls == null || ls.Points.Count <= 0)
          return;
        streamWriter.WriteLine("V:{0}", (object) ls.Points.Count);
        foreach (Point point in ls.Points)
          streamWriter.WriteLine("v:{0:F6},{1:F6},{2:F6};", (object) point.X, (object) point.Y, (object) 0);
      }
    }

    private static void OutputTriangles(IList<Point3D> points, IList<int> indices, string filePath)
    {
      using (StreamWriter streamWriter = new StreamWriter(filePath))
      {
        if (points == null || points.Count <= 0 || (indices == null || indices.Count <= 0))
          return;
        streamWriter.WriteLine("V:{0}", (object) points.Count);
        foreach (Point3D point in (IEnumerable<Point3D>) points)
          streamWriter.WriteLine("v:{0:F4},{1:F4},{2:F4};", (object) point.X, (object) point.Y, (object) point.Z);
        int num = indices.Count / 3;
        streamWriter.WriteLine("F:{0}", (object) num);
        for (int index = 0; index < num; ++index)
          streamWriter.WriteLine("f:{0},{1},{2};", (object) indices[index * 3], (object) indices[index * 3 + 1], (object) indices[index * 3 + 2]);
      }
    }

    private static void OutputPlanes(Design design, string filePath)
    {
      using (StreamWriter streamWriter = new StreamWriter(filePath))
      {
        foreach (Plane plane in (IEnumerable<Plane>) design.Planes)
        {
          streamWriter.Write("p: ");
          foreach (Point3D point in plane.Boundary.Points)
            streamWriter.Write("{0:F6},{1:F6},{2:F6} ", (object) point.X, (object) point.Y, (object) point.Z);
          streamWriter.WriteLine();
        }
      }
    }

    private static void OutputLines(Design design, string filePath)
    {
      using (StreamWriter streamWriter = new StreamWriter(filePath))
      {
        if (design.Pipeline != null && design.Pipeline.Points.Any<Point3D>())
        {
          streamWriter.Write("l: ");
          foreach (Point3D point in design.Pipeline.Points)
            streamWriter.Write("{0:F6},{1:F6},{2:F6} ", (object) point.X, (object) point.Y, (object) point.Z);
          streamWriter.WriteLine();
        }
        foreach (Linestring3D row in design.Rows)
        {
          if (row.Points.Any<Point3D>())
          {
            streamWriter.Write("l: ");
            foreach (Point3D point in row.Points)
              streamWriter.Write("{0:F6},{1:F6},{2:F6} ", (object) point.X, (object) point.Y, (object) point.Z);
            streamWriter.WriteLine();
          }
        }
        foreach (Linestring3D column in design.Columns)
        {
          if (column.Points.Any<Point3D>())
          {
            streamWriter.Write("l: ");
            foreach (Point3D point in column.Points)
              streamWriter.Write("{0:F6},{1:F6},{2:F6} ", (object) point.X, (object) point.Y, (object) point.Z);
            streamWriter.WriteLine();
          }
        }
      }
    }

        private static void SaveBitmap(BitmapSource bitmap, string filename)
    {
      PngBitmapEncoder pngBitmapEncoder = new PngBitmapEncoder();
      pngBitmapEncoder.Frames.Add(BitmapFrame.Create(bitmap));
      using (Stream stream = (Stream) File.Create(filename))
        pngBitmapEncoder.Save(stream);
    }
    */
  }
}

