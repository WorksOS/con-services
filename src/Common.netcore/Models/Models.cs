using System;
using System.Collections.Generic;

namespace LandfillService.Common.Models
{
  #region Web API Models

  /// <summary>
  ///   Project representation
  /// </summary>
  public class Project
  {
    public uint id { get; set; }
    public string name { get; set; }
    public string timeZoneName { get; set; } // project time zone name (NodaTime)
    public int? daysToSubscriptionExpiry { get; set; }
    public string projectUid { get; set; }
    public string legacyTimeZoneName { get; set; }
    public long legacyCustomerID { get; set; }
    public string customerUid { get; set; }
  }

  /// <summary>
  ///   GeofenceResponse representation
  /// </summary>
  public class GeofenceResponse
  {
    public Guid uid { get; set; }
    public string name { get; set; }
    public int type { get; set; }
    public BoundingBox bbox { get; set; }
  }

  /// <summary>
  ///   Bounding box representation
  /// </summary>
  public class BoundingBox
  {
    public double minLat { get; set; }
    public double minLng { get; set; }
    public double maxLat { get; set; }
    public double maxLng { get; set; }
  }

  /// <summary>
  ///   Date and geofence for entries with no volume
  /// </summary>
  public class DateEntry
  {
    public DateTime date { get; set; } // date of the entry; always in the project time zone
    public string geofenceUid { get; set; }

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of date entry</returns>
    public override string ToString()
    {
      return string.Format("date:{0}, geofence UID:{1}", date, geofenceUid);
    }
  }

  /// <summary>
  ///   Weight entry submitted by the user
  /// </summary>
  public class WeightEntry
  {
    public DateTime date { get; set; } // date of the entry; always in the project time zone
    public double weight { get; set; }

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of weight entry</returns>
    public override string ToString()
    {
      return string.Format("date:{0}, weight:{1}", date, weight);
    }
  }

  /// <summary>
  ///   Data entry for a given date - part of project data sent to the client
  /// </summary>
  public class DayEntry
  {
    public DateTime date { get; set; }
    public bool entryPresent { get; set; } // true if the entry has at least the weight value
    public double weight { get; set; }
    public double volume { get; set; }
  }

  /// <summary>
  ///   Weight entry for a geofence
  /// </summary>
  public class GeofenceWeight
  {
    public Guid geofenceUid { get; set; }
    public double weight { get; set; }

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of weight entry</returns>
    public override string ToString()
    {
      return string.Format("geofenceUid:{0}, weight:{1}", geofenceUid, weight);
    }
  }

  /// <summary>
  ///   Weight entry submitted by the user for a geofence
  /// </summary>
  public class GeofenceWeightEntry
  {
    public DateTime date { get; set; }
    public bool entryPresent { get; set; } // true if any site has a weight present
    public IEnumerable<GeofenceWeight> geofenceWeights { get; set; }
  }

  /// <summary>
  ///   Encapsulates weight data sent to the client
  /// </summary>
  public class WeightData
  {
    public IEnumerable<GeofenceWeightEntry> entries { get; set; }
    public bool retrievingVolumes { get; set; } // is the service currently retrieving volumes for this project?
    public Project Project { get; set; }
  }

  /// <summary>
  ///   Encapsulates project data sent to the client
  /// </summary>
  public class ProjectData
  {
    public IEnumerable<DayEntry> entries { get; set; }
    public bool retrievingVolumes { get; set; } // is the service currently retrieving volumes for this project?
    public Project Project { get; set; }
  }

  /// <summary>
  ///   A CCA ratio for a machine for one day (for all lifts)
  /// </summary>
  public class CCARatioEntry
  {
    public DateTime date { get; set; }
    public double ccaRatio { get; set; }
  }

  /// <summary>
  ///   CCA ratio representation for a machine
  /// </summary>
  public class CCARatioData
  {
    public string machineName { get; set; }
    public IEnumerable<CCARatioEntry> entries { get; set; }
  }

  /// <summary>
  ///   CCA summary representation for a machine
  /// </summary>
  public class CCASummaryData
  {
    public string machineName { get; set; }
    public int? liftId { get; set; }
    public double incomplete { get; set; }
    public double complete { get; set; }
    public double overcomplete { get; set; }
  }

  /// <summary>
  ///   Represents a CCA entry from the database
  /// </summary>
  public class CCA
  {
    public string geofenceUid { get; set; }
    public DateTime date { get; set; }
    public long machineId { get; set; }
    public int? liftId { get; set; }
    public double incomplete { get; set; }
    public double complete { get; set; }
    public double overcomplete { get; set; }
  }

  /// <summary>
  ///   Volume and time summary data
  /// </summary>
  public class VolumeTime
  {
    public double currentWeekVolume { get; set; }
    public double currentMonthVolume { get; set; }
    public double? remainingVolume { get; set; }
    public double? remainingTime { get; set; }
  }

  #endregion

  #region Raptor API Models

  /// <summary>
  ///   WGS point for volume summary requests sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class WGSPoint
  {
    public double Lat;
    public double Lon;

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of polygon filter params</returns>
    public override string ToString()
    {
      return string.Format("(Lat:{0}, Lon:{1})", Lat, Lon);
    }
  }

  /// <summary>
  ///   Filter for volume summary requests sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class VolumeFilter
  {
    public DateTime? endUTC;
    public List<WGSPoint> polygonLL;
    public bool returnEarliest;
    public DateTime? startUTC;

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of volume filter params</returns>
    public override string ToString()
    {
      var poly = string.Empty;
      if (polygonLL != null)
        foreach (var pt in polygonLL)
          poly = string.Format("{0}{1}", poly, pt);
      return string.Format("startUTC:{0}, endUTC:{1}, returnEarliest:{2}, polygonLL:{3}", startUTC, endUTC,
        returnEarliest, poly);
    }
  }

  /// <summary>
  ///   Volume calculation parameters sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class VolumeParams
  {
    public VolumeFilter baseFilter;
    public long projectId;
    public VolumeDesign topDesignDescriptor;
    public VolumeFilter topFilter;
    public int volumeCalcType;

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of volume request params</returns>
    public override string ToString()
    {
      return string.Format("projectId:{0}, volumeCalcType:{1}, baseFilter:{2}, topFilter:{3}, topDesignDescriptor:{4}",
        projectId, volumeCalcType, baseFilter, topFilter, topDesignDescriptor);
    }
  }

  /// <summary>
  ///   Volume calculation parameters sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class ProjectExtentsParams
  {
    public int[] excludedSurveyedSurfaceIds;
    public uint projectId;
  }

  /// <summary>
  ///   Design for volume summary requests sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class VolumeDesign
  {
    public DesignDescriptor file;
    public long id;
  }

  /// <summary>
  ///   Design descriptor for volume summary requests sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class DesignDescriptor
  {
    public string fileName;

    public string filespaceId;
    public string path;
  }

  /// <summary>
  ///   Statistics parameters sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class StatisticsParams
  {
    public long projectId;
  }

  /// <summary>
  ///   3D bounding box - returned in volume summary results from the Raptor API
  /// </summary>
  public class BoundingBox3DGrid
  {
    /// <summary>
    ///   Maximum X value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    public double maxX { get; set; }

    /// <summary>
    ///   Maximum Y value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    public double maxY { get; set; }

    /// <summary>
    ///   Maximum Z value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    public double maxZ { get; set; }

    /// <summary>
    ///   Minimum X value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    public double minX { get; set; }

    /// <summary>
    ///   Minimum Y value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    public double minY { get; set; }

    /// <summary>
    ///   Minimum Z value, in the cartesian grid coordinate system, expressed in meters
    /// </summary>
    public double minZ { get; set; }
  }

  public class DesignDescriptiorLegacy
  {
    public string name { get; set; }
    public string fileType { get; set; }
    public string insertUTC { get; set; }
    public int id { get; set; }
  }


  /// <summary>
  ///   Volume summary entry returned from the Raptor API
  /// </summary>
  public class SummaryVolumesResult
  {
    /// <summary>
    ///   Zone boundaries
    /// </summary>
    public BoundingBox3DGrid BoundingExtents { get; set; }

    /// <summary>
    ///   Cut volume in m3
    /// </summary>
    public double Cut { get; set; }

    /// <summary>
    ///   Fill volume in m3
    /// </summary>
    public double Fill { get; set; }

    /// <summary>
    ///   Cut area in m2
    /// </summary>
    public double CutArea { get; set; }

    /// <summary>
    ///   Fill area in m2
    /// </summary>
    public double FillArea { get; set; }

    /// <summary>
    ///   Total coverage area (cut + fill + no change) in m2.
    /// </summary>
    public double TotalCoverageArea { get; set; }

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of volume summary results</returns>
    public override string ToString()
    {
      return string.Format("cut:{0}, fill:{1}", Cut, Fill);
    }
  }


  /// <summary>
  ///   Project extents entry returned from the Raptor API
  /// </summary>
  public class ProjectExtentsResult
  {
    public DateTime startTime { get; set; }
    public DateTime endTime { get; set; }
  }

  /// <summary>
  ///   A representation of a set of spatial and temporal stastics for the project as a whole
  /// </summary>
  public class ProjectStatisticsResult
  {
    /// <summary>
    ///   Size of spatial data cells in the project (the default value is 34cm)
    /// </summary>
    public double cellSize;

    /// <summary>
    ///   Latest time stamped data present in the project, including both production and surveyed surface data.
    /// </summary>
    public DateTime endTime;

    /// <summary>
    ///   The three dimensional extents of the project including both production and surveyed surface data.
    /// </summary>
    public BoundingBox3DGrid extents;

    /// <summary>
    ///   The index origin offset from the absolute bottom left origin of the subgrid tree cartesian coordinate system to the
    ///   centered origin of the cartesian
    ///   grid coordinate system used in the project, and the centered origin cartesian coordinates of cell addresses.
    /// </summary>
    public int indexOriginOffset;

    /// <summary>
    ///   Earlist time stamped data present in the project, including both production and surveyed surface data.
    /// </summary>
    public DateTime startTime;

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>Formatted string representation of the project statistics information.</returns>
    public override string ToString()
    {
      return string.Format("Start time:{0}, end time:{1}, cellsize:{2}, indexOriginOffset:{3}, extents:{4}",
        startTime, endTime, cellSize, indexOriginOffset, extents);
    }
  }

  /// <summary>
  ///   Machine details sent to/received from the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class MachineDetails
  {
    public long assetId;
    public bool isJohnDoe;
    public string machineName;

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of machine details params</returns>
    public override string ToString()
    {
      return string.Format("({0},{1},{2})", assetId, machineName, isJohnDoe);
    }
  }

  /// <summary>
  ///   Filter for CCA summary requests sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class CCAFilter
  {
    public List<MachineDetails> contributingMachines;
    public DateTime endUTC;
    public int? layerNumber;
    public int? layerType;
    public List<WGSPoint> polygonLL;
    public DateTime startUTC;

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of CCA filter params</returns>
    public override string ToString()
    {
      var poly = string.Empty;
      if (polygonLL != null)
        foreach (var pt in polygonLL)
          poly = string.Format("{0}{1}", poly, pt);
      return string.Format("startUTC:{0}, endUTC:{1}, layerNumber:{2}, contributingMachines:{3}, polygonLL:{4}",
        startUTC, endUTC, layerNumber, contributingMachines, poly);
    }
  }

  /// <summary>
  ///   CCA summary parameters sent to the Raptor API; see Raptor API documentation for details
  /// </summary>
  public class CCASummaryParams
  {
    public CCAFilter filter;
    public LiftBuildSettings liftBuildSettings = new LiftBuildSettings();
    public long projectId;

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of volume request params</returns>
    public override string ToString()
    {
      return string.Format("projectId:{0}, filter:{1}", projectId, filter);
    }
  }

  public class LiftBuildSettings
  {
    public long liftDetectionType = 4;
  }

  /// <summary>
  ///   CCA summary entry returned from the Raptor API
  /// </summary>
  public class CCASummaryResult
  {
    /// <summary>
    ///   The percentage of the cells that are incomplete
    /// </summary>
    public double undercompletePercent { get; set; }

    /// <summary>
    ///   The percentage of cells that are complete within the target bounds
    /// </summary>
    public double completePercent { get; set; }

    /// <summary>
    ///   The percentage of the cells that are over-complete
    /// </summary>
    public double overcompletePercent { get; set; }

    /// <summary>
    ///   The internal result code of the request. Documented elsewhere.
    /// </summary>
    public short returnCode { get; set; }

    /// <summary>
    ///   The total area covered by non-null cells in the request area
    /// </summary>
    public double totalAreaCoveredSqMeters { get; set; }

    /// <summary>
    ///   ToString override
    /// </summary>
    /// <returns>A string representation of CCA summary results</returns>
    public override string ToString()
    {
      return string.Format("under:{0}, complete:{1}, over:{2}",
        undercompletePercent, completePercent, overcompletePercent);
    }
  }

  /// <summary>
  ///   List of machines and lifts returned from the Raptor API
  /// </summary>
  public class MachineLayerIdsExecutionResult
  {
    public MachineLiftDetails[] MachineLiftDetails { get; set; }
  }

  /// <summary>
  ///   Machine Lift/Layer Details returned from the Raptor API
  /// </summary>
  public class MachineLiftDetails : MachineDetails
  {
    public LiftDetails[] lifts { get; set; }
  }

  /// <summary>
  ///   Lift details returned from the Raptor API
  /// </summary>
  public class LiftDetails
  {
    public int layerId { get; set; }
    public DateTime endUtc { get; set; }
  }

  /// <summary>
  ///   Machine details with its lifts for the Web API
  /// </summary>
  public class MachineLifts : MachineDetails
  {
    public List<Lift> lifts { get; set; }
  }

  /// <summary>
  ///   Lift data for the Web API
  /// </summary>
  public class Lift
  {
    public int layerId { get; set; }
    public DateTime endTime { get; set; } //in project time zone
  }

  #endregion
}