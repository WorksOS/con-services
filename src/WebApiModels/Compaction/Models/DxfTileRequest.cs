﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using src.Models;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;

namespace WebApiModels.Compaction.Models
{
  /// <summary>
  /// The request representation for getting a DXF tile from TCC
  /// </summary>
  public class DxfTileRequest : IValidatable
  {
    /// <summary>
    /// The files for which to retrieve the tiles.
    /// </summary>
    [Required]
    public IEnumerable<FileData> files { get; private set; }

    /// <summary>
    /// The bounding box of the tile
    /// </summary>
    [Required]
    public BoundingBox2DLatLon bbox { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private DxfTileRequest()
    {
    }

    /// <summary>
    /// Create instance of CompactionTileRequest
    /// </summary>
    public static DxfTileRequest CreateTileRequest(
      IEnumerable<FileData> files,
      BoundingBox2DLatLon boundingBoxLatLon
    )
    {
      return new DxfTileRequest
      {
          files = files,
          bbox = boundingBoxLatLon
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public void Validate()
    {
    }
  }
}
