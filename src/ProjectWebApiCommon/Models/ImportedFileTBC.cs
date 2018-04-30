using System;
using Newtonsoft.Json;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{

  public class SurfaceFile
  {
    /// <summary>
    /// Gets or sets the surveyed UTC date/time.
    /// </summary>
    [JsonProperty(PropertyName = "SurveyedUTC", Required = Required.Default)]
    public DateTime SurveyedUtc { get; set; }
  }

  public class LineworkFile
  {
    /// <summary>
    /// Gets or sets the distance units. Values are:
    /// 0 : Meters
    /// 1 : Imperial Feet
    /// 2 : US Survey Feet
    /// </summary>
    [JsonProperty(PropertyName = "DxfUnitsTypeID", Required = Required.Default)]
    public DxfUnitsType DxfUnitsTypeId { get; set; }
  }

  public class AlignmentFile
  {
    /// <summary>
    /// Gets or sets the centerline offset.
    /// </summary>
    [JsonProperty(PropertyName = "Offset", Required = Required.Default)]
    public double Offset { get; set; }
  }


  public class ImportedFileTbc : BusinessCenterFile
  {
    /// <summary>
    /// Gets or sets the file's type ID.  Values are:
    /// 0 : Linework
    /// 1 : Design Surface
    /// 2 : Surveyed Surface
    /// 3 : Alignment
    /// 4 : MobileLinework
    /// 7 : MassHaulPlan
    /// </summary>
    /// 
    /// 
    [JsonProperty(PropertyName = "ImportedFileTypeID", Required = Required.Always)]
    public ImportedFileType ImportedFileTypeId { get; set; }

    /// <summary>
    /// Alignment.offset: Gets or sets the centerline offset.
    /// </summary>
    [JsonProperty(PropertyName = "AlignmentFile", Required = Required.Default)]
    public AlignmentFile AlignmentFile { get; set; }
    
    /// <summary>
    /// surfacefile.surveyedUTC: Gets or sets the surveyed UTC date/time.
    /// </summary>
    [JsonProperty(PropertyName = "SurfaceFile", Required = Required.Default)]
    public SurfaceFile SurfaceFile { get; set; }

    /// <summary>
    /// LineworkFile.dxfUnitsTypeID:  Gets or sets the distance units. Values are:
    /// 0 : Meters
    /// 1 : Imperial Feet
    /// 2 : US Survey Feet
    /// </summary>
    [JsonProperty(PropertyName = "LineworkFile", Required = Required.Default)]
    public LineworkFile LineworkFile { get; set; }

    [Obsolete]
    [JsonProperty(PropertyName = "MassHaulPlanFile", Required = Required.Default)]
    public string MassHaulPlanFile { get; set; }
    
  }
}

