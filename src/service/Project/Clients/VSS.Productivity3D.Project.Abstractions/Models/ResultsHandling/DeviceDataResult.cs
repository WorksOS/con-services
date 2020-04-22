using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  public class DeviceDataResult : BaseDataResult, IMasterDataModel
  {
    private DeviceData _deviceDescriptor { get; set; }

    public DeviceDataResult()
    { }

    /// <summary>
    /// Gets or sets the project descriptor.
    /// </summary>
    /// <value>
    /// The project descriptor.
    /// </value>
    public DeviceData DeviceDescriptor { get { return _deviceDescriptor; } set { _deviceDescriptor = value; } }

    public List<string> GetIdentifiers() => new List<string>()
    {
      _deviceDescriptor.CustomerUID.ToString(),
      _deviceDescriptor.DeviceUID.ToString()
    };
  }
}
