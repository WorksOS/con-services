﻿using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Productivity3D.Models;

namespace VSS.Productivity3D.Models.Models
{
  /// <summary>
  /// The representation of a summary volumes request
  /// </summary>
  public class SummaryVolumesDataRequest : ProjectID
  {
    /// <summary>
    /// The base or earliest filter to be used for filter-filter and filter-design volumes.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult BaseFilter { get; private set; }

    /// <summary>
    /// The top or latest filter to be used for filter-filter and design-filter volumes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult TopFilter { get; private set; }

    /// <summary>
    /// An additional spatial constraining filter that may be used to provide additional control over the area the summary volumes are being calculated over.
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public FilterResult AdditionalSpatialFilter { get; private set; }

    /// <summary>
    /// The type of volume computation to be performed as a summary volumes request
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public VolumesType VolumeCalcType { get; private set; }
    
    /// <summary>
    /// The unique identifier of the design surface to be used as the base or earliest surface for design-filter volumes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid? BaseDesignUid { get; private set; }

    /// <summary>
    /// The offset for the base design if it is a reference surface
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double? BaseDesignOffset { get; private set; }

    /// <summary>
    /// The unique identifier of the design surface to be used as the top or latest surface for filter-design volumes
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public Guid? TopDesignUid { get; private set; }

    /// <summary>
    /// The offset for the top design if it is a reference surface
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public double? TopDesignOffset { get; private set; }

    /// <summary>
    /// Sets the cut tolerance to calculate Summary Volumes in meters
    /// </summary>
    /// <value>
    /// The cut tolerance.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? CutTolerance { get; private set; }

    /// <summary>
    /// Sets the fill tolerance to calculate Summary Volumes in meters
    /// </summary>
    /// <value>
    /// The cut tolerance.
    /// </value>
    [JsonProperty(Required = Required.Default)]
    public double? FillTolerance { get; private set; }

    /// <summary>
    /// Prevents a default instance of the <see cref="SummaryVolumesDataRequest"/> class from being created.
    /// </summary>
    private SummaryVolumesDataRequest()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    public SummaryVolumesDataRequest(
      Guid? projectUid,
      FilterResult baseFilter,
      FilterResult topFilter,
      Guid? baseDesignUid,
      double? baseDesignOffset,
      Guid? topDesignUid,
      double? topDesignOffset,
      VolumesType volumeCalcType)
    {
      ProjectUid = projectUid;
      BaseFilter = baseFilter;
      TopFilter = topFilter;
      BaseDesignUid = baseDesignUid;
      BaseDesignOffset = baseDesignOffset;
      TopDesignUid = topDesignUid;
      TopDesignOffset = topDesignOffset;
      VolumeCalcType = volumeCalcType;
    }

    /// <summary>
    /// Validates all properties.
    /// </summary>
    public override void Validate()
    {
      AdditionalSpatialFilter?.Validate();
      TopFilter?.Validate();
      BaseFilter?.Validate();
    }
  }
}
