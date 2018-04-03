using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Interfaces;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Utilities;
using NodaTime;
using NodaTime.Extensions;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Defines all the filter parameters that may be supplied and validates them.
  /// </summary>
  public class Filter : IValidatable, IEquatable<Filter>
  {
    /// <summary>
    /// The 'start' time for a time based filter. Data recorded earlier to this time is not considered.
    /// Optional. If not present then there is no start time bound.
    /// </summary>
    [JsonProperty(PropertyName = "startUtc", Required = Required.Default)]
    public DateTime? StartUtc { get; private set; }

    /// <summary>
    /// The 'end' time for a time based filter. Data recorded after this time is not considered.
    /// Optional. If not present there is no end time bound.
    /// </summary>
    [JsonProperty(PropertyName = "endUtc", Required = Required.Default)]
    public DateTime? EndUtc { get; private set; }

    /// <summary>
    /// Gets the date range type for this filter, e.g. day, week, project extents.
    /// </summary>
    [JsonProperty(PropertyName = "dateRangeType", Required = Required.Default)]
    public DateRangeType? DateRangeType { get; private set; }

    /// <summary>
    /// Gets the date range name for this filter, e.g. Today, Yesterday, ProjectExtents.
    /// </summary>
    [JsonProperty(PropertyName = "dateRangeName")]
    public string DateRangeName => this.DateRangeType != null ? Enum.GetName(typeof(DateRangeType), this.DateRangeType) : string.Empty;

    /// <summary>
    /// A design file unique identifier. Used as a spatial filter.
    /// </summary>
    [JsonProperty(PropertyName = "designUid", Required = Required.Default)]
    public string DesignUid { get; protected set; }

    /// <summary>
    /// A comma-separated list of contributing machines.
    /// Optional as it is not used in the proper functioning of a filter.
    /// </summary>
    [JsonProperty(PropertyName = "contributingMachines", Required = Required.Default)]
    public List<MachineDetails> ContributingMachines { get; private set; }

    /// <summary>
    /// A machine reported design. Cell passes recorded when a machine did not have this design loaded at the time is not considered.
    /// May be null/empty, which indicates no restriction. 
    /// </summary>
    [JsonProperty(PropertyName = "onMachineDesignId", Required = Required.Default)]
    public long? OnMachineDesignId { get; private set; } //PDS not VL ID

    /// <summary>
    /// Controls the cell pass from which to determine data based on its elevation.
    /// </summary>
    [JsonProperty(PropertyName = "elevationType", Required = Required.Default)]
    public ElevationType? ElevationType { get; private set; }

    /// <summary>
    /// Only filter cell passes recorded when the vibratory drum was 'on'.  If set to null, returns all cell passes.  If true, returns only cell passes with the cell pass parameter and the drum was on.  If false, returns only cell passes with the cell pass parameter and the drum was off.
    /// </summary>
    [JsonProperty(PropertyName = "vibeStateOn", Required = Required.Default)]
    public bool? VibeStateOn { get; private set; }

    /// <summary>
    /// The boundary/geofence unique identifier. Used as a spatial filter.
    /// </summary>
    [JsonProperty(PropertyName = "polygonUid", Required = Required.Default)]
    public string PolygonUid { get; protected set; }

    /// <summary>
    /// name of PolygonLL 
    /// </summary>
    [JsonProperty(PropertyName = "polygonName", Required = Required.Default)]
    public string PolygonName { get; private set; }

    /// <summary>
    /// A polygon to be used as a spatial filter boundary. The vertices are WGS84 positions
    /// </summary>
    [JsonProperty(PropertyName = "polygonLL", Required = Required.Default)]
    public List<WGSPoint> PolygonLL { get; private set; }

    /// <summary>
    /// Only use cell passes recorded when the machine was driving in the forwards direction. If true, only returns machines travelling forward, if false, returns machines travelling in reverse, if null, returns all machines.
    /// </summary>
    [JsonProperty(PropertyName = "forwardDirection", Required = Required.Default)]
    public bool? ForwardDirection { get; private set; }

    /// <summary>
    /// The number of the 3D spatial layer (determined through bench elevation and layer thickness or the tag file) to be used as the layer type filter. Layer 3 is then the third layer from the
    /// datum elevation where each layer has a thickness defined by the layerThickness member.
    /// </summary>
    [Range(ValidationConstants.MIN_LAYER_NUMBER, ValidationConstants.MAX_LAYER_NUMBER)]
    [JsonProperty(PropertyName = "layerNumber", Required = Required.Default)]
    public int? LayerNumber { get; private set; }

    /// <summary>
    /// The alignmentFile unique identifier. Used as a spatial filter along with station and offset.
    /// </summary>
    [JsonProperty(PropertyName = "alignmentUid", Required = Required.Default)]
    public string AlignmentUid { get; protected set; }

    /// <summary>
    /// The starting Station along the alignment.
    /// </summary>
    [JsonProperty(PropertyName = "startStation", Required = Required.Default)]
    public double? StartStation { get; protected set; }

    /// <summary>
    /// The ending Station along the alignment.
    /// </summary>
    [JsonProperty(PropertyName = "endStation", Required = Required.Default)]
    public double? EndStation { get; protected set; }

    /// <summary>
    /// The leftmost offset from the alignment.
    /// </summary>
    [JsonProperty(PropertyName = "leftOffset", Required = Required.Default)]
    public double? LeftOffset { get; protected set; }

    /// <summary>
    /// The leftmost offset from the alignment.
    /// </summary>
    [JsonProperty(PropertyName = "rightOffset", Required = Required.Default)]
    public double? RightOffset { get; protected set; }


    #region For JSON Serialization
    public bool ShouldSerializeStartUtc()
    {
      return StartUtc != null;
    }
    public bool ShouldSerializeEndUtc()
    {
      return EndUtc != null;
    }
    public bool ShouldSerializeDateRangeType()
    {
      return DateRangeType != null;
    }
    public bool ShouldSerializeDateRangeName()
    {
      return DateRangeType != null;
    }
    public bool ShouldSerializeDesignUid()
    {
      return DesignUid != null;
    }
    public bool ShouldSerializeContributingMachines()
    {
      return ContributingMachines != null;
    }
    public bool ShouldSerializeOnMachineDesignId()
    {
      return OnMachineDesignId != null;
    }
    public bool ShouldSerializeElevationType()
    {
      return ElevationType != null;
    }
    public bool ShouldSerializeVibeStateOn()
    {
      return VibeStateOn != null;
    }
    public bool ShouldSerializePolygonUid()
    {
      return PolygonUid != null;
    }
    public bool ShouldSerializePolygonName()
    {
      return PolygonName != null;
    }
    public bool ShouldSerializePolygonLL()
    {
      return PolygonLL != null;
    }
    public bool ShouldSerializeForwardDirection()
    {
      return ForwardDirection != null;
    }
    public bool ShouldSerializeLayerNumber()
    {
      return LayerNumber != null;
    }
    public bool ShouldSerializeAlignmentUid()
    {
      return AlignmentUid != null;
    }
    public bool ShouldSerializeStartStation()
    {
      return StartStation != null;
    }
    public bool ShouldSerializeEndStation()
    {
      return EndStation != null;
    }
    public bool ShouldSerializeLeftOffset()
    {
      return LeftOffset != null;
    }
    public bool ShouldSerializeRightOffset()
    {
      return RightOffset != null;
    }
    #endregion

    public bool HasData() =>
      StartUtc.HasValue ||
      EndUtc.HasValue ||
      DateRangeType.HasValue ||
      OnMachineDesignId.HasValue ||
      VibeStateOn.HasValue ||
      ElevationType.HasValue ||
      LayerNumber.HasValue ||
      ForwardDirection.HasValue ||
      (ContributingMachines != null && ContributingMachines.Count > 0) ||
      (PolygonLL != null && PolygonLL.Count > 0) ||
      !string.IsNullOrEmpty(AlignmentUid) ||
      StartStation.HasValue ||
      EndStation.HasValue ||
      LeftOffset.HasValue ||
      RightOffset.HasValue;

    public void AddBoundary(string polygonUID, string polygonName, List<WGSPoint> polygonLL)
    {
      this.PolygonUid = polygonUID;
      this.PolygonName = polygonName;
      this.PolygonLL = polygonLL;
    }

    /// <summary>
    /// Create instance of Filter
    /// </summary>
    public static Filter CreateFilter
      (
        DateTime? startUtc,
        DateTime? endUtc,
        string designUid,
        List<MachineDetails> contributingMachines,
        long? onMachineDesignId,
        ElevationType? elevationType,
        bool? vibeStateOn,
        List<WGSPoint> polygonLL,
        bool? forwardDirection,
        int? layerNumber,
        string polygonUid = null,
        string polygonName = null,
        string alignmentUid = null,
        double? startStation = null,
        double? endStation = null,
        double? leftOffset = null,
        double? rightOffset = null
      )
    {
      return new Filter
      {
        StartUtc = startUtc,
        EndUtc = endUtc,
        DesignUid = designUid,
        ContributingMachines = contributingMachines,
        OnMachineDesignId = onMachineDesignId,
        ElevationType = elevationType,
        VibeStateOn = vibeStateOn,
        PolygonLL = polygonLL,
        ForwardDirection = forwardDirection,
        LayerNumber = layerNumber,
        PolygonUid = polygonUid,
        PolygonName = polygonName,
        AlignmentUid = alignmentUid,
        StartStation = startStation,
        EndStation = endStation,
        LeftOffset = leftOffset,
        RightOffset = rightOffset
      };
    }

    public string ToJsonString()
    {
      var filter = CreateFilter(StartUtc, EndUtc, DesignUid, ContributingMachines, OnMachineDesignId, ElevationType, VibeStateOn, PolygonLL, ForwardDirection, LayerNumber, 
        PolygonUid, PolygonName, 
        AlignmentUid, StartStation, EndStation, LeftOffset, RightOffset);

      return JsonConvert.SerializeObject(filter);
    }

    public void Validate([FromServices] IServiceExceptionHandler serviceExceptionHandler)
    {
      //Check date range properties
      if (StartUtc.HasValue || EndUtc.HasValue)
      {
        if (StartUtc.HasValue && EndUtc.HasValue)
        {
          if (StartUtc.Value > EndUtc.Value)
          {
            serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 29);
          }
        }
        else
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 30);
        }
      }

      if (DesignUid != null && Guid.TryParse(DesignUid, out Guid _) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 31);
      }

      if (ContributingMachines != null)
      {
        foreach (var machine in ContributingMachines)
        {
          machine.Validate();
        }
      }

      //Check boundary if provided
      //Raptor handles any weird boundary you give it and automatically closes it if not closed already therefore we just need to check we have at least 3 points
      if (PolygonLL != null && PolygonLL.Count < 3)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 35);
      }

      // check alignment
      if (AlignmentUid != null && Guid.TryParse(AlignmentUid, out Guid _) == false)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 64); 
      }

      // must have both or neither; must increase 
      if (StartStation.HasValue != EndStation.HasValue)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 65);
      }

      if (EndStation.HasValue && StartStation.HasValue && StartStation.Value >= EndStation.Value)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 65);
      }

      // must have both or neither; 
      // offsets if positive apply to their side of the centerline
      // Negative offsets allow the user to indicate a slice on one side of the centreline
      //   e.g. LeftOffset = -20 and RightOffset = 25
      //      will result in the strip on the right side of the road between 20 and 25
      if (LeftOffset.HasValue != RightOffset.HasValue)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 66);
      }

      // if any present then ALL must exist.
      if ((StartStation.HasValue != LeftOffset.HasValue != string.IsNullOrEmpty(AlignmentUid))
          && StartStation.HasValue)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 67);
      }

    }

    public bool Equals(Filter other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return StartUtc.Equals(other.StartUtc) && EndUtc.Equals(other.EndUtc) && DateRangeType == other.DateRangeType &&
             string.Equals(DesignUid, other.DesignUid) && ContributingMachines.ScrambledEquals(other.ContributingMachines) &&
             OnMachineDesignId == other.OnMachineDesignId && ElevationType == other.ElevationType &&
             VibeStateOn == other.VibeStateOn && string.Equals(PolygonUid, other.PolygonUid) &&
             string.Equals(PolygonName, other.PolygonName) && PolygonLL.ScrambledEquals(other.PolygonLL) &&
             ForwardDirection == other.ForwardDirection && LayerNumber == other.LayerNumber && 
             string.Equals(AlignmentUid, other.AlignmentUid) && string.Equals(StartStation, other.StartStation) && string.Equals(EndStation, other.EndStation) &&
             string.Equals(LeftOffset, other.LeftOffset) && string.Equals(RightOffset, other.RightOffset);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((Filter)obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = StartUtc.GetHashCode();
        hashCode = (hashCode * 397) ^ EndUtc.GetHashCode();
        hashCode = (hashCode * 397) ^ (DateRangeType != null ? DateRangeType.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (DesignUid != null ? DesignUid.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (ContributingMachines != null ? ContributingMachines.GetListHashCode() : 397);
        hashCode = (hashCode * 397) ^ (OnMachineDesignId != null ? OnMachineDesignId.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (ElevationType !=null ? ElevationType.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (VibeStateOn !=null ? VibeStateOn.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (PolygonUid != null ? PolygonUid.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (PolygonName != null ? PolygonName.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (PolygonLL != null ? PolygonLL.GetListHashCode() : 397);
        hashCode = (hashCode * 397) ^ (ForwardDirection !=null ? ForwardDirection.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (LayerNumber!=null ? LayerNumber.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (AlignmentUid != null ? AlignmentUid.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (StartStation != null ? StartStation.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (EndStation != null ? EndStation.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (LeftOffset != null ? LeftOffset.GetHashCode() : 397);
        hashCode = (hashCode * 397) ^ (RightOffset != null ? RightOffset.GetHashCode() : 397);
        return hashCode;
      }
    }

    public static bool operator ==(Filter left, Filter right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(Filter left, Filter right)
    {
      return !Equals(left, right);
    }

    /// <summary>
    /// Apply the date range type to the start and end UTC.
    /// </summary>
    /// <param name="ianaTimeZoneName">The project time zone to use (IANA name)</param>
    /// <param name="useEndOfCurrentDay">True if the current date range types should use the end of the day rather than now for the end of the period.
    /// The filter service uses 'now' as the value is returned to the client and displayed in the UI. The 3dpm service uses the end of the day to pass 
    /// to Raptor so that Raptor's cache works properly.</param>
    public void ApplyDateRange(string ianaTimeZoneName, bool useEndOfCurrentDay=false)
    {
      if (!string.IsNullOrEmpty(ianaTimeZoneName) &&
          DateRangeType != null &&
          DateRangeType != Internal.DateRangeType.Custom)
      {
        // Force date range filters to be null if ProjectExtents is specified.
        if (DateRangeType == Internal.DateRangeType.ProjectExtents)
        {
          StartUtc = null;
          EndUtc = null;
        }
        else
        {
          StartUtc = DateRangeType?.UtcForDateRangeType(ianaTimeZoneName, true, useEndOfCurrentDay);
          EndUtc = DateRangeType?.UtcForDateRangeType(ianaTimeZoneName, false, useEndOfCurrentDay);
        }
      }
    }
  }
}