﻿using MasterDataProxies.ResultHandling;
using Newtonsoft.Json;
using VSS.Raptor.Service.WebApiModels.Compaction.Models.Palettes;

namespace VSS.Raptor.Service.WebApiModels.Compaction.ResultHandling
{
  /// <summary>
  /// Represents color palette result for a elevation palette request
  /// </summary>
  public class CompactionDetailPaletteResult : ContractExecutionResult
  {
    /// <summary>
    /// The palette for displaying elevation values.
    /// </summary>
    [JsonProperty(PropertyName = "palette", Required = Required.Default)]
    public DetailPalette Palette { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    /// 
    private CompactionDetailPaletteResult()
    {
      // ...
    }

    /// <summary>
    /// Creates an instance of the CompactionDetailPaletteResult class.
    /// </summary>
    /// <returns>An instance of the CompactionDetailPaletteResult class.</returns> 
    public static CompactionDetailPaletteResult CreateCompactionDetailPaletteResult(
      DetailPalette elevationPalette)
    {
      return new CompactionDetailPaletteResult
      {
        Palette = elevationPalette
      };
    }
  }
}
