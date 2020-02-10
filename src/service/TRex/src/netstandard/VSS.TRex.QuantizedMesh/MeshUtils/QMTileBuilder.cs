using System;
using System.IO;
using Microsoft.Extensions.Logging;
using VSS.TRex.QuantizedMesh.MeshUtils;
using VSS.TRex.QuantizedMesh.Models;
using VSS.TRex.Types;

namespace VSS.TRex.QuantizedMesh.GridFabric
{
  public class QMTileBuilder
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<QMTileBuilder>();

    public RequestErrorStatus BuildTileFaultCode = RequestErrorStatus.Unknown;

    public int GridSize { get; set; }

    public bool CompressTile = true; 

    public byte[] QuantizedMeshTile = null;

    public ElevationData TileData;

    /// <summary>
    /// Aborted is a flag that allows an external agency to abort the process
    /// </summary>
    public bool Aborted = false;

    public QMTileBuilder()
    {
    }


    /// <summary>
    /// Compute all header info
    /// </summary>
    /// <returns></returns>
    private void ComputeHeaderInfo()
    {

      // Work out bounding sphere for Cesium tile
      TileInfo tileInfo = new TileInfo();
      var tileHeader = tileInfo.CalculateHeaderInfo(ref TileData.EcefPoints);
      TileData.CenterX = tileHeader.CenterX;
      TileData.CenterY = tileHeader.CenterY;
      TileData.CenterZ = tileHeader.CenterZ;
      TileData.BoundingSphereCenterX = tileHeader.CenterX;
      TileData.BoundingSphereCenterY = tileHeader.CenterY;
      TileData.BoundingSphereCenterZ = tileHeader.CenterZ;
      TileData.BoundingSphereRadius = tileHeader.BoundingSphereRadius;

      // Work out tile HorizonOcclusionPoint
      var hop = HorizonOcclusionPoint.FromPoints(ref TileData.EcefPoints, tileInfo.BoundingSphere);
      TileData.HorizonOcclusionPointX = hop.X;
      TileData.HorizonOcclusionPointY = hop.Y;
      TileData.HorizonOcclusionPointZ = hop.Z;

      // If lighting is required add code here
    }


    /// <summary>
    /// Generate quantized mesh tile from the supplied grid
    /// </summary>
    /// <returns></returns>
    public bool BuildQuantizedMeshTile()
    {
      try
      {

        ComputeHeaderInfo(); 

        // Turn grid into a quantized mesh
        var vertices = MeshBuilder.MakeQuantizedMesh(ref TileData);

        var tileHeader = new TerrainTileHeader()
        {
          MaximumHeight = TileData.MaximumHeight,
          MinimumHeight = TileData.MinimumHeight,
          CenterX = TileData.CenterX,
          CenterY = TileData.CenterY,
          CenterZ = TileData.CenterZ,
          BoundingSphereCenterX = TileData.BoundingSphereCenterX,
          BoundingSphereCenterY = TileData.BoundingSphereCenterY,
          BoundingSphereCenterZ = TileData.BoundingSphereCenterZ,
          BoundingSphereRadius = TileData.BoundingSphereRadius,
          HorizonOcclusionPointX = TileData.HorizonOcclusionPointX,
          HorizonOcclusionPointY = TileData.HorizonOcclusionPointY,
          HorizonOcclusionPointZ = TileData.HorizonOcclusionPointZ
        };

        // This class constructs a tile from the computed mesh
        var tileBuilder = new TileBuilder();
        var tile = tileBuilder.MakeTile(vertices, ref TileData.VertexNormals, tileHeader, MapUtils.GridSizeToTriangleCount(TileData.GridSize), TileData.GridSize, TileData.HasLighting);
        QuantizedMeshTile = CompressTile ? MapUtils.Compress(tile) : tile;

        BuildTileFaultCode = RequestErrorStatus.OK;
      }
      catch (Exception E)
      {
        Log.LogError(E, "BuildQuantizedMeshTile: Exception:");
        return false;
      }
      return true;
    }


  }
}
