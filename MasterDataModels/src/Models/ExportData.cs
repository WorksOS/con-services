using System;
using System.Collections.Generic;
using System.Text;

namespace VSS.MasterData.Models.Models
{
  /// <summary>
  /// Container for export data
  /// </summary>
  /// <seealso cref="VSS.MasterData.Models.Models.BaseDataResult" />
  public class ExportData : BaseDataResult
  {
    /// <summary>
    /// Gets or sets the data.
    /// </summary>
    /// <value>
    /// The data.
    /// </value>
    public byte[] Data { get; set; }
  }
}