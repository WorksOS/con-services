using System;
using System.IO;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models
{
  public class PondingRequest
  {
    /// <summary>A project unique identifier.</summary>
    [JsonProperty(PropertyName = "projectUid", Required = Required.Default)]
    public Guid ProjectUid { get; set; }

    /// <summary>Filter may contain either: 1 DesignBoundary or GeofenceBoundary (else project boundary is used)</summary>
    [JsonProperty(PropertyName = "filterUid", Required = Required.Default)]
    public Guid? FilterUid { get; set; }

    ///// <summary>The path and filename of the dxf file containing the original surface mesh.</summary>
    //[JsonProperty(PropertyName = "SurfaceFileName", Required = Required.Default)]
    //public string SurfaceFileName { get; protected set; }

    /// <summary>The nominal distance between irrigation rows.</summary>
    [JsonProperty(PropertyName = "Resolution", Required = Required.Default)]
    public double Resolution { get; set; }

    /// <summary>
    /// The minimum slope. Use <c>NaN</c> for undefined.
    /// </summary>
    [JsonProperty(PropertyName = "MinSlope", Required = Required.Default)]
    public double MinSlope { get; set; }

    /// <summary>The maximum slope. Use <c>NaN</c> for undefined.</summary>
    [JsonProperty(PropertyName = "MaxSlope", Required = Required.Default)]
    public double MaxSlope { get; set; }

    /// <summary>The maximum slope change along the irrigation rows. Use <c>NaN</c> for undefined.</summary>
    [JsonProperty(PropertyName = "MaxSlopeChange", Required = Required.Default)]
    public double MaxSlopeChange { get; set; }

    ///// <summary>Specifies the absolute direction of the furrows comming out of the pipeline (0 degrees is north and increasing clockwise).</summary>
    //[JsonProperty(PropertyName = "FurrowHeading", Required = Required.Default)]
    //public double FurrowHeading { get; set; }

    ///// <summary>Specifies the absolute direction of the main axis of grid in subzones computation (0 degrees is north and increasing clockwise).</summary>
    //[JsonProperty(PropertyName = "MainHeading", Required = Required.Default)]
    //public double MainHeading { get; set; }

    /// <summary>Specifies the minimum cross-slope as a fraction: vertical/horizontal.</summary>
    [JsonProperty(PropertyName = "MinCrossSlope", Required = Required.Default)]
    public double MinCrossSlope { get; set; }

    /// <summary>Specifies the maximum cross-slope as a fraction: vertical/horizontal.</summary>
    [JsonProperty(PropertyName = "MaxCrossSlope", Required = Required.Default)]
    public double MaxCrossSlope { get; set; }

    /// <summary>Specifies the maximum cross-slope change: previous cross-slope - current cross-slope.</summary>
    [JsonProperty(PropertyName = "MaxCrossSlopeChange", Required = Required.Default)]
    public double MaxCrossSlopeChange { get; set; }

    /// <summary>Specifies the maximum cut that can be applied to the original surface.</summary>
    [JsonProperty(PropertyName = "MaxCutDepth", Required = Required.Default)]
    public double MaxCutDepth { get; set; }

    /// <summary>Specifies the maximum fill height that can be applied to the original surface.</summary>
    [JsonProperty(PropertyName = "MaxFillHeight", Required = Required.Default)]
    public double MaxFillHeight { get; set; }

    ///// <summary>Specifies whether first column is the point id.</summary>
    //[JsonProperty(PropertyName = "HasPointIds", Required = Required.Default)]
    //public bool HasPointIds { get; set; }

    /// <summary>Specifies whether the order of the columns is X, Y, Z or Y, X, Z.</summary>
    [JsonProperty(PropertyName = "IsXYZ", Required = Required.Default)]
    public bool IsXYZ { get; set; }

    /// <summary>Specifies whether the unit of length is meter or feet.</summary>
    [JsonProperty(PropertyName = "IsMetric", Required = Required.Default)]
    public bool IsMetric { get; set; }

    ///// <summary>The percentage volume reduction between bank cut and bank fill volumes.</summary>
    //[JsonProperty(PropertyName = "Shrinkage", Required = Required.Default)]
    //public double Shrinkage { get; set; }

    ///// <summary>The percentage volume explantion between bank cut and loose haul volumes.</summary>
    //[JsonProperty(PropertyName = "Bulkage", Required = Required.Default)]
    //public double Bulkage { get; set; }

    ///// <summary>Computation type.</summary>
    //[JsonProperty(PropertyName = "IsMetric", Required = Required.Default)]
    //public ComputeEnum Compute { get; set; }

    ///// <summary>
    ///// This is a closed linestring that limits the scope of the computation to the surface within the boundary.
    ///// </summary>
    //public Linestring Boundary { get; set; }

    ///// <summary>
    ///// This is an open linestring. Every triangle should face towards the closest point in the ditch polyline within 60 degrees.
    ///// </summary>
    //public Linestring TargetDitch
    //{
    //  get
    //  {
    //    return this.TargetDitches.FirstOrDefault<Linestring>();
    //  }
    //  set
    //  {
    //    if (value == null)
    //      throw new ArgumentNullException();
    //    if (!value.Points.Any<Point>())
    //      return;
    //    this.TargetDitches = Enumerable.Repeat<Linestring>(value, 1).ToList<Linestring>();
    //  }
    //}

    ///// <summary>The list of target ditches.</summary>
    //[XmlArray("TargetDitches")]
    //[XmlArrayItem("Ditch")]
    //public List<Linestring> TargetDitches { get; set; }

    ///// <summary>
    ///// This is an open linestring that represents the pipepline. Furrows originate from the linestring.
    ///// </summary>
    //public Linestring Pipeline { get; set; }

    ///// <summary>The list of section lines.</summary>
    //[XmlArray("SectionLines")]
    //[XmlArrayItem("SectionLine")]
    //public List<Linestring> SectionLines { get; set; }

    ///// <summary>Section areas.</summary>
    //[XmlArray("Sections")]
    //[XmlArrayItem("PlanesConstraints")]
    //public List<PlanesConstraints> Sections { get; set; }

    ///// <summary>Exclusion zones.</summary>
    //[XmlArray("ExclusionZones")]
    //[XmlArrayItem("Zone")]
    //public List<Linestring> ExclusionZones { get; set; }

    ///// <summary>Subzone constraints.</summary>
    //[XmlArray("Zones")]
    //[XmlArrayItem("SubzoneConstraints")]
    //public List<SubzoneConstraints> Zones { get; set; }

    ///// <summary>Drainage area constraints.</summary>
    //[XmlArray("Areas")]
    //[XmlArrayItem("AreaConstraints")]
    //public List<AreaConstraints> Areas { get; set; }

    ///// <summary>
    ///// The resulting export loose haul volume to produce: cut - fill.
    ///// </summary>
    //[XmlAttribute]
    //public double ExportVolume { get; set; }

    ///// <summary>The exit point in basin computations.</summary>
    //public Point3D ExitPoint { get; set; }

    ///// <summary>
    ///// List of vizualization tools applied to the original surface.
    ///// </summary>
    //[XmlArray("OriginalVisualizationTools")]
    //[XmlArrayItem(typeof(OmniSlope))]
    //[XmlArrayItem(typeof(DirectionalSlope))]
    //[XmlArrayItem(typeof(WaterShed))]
    //[XmlArrayItem(typeof(PondMap))]
    //[XmlArrayItem(typeof(DrainageViolations))]
    //public VizTool[] OriginalVisualizationTools { get; set; }

    ///// <summary>
    ///// List of vizualization tools applied to the design surface.
    ///// </summary>
    //[XmlArray("DesignVisualizationTools")]
    //[XmlArrayItem(typeof(OmniSlope))]
    //[XmlArrayItem(typeof(DirectionalSlope))]
    //[XmlArrayItem(typeof(WaterShed))]
    //[XmlArrayItem(typeof(PondMap))]
    //[XmlArrayItem(typeof(DrainageViolations))]
    //public VizTool[] DesignVisualizationTools { get; set; }


    public PondingRequest()
    {
      Initialize();
    }

    private void Initialize()
    {
      //SurfaceFileName = string.Empty;
      Resolution = double.NaN;
      MinSlope = double.NaN;
      MaxSlope = double.NaN;
      MaxSlopeChange = double.NaN;
      //FurrowHeading = double.NaN;
      //MainHeading = double.NaN;
      MinCrossSlope = double.NaN;
      MaxCrossSlope = double.NaN;
      MaxCrossSlopeChange = double.NaN;
      MaxCutDepth = double.NaN;
      MaxFillHeight = double.NaN;
      //HasPointIds = true;
      IsXYZ = true;
      IsMetric = true;
      //Shrinkage = 0.0;
      //Bulkage = 0.0;
      //Compute = ComputeEnum.None;
      //Boundary = new Linestring();
      //SectionLines = (List<Linestring>)null;
      //Sections = new List<PlanesConstraints>();
      //TargetDitches = Enumerable.Repeat<Linestring>(new Linestring(), 1).ToList<Linestring>();
      //Pipeline = new Linestring();
      //ExclusionZones = new List<Linestring>();
      //Zones = new List<SubzoneConstraints>();
      //Areas = new List<AreaConstraints>();
      //ExportVolume = double.NaN;
      //ExitPoint = Utils.EmptyPoint3D;
    }

    public PondingRequest(string surfaceFileName, double resolution = 1, // meters/pixel
      bool isXYZ = false, bool isMetric = true
      )
    {
      Initialize();
      //SurfaceFileName = surfaceFileName;
      Resolution = resolution;
      IsXYZ = isXYZ;
      IsMetric = isMetric;
    }

    public void Validate()
    {
      if (ProjectUid == Guid.Empty)
      {
        //throw Exception("Invalid project UID.");
      }

      //// todo could be:
      //// a) on A3
      //// b) further params to define call to TRex get design e.g. projectUid; area
      //// Also potentially convert one file format to dxf mesh or whatever drainage libraries take
      //if (!File.Exists(SurfaceFileName))
      //{
      //  throw new ServiceException(HttpStatusCode.BadRequest,
      //    new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
      //      "Unable to locate the surface Design file"));
      //}
    }
  }
}

