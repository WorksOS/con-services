﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Microsoft.Practices.ServiceLocation;
using Morph.Services.Core.DataModel;
using Morph.Services.Core.Interfaces;
using Morph.Services.Core.Tools;
using SkuTester.DataModel;
using Trimble.Vce.Data.Skp;
using Trimble.Vce.Data.Skp.SkpLib;

namespace DrainageTest
{
  internal class SketchupFile
  {
    private readonly ILogger _logger = ServiceLocator.Current.GetInstance<ILogger>();
    private readonly TestCase _useCase;
    private readonly ISurfaceInfo _surfaceInfo;
    private readonly Design _design;

    public SketchupFile(TestCase useCase, ISurfaceInfo surfaceInfo, Design design)
    {
      _useCase = useCase;
      _surfaceInfo = surfaceInfo;
      _design = design;
    }

    protected internal string CreateModel(string commandLineArgument)
    {
      if (_useCase == null || _surfaceInfo == null // || _design == null
          || string.IsNullOrEmpty(commandLineArgument)
          || string.IsNullOrEmpty(Path.GetFullPath(commandLineArgument))
          || string.IsNullOrEmpty(Path.GetDirectoryName(Path.GetFullPath(commandLineArgument)))
      )
        return null;

      var targetPath =
        Path.ChangeExtension(
          Path.Combine(Path.GetDirectoryName(Path.GetFullPath(commandLineArgument)),
            Path.GetFileNameWithoutExtension(commandLineArgument)), "skp");
      CreateLayers(targetPath);

      _logger.LogInfo(nameof(CreateModel), $"targetPath: {targetPath}");
      return targetPath;
    }

    /// <summary>
    /// Creates the .skp model which sketchup launches with
    ///    also generates pngs.
    /// </summary>
    private void CreateLayers(string skuFile)
    {
      using (var skuModel = new SkuModel(_useCase.IsMetric))
      {
        skuModel.Name = Path.GetFileNameWithoutExtension(skuFile);
        _logger.LogInfo(nameof(CreateLayers), $"Name: {skuModel.Name}");

        AddOriginalLayers(skuFile, skuModel);

        if (_design != null)
          AddProposedLayers(skuFile, skuModel);

        skuModel.ZoomToExtents();
        skuModel.Save(skuFile, ModelVersion.SU2015);
      }
    }

    private void AddOriginalLayers(string skuFile, SkuModel skuModel)
    {
      _logger.LogInfo(nameof(AddOriginalLayers),
        $"originalVisualization: {_useCase.OriginalVisualizationTools.Length}");

      // generate original horizontal ground with first OriginalVisualizationTools (so order of these significant to result)
      // generates TestCase-original-<>-slope.png for each Original configuration in txt file
      if (_useCase.OriginalVisualizationTools != null)
      {
        foreach (var visualizationTool in _useCase.OriginalVisualizationTools)
        {
          var andSaveTexture = visualizationTool.GenerateAndSaveTexture(_surfaceInfo, skuFile, "original");
          var originalLayerName = "originalSurface " + visualizationTool.GetType();

          skuModel.AddSurfaceWithHorizontalTexture(_surfaceInfo.Points, _surfaceInfo.Triangles, originalLayerName, andSaveTexture, 0.75, originalLayerName, null);
        }
      }
    }

    private void AddProposedLayers(string skuFile, SkuModel skuModel)
    {
      _logger.LogInfo(nameof(AddProposedLayers),
        $"designVisualization: {_useCase.DesignVisualizationTools?.Length} designFitType: {_useCase.Compute}");
      
      // generate design horizontal ground with first DesignVisualizationTools (so order of these significant to result)
      // generates TestCase-design-<>-slope.png for each Design configuration in txt file
      if (_useCase.DesignVisualizationTools != null)
      { 
        foreach (var visualizationTool in _useCase.DesignVisualizationTools)
        {
          var andSaveTexture = visualizationTool.GenerateAndSaveTexture(_design.Surface, skuFile, "proposed");
          var proposedLayerName = "proposedSurface " + visualizationTool.GetType();

          skuModel.AddSurfaceWithHorizontalTexture(_design.Surface.Points, _design.Surface.Triangles, proposedLayerName, andSaveTexture, 0.75, proposedLayerName, null);
        }
      }
    }

    //private void AddOriginalLayers(string skuFile, SkuModel skuModel)
    //{
    //  _logger.LogInfo(nameof(AddOriginalLayers),
    //    $"originalVisualization: {_useCase.OriginalVisualizationTools.Length}");

    //  // generate original horizontal ground with first OriginalVisualizationTools (so order of these significant to result)
    //  // generates TestCase-original-<>-slope.png for each Original configuration in txt file
    //  var originalHorizontalTexture = (BitmapSource)null;
    //  var visualizationType = "unknown";
    //  if (_useCase.OriginalVisualizationTools != null)
    //  {
    //    foreach (var visualizationTool in _useCase.OriginalVisualizationTools)
    //    {
    //      var andSaveTexture = visualizationTool.GenerateAndSaveTexture(_surfaceInfo, skuFile, "original");
    //      if (originalHorizontalTexture == null && andSaveTexture != null)
    //      {
    //        visualizationType = visualizationTool.GetType().ToString();
    //        originalHorizontalTexture = andSaveTexture;
    //      }
    //    }
    //  }
    //  var originalLayerName = "originalSurface " + visualizationType;
    //  if (originalHorizontalTexture != null)
    //    skuModel.AddSurfaceWithHorizontalTexture(_surfaceInfo.Points, _surfaceInfo.Triangles, originalLayerName, originalHorizontalTexture, 0.75, originalLayerName, null);
    //  else
    //    skuModel.AddSurface(_surfaceInfo.Points, _surfaceInfo.Triangles, originalLayerName, "BurlyWood", 0.75, originalLayerName, null);

    //  // add surface flowLines
    //  if (_useCase.Compute == ComputeEnum.SurfaceBestFit)
    //  {
    //    var flowSegments = _surfaceInfo.GenerateFlowSegments(_design.CellSize, out _,
    //      null, null, new CancellationToken());
    //    skuModel.AddLinestrings(flowSegments.Select(
    //        fs => new Point3D[2] { fs.Point1, fs.Point2 }),
    //      "original flows", "brown", 0.75, false, "");
    //  }
    //}

    //private void AddProposedLayers(string skuFile, SkuModel skuModel)
    //{
    //  _logger.LogInfo(nameof(AddProposedLayers),
    //    $"designVisualization: {_useCase.DesignVisualizationTools?.Length} designFitType: {_useCase.Compute}");

    //  // generate design horizontal ground with first DesignVisualizationTools (so order of these significant to result)
    //  // generates TestCase-design-<>-slope.png for each Design configuration in txt file
    //  var designHorizontalTexture = (BitmapSource)null;
    //  var visualizationType = "unknown";
    //  if (_useCase.DesignVisualizationTools != null)
    //  {

    //    foreach (var visualizationTool in _useCase.DesignVisualizationTools)
    //    {
    //      // if (visualizationTool is SkuTester.DataModel.PondMap)
    //      if (visualizationTool is SkuTester.DataModel.DrainageViolations)
    //      {
    //        var andSaveTexture = visualizationTool.GenerateAndSaveTexture(_design.Surface, skuFile, "proposed");
    //        if (designHorizontalTexture == null && andSaveTexture != null)
    //        {
    //          visualizationType = visualizationTool.GetType().ToString();
    //          designHorizontalTexture = andSaveTexture;
    //        }
    //      }
    //    }
    //  }

    //  var proposedLayerName = "proposedHorizontalSurface " + visualizationType;
    //  if (designHorizontalTexture != null)
    //    skuModel.AddSurfaceWithHorizontalTexture(_design.Surface.Points, _design.Surface.Triangles, proposedLayerName,
    //      designHorizontalTexture, 0.75, proposedLayerName, null);
    //  else
    //    skuModel.AddSurface(_design.Surface.Points, _design.Surface.Triangles, proposedLayerName, "Green",
    //      0.75, proposedLayerName, null);

    //  /*
    //  // add design flowLines
    //  if (_useCase.Compute == ComputeEnum.SurfaceBestFit)
    //  {
    //    var flowSegments = _design.Surface.GenerateFlowSegments(_design.CellSize, out _,
    //      null, null, new CancellationToken());
    //    skuModel.AddLinestrings(flowSegments.Select(
    //      fs => new Point3D[2] {fs.Point1, fs.Point2}),
    //      "proposedSurfaceFlows", "DarkGreen", 0.75, false, "proposedSurfaceFlows");
    //  }

    //  // add vertical surface ie. cut-fill
    //  string fullPath = Path.GetFullPath(Path.Combine("Sketchup", "AltitudeTexture.png"));
    //  skuModel.AddSurfaceWithVerticalTexture(_design.CutFillSurface.Points, _design.CutFillSurface.Triangles, true, "proposedVerticalSurface", fullPath, 0.75, "proposedVerticalSurface", null);

    //  if (_useCase.TargetDitches != null && _useCase.TargetDitches.Any())
    //  {
    //    int num = 1;
    //    foreach (var targetDitch in _useCase.TargetDitches)
    //    {
    //      if (targetDitch.Points.Any())
    //        skuModel.AddLinestring(targetDitch.Points.Select(p => new Point3D(p.X, p.Y, 0.0)), $"ditch-{num++}", "Lime", 1.0, true, "");
    //    }
    //  }
    //  if (_useCase.Pipeline != null && _useCase.Pipeline.Points.Any())
    //    skuModel.AddLinestring(_useCase.Pipeline.Points.Select(p => new Point3D(p.X, p.Y, 0.0)), "pipeline", "Lime", 1.0, true, "");
    //  if (_design.Rows.Any())
    //    skuModel.AddLinestrings(_design.Rows.Select(ls => ls.Points), "Rows", "Green", 1.0, false, "Rows");
    //  if (_design.Columns.Any())
    //    skuModel.AddLinestrings(_design.Columns.Select(ls => ls.Points), "Columns", "Green", 1.0, false, "Columns");
    //  foreach (var plane in _design.Planes)
    //    skuModel.AddPlane(plane.Boundary.Points, plane.Tag, "Red", 0.75, "");
    //  */
    //}

  }
}

