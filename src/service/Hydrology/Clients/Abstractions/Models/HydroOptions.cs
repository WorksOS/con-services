using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Hydrology.WebApi.Abstractions.Models
{
  public class HydroOptions
  {
    private const int DEFAULT_RESOLUTION = 1;
    private const int DEFAULT_PONDING_LEVELS = 10;
    private const double DEFAULT_MINSLOPE = 0.05;
    private const double DEFAULT_MAXSLOPE = 0.1;
    private const string DEFAULT_COLOR_VORTEX_VIOLATION = "Khaki";
    private const string DEFAULT_COLOR_MAX_SLOPE_VIOLATION = "IndianRed";
    private const string DEFAULT_COLOR_NO_VIOLATION_DARK = "#2FE23E";
    private const string DEFAULT_COLOR_NO_VIOLATION_MID = "#5DEF69";
    private const string DEFAULT_COLOR_NO_VIOLATION_LIGHT = "#99F7A1";
    private const string DEFAULT_COLOR_MIN_SLOPE_VIOLATION = "LightBlue";

    /// <summary>
    /// PondMap & DrainageViolations: Resolution: the cell size to use for computation
    /// </summary>
    [JsonProperty(PropertyName = "Resolution", Required = Required.Default)]
    public double Resolution { get; set; } = DEFAULT_RESOLUTION;

    /// <summary>
    /// PondMap: Levels: The number of color levels [2,240] - we default to 10
    /// </summary>
    [JsonProperty(PropertyName = "Levels", Required = Required.Default)]
    public int Levels { get; set; } = DEFAULT_PONDING_LEVELS;

    /// <summary>
    /// DrainageViolations: MinSlope: the value of the minimum slope: lower slopes generate transparent pixels
    /// </summary>
    [JsonProperty(PropertyName = "MinSlope", Required = Required.Default)]
    public double MinSlope { get; set; } = DEFAULT_MINSLOPE;

    /// <summary>
    /// DrainageViolations: MaxSlope: the value of the maximum slope: higher slopes generate transparent pixels
    /// </summary>
    [JsonProperty(PropertyName = "MaxSlope", Required = Required.Default)]
    public double MaxSlope { get; set; } = DEFAULT_MAXSLOPE;

    /// <summary>
    /// DrainageViolations: VortexColor: the color to use when slope is lower than minimum: default yellow.
    /// </summary>
    [JsonIgnore]
    public string VortexViolationColor { get; set; } = DEFAULT_COLOR_VORTEX_VIOLATION;

    /// <summary>
    /// DrainageViolations: MaxSlopeColor: the color to use when slope is higher than minimum: default red
    /// </summary>
    [JsonIgnore]
    public string MaxSlopeViolationColor { get; set; } = DEFAULT_COLOR_MAX_SLOPE_VIOLATION;

    /// <summary>
    /// DrainageViolations: NoViolationColor: the color to use when slope is within range: default green
    ///       This comes in 3 shades todoJeannie what do these mean?
    /// </summary>
    [JsonIgnore]
    public string NoViolationColorDark { get; set; } = DEFAULT_COLOR_NO_VIOLATION_DARK;

    [JsonIgnore] public string NoViolationColorMid { get; set; } = DEFAULT_COLOR_NO_VIOLATION_MID;

    [JsonIgnore] public string NoViolationColorLight { get; set; } = DEFAULT_COLOR_NO_VIOLATION_LIGHT;

    /// <summary>
    /// DrainageViolations: MinSlopeColor: the color to use when slope is lower than minimum: default blue
    /// </summary>
    [JsonIgnore]
    public string MinSlopeViolationColor { get; set; } = DEFAULT_COLOR_MIN_SLOPE_VIOLATION;

    /****** following boundary and zones are not supported at present ****/
    /// <summary>
    /// DrainageViolations: Boundary: area to include . at present this is the design boundary
    /// </summary>
    [JsonIgnore]
    public List<Point> Boundary { get; set; } = null;

    /// <summary>
    /// DrainageViolations: InclusionZones: This list specifies the constraints for each subzone. Each subzone has a boundary and constraint parameters.
    /// </summary>
    [JsonIgnore]
    public List<List<Point>> InclusionZones { get; set; } = null;

    /// <summary>
    /// DrainageViolations: ExclusionZones: This is a list of closed linestring. Each linestring is an exclusion zone. It most be completely contained within the boundary.
    /// </summary>
    [JsonIgnore]
    public List<List<Point>> ExclusionZones { get; set; } = null;


    public HydroOptions()
    {
      Initialize();
    }

    private void Initialize()
    {
      Resolution = DEFAULT_RESOLUTION;
      Levels = DEFAULT_PONDING_LEVELS;
      MinSlope = DEFAULT_MINSLOPE;
      MaxSlope = DEFAULT_MAXSLOPE;
      VortexViolationColor = DEFAULT_COLOR_VORTEX_VIOLATION;
      MaxSlopeViolationColor = DEFAULT_COLOR_MAX_SLOPE_VIOLATION;
      NoViolationColorDark = DEFAULT_COLOR_NO_VIOLATION_DARK;
      NoViolationColorMid = DEFAULT_COLOR_NO_VIOLATION_MID;
      NoViolationColorLight = DEFAULT_COLOR_NO_VIOLATION_LIGHT;
      MinSlopeViolationColor = DEFAULT_COLOR_MIN_SLOPE_VIOLATION;
    }

    public HydroOptions(double resolution = DEFAULT_RESOLUTION, int levels = DEFAULT_PONDING_LEVELS,
      double minSlope = DEFAULT_MINSLOPE, double maxSlope = DEFAULT_MAXSLOPE,
      string vortexViolationColor = DEFAULT_COLOR_VORTEX_VIOLATION,
      string maxSlopeViolationColor = DEFAULT_COLOR_MAX_SLOPE_VIOLATION,
      string noViolationColorDark = DEFAULT_COLOR_NO_VIOLATION_DARK,
      string noViolationColorMid = DEFAULT_COLOR_NO_VIOLATION_MID,
      string noViolationColorLight = DEFAULT_COLOR_NO_VIOLATION_LIGHT,
      string minSlopeViolationColor = DEFAULT_COLOR_MIN_SLOPE_VIOLATION
    )
    {
      Initialize();
      Resolution = resolution;
      Levels = levels;
      MinSlope = minSlope;
      MaxSlope = maxSlope;
      VortexViolationColor = vortexViolationColor;
      MaxSlopeViolationColor = maxSlopeViolationColor;
      NoViolationColorDark = noViolationColorDark;
      NoViolationColorMid = noViolationColorMid;
      NoViolationColorLight = noViolationColorLight;
      MinSlopeViolationColor = minSlopeViolationColor;
    }

    public void Validate()
    {
      if (Resolution <= 0.005 || Resolution > 1000000) // todoJeannie what should these be?
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2004, "Resolution must be between 0.005 and < 1,000,000."));
      }

      if (Levels < 2 || Levels > 240)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2008, "Levels must be between 2 and 240."));
      }

      if (MinSlope < 0.005 || MinSlope > 100.0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2018, "MinSlope must be between 0.005 and 100.0.")); // todoJeannie
      }

      if (MaxSlope < 0.005 || MaxSlope > 100.0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2019, "MaxSlope must be between 0.005 and 100.0.")); // todoJeannie
      }

      if (MaxSlope <= MinSlope)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2020, "MaxSlope must be greater than MinSlope."));
      }

      if (string.IsNullOrEmpty(VortexViolationColor) /* todoJeannie validate color string content */)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2021, $"VortexViolationColor must be a valid color."));
      }

      if (string.IsNullOrEmpty(MaxSlopeViolationColor) /* todoJeannie validate color string content */)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2021, $"MaxSlopeViolationColor must be a valid color."));
      }

      if (string.IsNullOrEmpty(NoViolationColorDark) /* todoJeannie validate color string content */)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2021, $"NoViolationColorDark must be a valid color."));
      }

      if (string.IsNullOrEmpty(NoViolationColorMid) /* todoJeannie validate color string content */)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2021, $"NoViolationColorMid must be a valid color."));
      }

      if (string.IsNullOrEmpty(NoViolationColorLight) /* todoJeannie validate color string content */)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2021, $"NoViolationColorLight must be a valid color."));
      }

      if (string.IsNullOrEmpty(MinSlopeViolationColor) /* todoJeannie validate color string content */)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(2021, $"MinSlopeViolationColor must be a valid color."));
      }
    }
  }
}

