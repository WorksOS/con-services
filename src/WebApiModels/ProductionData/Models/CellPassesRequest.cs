
using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.Models
{
  /// <summary>
  /// CellPassesRequest
  /// </summary>
  public class CellPassesRequest : ProjectID, IValidatable
  {
 
    /// <summary>
    /// Location of the cell in the form of cartesian cell index address. 
    /// May be null.
    /// </summary>       
    [JsonProperty(PropertyName = "cellAddress", Required = Required.Default)]
    public CellAddress cellAddress { get; private set; }

    /// <summary>
    /// Location of the cell in the form of a grid position within it. 
    /// May be null.
    /// </summary>   
    [JsonProperty(PropertyName = "probePositionGrid", Required = Required.Default)]
    public Point probePositionGrid { get; private set; }

    /// <summary>
    /// Location of the cell in the form of a WGS84 position within it. 
    /// May be null.
    /// </summary>       
    [JsonProperty(PropertyName = "probePositionLL", Required = Required.Default)]
    public WGSPoint probePositionLL { get; private set; }

    /// <summary>
    /// The lift/layer build settings to be used.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]    
    public LiftBuildSettings liftBuildSettings { get; private set; }

    /// <summary>
    /// The type of data being requested for the processed passes and layers to represent.
    /// Defined types are as follows:
    ///  icdtAll = $00000000;
    ///  icdtCCV = $00000001;
    ///  icdtHeight = $00000002;
    ///  icdtLatency = $00000003;
    ///  icdtPassCount = $00000004;
    ///  icdtFrequency = $00000005;
    ///  icdtAmplitude = $00000006;
    ///  icdtMoisture = $00000007;
    ///  icdtTemperature = $00000008;
    ///  icdtRMV = $00000009;
    ///  icdtCCVPercent = $0000000B;
    ///  icdtGPSMode = $0000000A;
    ///  icdtSimpleVolumeOverlay = $0000000C;
    ///  icdtHeightAndTime = $0000000D;
    ///  icdtCompositeHeights = $0000000E;
    ///  icdtMDP = $0000000F;
    ///  icdtMDPPercent = $00000010;
    ///  icdtCellProfile = $00000011;
    ///  icdtCellPasses = $00000012;
    /// </summary>
    //TODO Move this to a new ENUM
    [JsonProperty(PropertyName = "gridDataType", Required = Required.Always)]
    [Required]
    public int gridDataType { get; private set; }

    /// <summary>
    /// The ID of the filter to be used. 
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterId", Required = Required.Default)]
    public long? filterId { get; private set; }

    /// <summary>
    /// The lift/layer build settings to be used.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public Filter filter { get; private set; }

    /// <summary>
    /// Validation of CellPassRequest
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      if (this.cellAddress == null && this.probePositionGrid == null && this.probePositionLL == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
              new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "cellAddress, probePositionGrid, probePositionLL one must be set"));
      }


    //  throw new NotImplementedException();
    }

    /// <summary>
    /// Create instance of CellPassRequest
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="cellAddress"></param>
    /// <param name="probePositionGrid"></param>
    /// <param name="probePositionLL"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="gridDataType"></param>
    /// <param name="filterId"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static CellPassesRequest CreateCellPassRequest(long projectId, CellAddress cellAddress, Point probePositionGrid,
     WGSPoint probePositionLL, LiftBuildSettings liftBuildSettings, int gridDataType, long filterId ,Filter filter )
    {
      return new CellPassesRequest
      {
        projectId = projectId,cellAddress = cellAddress,probePositionGrid = probePositionGrid,probePositionLL = probePositionLL,
        liftBuildSettings = liftBuildSettings, gridDataType = gridDataType, filterId = filterId,filter = filter
      };
    }

    /// <summary>
    /// Create example instance of CellPassesRequest to display in Help documentation.
    /// </summary>
    public new static CellPassesRequest HelpSample
    {
      get
      {
        return new CellPassesRequest()
        {
            projectId = 100,
            cellAddress = CellAddress.HelpSample,
            filter = Filter.HelpSample,
            filterId = 132,
            gridDataType = 1,
            liftBuildSettings = LiftBuildSettings.HelpSample,
            probePositionGrid = Point.HelpSample,
            probePositionLL = WGSPoint.HelpSample,
        };
      }
    }

  }
}