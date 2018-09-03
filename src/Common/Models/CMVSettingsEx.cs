using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Common.Models
{
  /// <summary>
  /// The parameters for CMV detailed and summary computations
  /// </summary>
  public class CMVSettingsEx : CMVSettings
  {
    /// <summary>
    /// The collection of CMV targets. Values are in ascending order.
    /// There must be 5 values and the first value must be 0.
    /// This property is not used for a summary report only for a detailed report.
    /// </summary>
    [JsonProperty(PropertyName = "customCMVDetailTargets", Required = Required.Always)]
    public int[] CustomCMVDetailTargets { get; private set; }

    /// <summary>
    /// Default private constructor
    /// </summary>
    private CMVSettingsEx()
    {
    }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>
    /// <param name="cmvTarget"></param>
    /// <param name="maxCMV"></param>
    /// <param name="maxCMVPercent"></param>
    /// <param name="minCMV"></param>
    /// <param name="minCMVPercent"></param>
    /// <param name="overrideTargetCMV"></param>
    /// <param name="customCMVDetailTargets"></param>
    public CMVSettingsEx
    (
      short cmvTarget,
      short maxCMV,
      double maxCMVPercent,
      short minCMV,
      double minCMVPercent,
      bool overrideTargetCMV,
      int[] customCMVDetailTargets
    )
    {
      CmvTarget = cmvTarget;
      MaxCMV = maxCMV;
      MaxCMVPercent = maxCMVPercent;
      MinCMV = minCMV;
      MinCMVPercent = minCMVPercent;
      OverrideTargetCMV = overrideTargetCMV;
      CustomCMVDetailTargets = customCMVDetailTargets;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      // Validate custom CMV Detail targets...
      if (CustomCMVDetailTargets == null || CustomCMVDetailTargets.Length == 0)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMV Detail targets required"));
      }
      if (CustomCMVDetailTargets[0] != MIN_CMV)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"CMV Detail targets must start at {MIN_CMV}"));
      }
      for (int i = 1; i < CustomCMVDetailTargets.Length; i++)
      {
        if (CustomCMVDetailTargets[i] <= CustomCMVDetailTargets[i - 1])
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "CMV Detail targets must be ordered from lowest to the highest"));
        }
      }
      if (CustomCMVDetailTargets[CustomCMVDetailTargets.Length - 1] < MIN_CMV || CustomCMVDetailTargets[CustomCMVDetailTargets.Length - 1] > MAX_CMV)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"CMV Detail targets must be between {MIN_CMV + 1} and {MAX_CMV}"));
      }
    }

  }
}
