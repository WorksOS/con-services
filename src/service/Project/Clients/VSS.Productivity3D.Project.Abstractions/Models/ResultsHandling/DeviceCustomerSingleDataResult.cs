using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.Models;

namespace VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling
{
  /// <summary>
  ///   Describes VL customer
  /// </summary>
  public class DeviceCustomerSingleDataResult : BaseDataResult, IMasterDataModel
  {
    /// <summary>
    /// Gets or sets the customer uid.
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    public string uid { get; set; }

    /// <summary>
    /// Gets or sets the customer name.
    /// </summary>
    /// <value>
    /// The customer name.
    /// </value>
    public string name { get; set; }

    /// <summary>
    /// Gets or sets the relationship between device and this customer
    /// </summary>
    /// <value>
    /// "PENDING" or "ACTIVE" (yes upper case from cws)
    /// </value>
    public string relationStatus { get; set; }

    /// <summary>
    /// Gets or sets the deviceUid
    /// </summary>
    /// <value>
    /// null if device is not associated to this account
    /// </value>
    public string deviceUid { get; set; }

    /// <summary>
    /// Gets or sets the customer uid.
    /// </summary>
    /// <value>
    /// "Pending"or "Registered" (yes camel case from cws)
    ///  null if device is not associated to this account
    /// </value>
    public string deviceStatus { get; set; }

    public List<string> GetIdentifiers() => new List<string>
    {
      uid
    };
  }
}
