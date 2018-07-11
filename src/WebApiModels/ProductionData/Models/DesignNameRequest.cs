using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Models
{
  /// <summary>
  /// Request to do operations on design file in design cache
  /// </summary>
  public class DesignNameRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// Description to identify a design file in DesignCache.
    /// </summary>
    /// 
    [JsonProperty(PropertyName = "DesignFilename", Required = Required.Always)]
    [Required]
    public string DesignFilename { get; private set; }

    public override void Validate()
    {
      base.Validate();
    }
  }
}
